using System;
using UnityEngine;
using Il2CppEntities.Stats;
using Il2CppEntities.Players;
using TombstoneAI.Library.Utilities;
using Il2CppInventory.Items.Equipment;
using Il2CppTileObjects;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Il2CppTileObjects.Crafting;
using static UnityEngine.GraphicsBuffer;
using System.Runtime.Remoting.Messaging;
using Il2CppTileObjects.Npcs;
using Il2CppTileObjects.Crafting.Recipes;
using static TombstoneAI.Test.Hooks;
using Il2CppUI.Crafting;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.InteropServices;
using MelonLoader;
using Il2CppUI;
using static TombstoneAI.Library.Datastub;
using Il2CppEntities.Enemies;
using Il2CppTileObjects.ItemDrops;
namespace TombstoneAI.Library
{
    public class Player
    {
        Bank bank;
        bool _isActive=true;
        Inventory inventory;
        UiCraftingSlot slot;
        private Timer runTimer;
        private Timer walkTimer;
        private Timer craftTimer;
        private Timer getRecipeTimer;
        Il2CppEntities.Players.Player player;
        static int MINIMUM_STAMINA_FOR_RUNNING=50;
        GameObject go=new GameObject("DummyCraftSlot");
        static readonly object _lockMinStamRun=new object();
        public static void SET_MINIMUM_STAMINA_FOR_RUNNING(int stamina){
            if(stamina<0)stamina=0;else
            if(stamina>100)stamina=100;
            lock(_lockMinStamRun)MINIMUM_STAMINA_FOR_RUNNING=stamina;
        }
        public static int GET_MINIMUM_STAMINA_FOR_RUNNING(){
            lock(_lockMinStamRun)return(MINIMUM_STAMINA_FOR_RUNNING);
        }
        public Player(Il2CppEntities.Players.Player player){
            if(player==null){
                this.player=null;
                this._isActive=false;
            }else{
                this.slot=go.AddComponent<UiCraftingSlot>();
                this.inventory=new Inventory(player);
                this.bank=new Bank(player,inventory);
                this.player=player;
            }
        }
        public Bank getBank(){return(this.bank);}
        public bool isActive(){return(this._isActive);}
        public Inventory getInventory(){return(this.inventory);}
        public bool canRun(){return(this.player.Stamina.CanSprint);}
        public bool canWalk(){return(this.player.Controller.CanMove());}
        public bool isRunning(){return(this.player.Controller.IsSprinting);}
        public PlayerEquipment getEquipment(){return(this.player.Equipment);}
        public Il2CppEntities.Players.Player basePlayer(){return(this.player);}
        public bool isToolEquipped(ToolFlags type){return(this.player.HasToolEquipped(type));}
        
        public int getMaxHealth(){return(int)this.player.Health.sync___get_value__maxHealth();}
        public int getHealth(){return(int)this.player.Health.sync___get_value__currentHealth();}
        public int getStamina(){return(int)(this.player.Stamina.sync___get_value__stamina()*100);}
        public bool canWalkTo(Position position){return(this.player.Controller.IsMoveValid(this.player.Controller.sync___get_value_curPos(),new Vector2Int(position.x,position.y),this.getTick(true)));}
        public MoveData getMoveData(Position position){
            bool isTileObject;
            MoveData R=this.player.Controller.GetClickMoveData(new Vector2(position.x,position.y),out isTileObject);
            return(R);
        }
        public uint getTick(){
            return(Il2CppTickManagement.TickManager.GameTick);
        }
        public Il2CppSystem.Nullable<uint>getTick(bool nullable=true){
            var R=new Il2CppSystem.Nullable<uint>();
            R.value=Il2CppTickManagement.TickManager.GameTick;
            return(R);
        }
        public Position getPosition(){
            var pos=this.player.Controller.sync___get_value_curPos();
            return new Position(pos.x,pos.y);
        }
        public enum Animation{
            WOODCUTTING,
            CRAFTING,
            WALK,
            SWIM,
            NONE,
            IDLE,
        }
        public Animation getAnimation(){
            var R=Il2CppEntities.AnimationConfig.HashToName(this.player.PlayerVisuals.CurAnimHash);
            if(R=="SkillingSwing")return(Animation.WOODCUTTING);else
            if(R=="Inv craft")return(Animation.CRAFTING);else
            if(R=="Walk")return(Animation.WALK);else
            if(R=="Swim")return(Animation.SWIM);else
            if(R=="Idle")return(Animation.IDLE);else
            return(Animation.NONE);
        }
        public Stat getCurrentSkill(){
            return(player.Controller.SkillingState);
        }
        public string getRawAnimation(){
            return(Il2CppEntities.AnimationConfig.HashToName(this.player.PlayerVisuals.CurAnimHash));
        }
        public bool isNear(Position position,int maxDistance=1){
            return(this.getPosition().Distance(position)<=maxDistance);
        }
        public bool isNear(Vector2Int position,int maxDistance=1){
            return(this.isNear(new Position(position.x,position.y),maxDistance));
        }
        private Position _getNearest(Position target,HashSet<Position>exclude=null){
            Position current=this.getPosition();
            float bestDist=float.MaxValue;
            Position best=current;
            for(int dx=-1;dx<=1;dx++){
                for(int dy=-1;dy<=1;dy++){
                    if(dx==0&&dy==0)continue;
                    Position p=new Position(current.x+dx,current.y+dy);
                    if(exclude!=null&&exclude.Contains(p))continue;
                    float dist=Mathf.Abs(target.x-p.x)+Mathf.Abs(target.y-p.y);///Manhattan distance
                    if(dist<bestDist){
                        bestDist=dist;
                        best=p;
                    }
                }
            }
            return(best);
        }
        public void walk(Position position,Action callback=null,bool force=false,int maxDistance=1){
            this.player.Controller.ClearPath();
            this.player.Controller.ToggleSprint=false;
            if(this.walkTimer!=null)this.walkTimer.Stop();
            this.player.Controller.CspFindPath(new Vector2(position.x,position.y),true,force);
            this.player.Controller.FollowPathCache();
            this.walkTimer=null;
            this.walkTimer=new Timer(()=>{
                if(!this.isNear(position,maxDistance)){
                    if(this.getAnimation()!=Animation.WALK){
                        Action c=(Action)this.walkTimer.GetValue("callback");
                        bool f=(bool)this.walkTimer.GetValue("force");
                        this.walk(position,c,f);
                        this.walkTimer.Stop();
                    }
                }else{
                    ((Action)this.walkTimer.GetValue("callback"))();
                    this.walkTimer.Stop();
                }
            },1E3,true);
            this.walkTimer.SetValue("callback",callback);
            this.walkTimer.SetValue("force",force);
            this.walkTimer.Start();
        }
        public void run(Position position,Action callback=null,bool force=false,int maxDistance=1){
            this.player.Controller.ClearPath();
            if(this.runTimer!=null)this.runTimer.Stop();
            this.player.Controller.ToggleSprint=this.canRun()&&this.getStamina()>=MINIMUM_STAMINA_FOR_RUNNING;
            this.player.Controller.CspFindPath(new Vector2(position.x,position.y),true,force);
            this.player.Controller.FollowPathCache(this.player.Controller.ToggleSprint);
            this.runTimer=null;
            this.runTimer=new Timer(()=>{
                if(!this.isNear(position,maxDistance)){
                    if(this.getAnimation()!=Animation.WALK){
                        Action c=(Action)this.runTimer.GetValue("callback");
                        bool f=(bool)this.runTimer.GetValue("force");
                        this.run(position,c,f);
                        this.runTimer.Stop();
                    }
                }else{
                    ((Action)this.runTimer.GetValue("callback"))();
                    this.runTimer.Stop();
                }
            },1E3,true);
            this.runTimer.SetValue("callback",callback);
            this.runTimer.SetValue("force",force);
            this.runTimer.Start();
        }
        public void attack(Enemy enemy){
            var inter=new IInteractable(enemy.Pointer);
            var move=new MoveData(enemy.TilePosition,inter);
            this.player.Controller.ClientSendMoveRequest(move);
        }
        public void loot(ItemDrop loot){
            var inter=new IInteractable(loot.Pointer);
            var move=new MoveData(loot.TilePosition,inter);
            this.player.Controller.ClientSendMoveRequest(move);
        }
        public void interact(Position position,int dialogueNodeID=0){
            var R=this.getMoveData(position);
            R.DialogueNodeId=dialogueNodeID;
            this.player.Controller.ClientSendMoveRequest(R);
        }
        public void interact(Vector2Int position,int dialogueNodeID=0){
            this.interact(new Position(position.x,position.y),dialogueNodeID);
        }
        public void walk(Vector2Int position,Action callback=null,bool force=false,int maxDistance=1){
            this.walk(new Position(position.x,position.y),callback,force,maxDistance);
        }
        public void run(Vector2Int position,Action callback=null,bool force=false,int maxDistance=1){
            this.run(new Position(position.x,position.y),callback,force,maxDistance);
        }
        public int getLevel(string stat){
            stat=char.ToUpper(stat[0])+stat.Substring(1).ToLower();
            if(stat=="Combat")return(this.player.Stats.sync___get_value_CombatLevel());
            var statEnum=(Stat)Enum.Parse(typeof(Stat),stat);
            return(this.player.Stats[statEnum].Level);
        }
        public int getLevel(Stat stat){
            return(this.player.Stats[stat].Level);
        }
        public float getExp(string stat){
            stat=char.ToUpper(stat[0])+stat.Substring(1).ToLower();
            int id=(int)Enum.Parse(typeof(Stat),stat);
            return(this.player.Stats[(Stat)id].Exp);
        }
        public float getExp(Stat stat){
            return(this.player.Stats[stat].Exp);
        }
        public string getUsername(){
            return(this.player.Username);
        }
        public string getName(){
            return(this.player.Username);
        }
        public GameMode getGameMode(){
            return(this.player.GameMode);
        }
        public IslandType getIsland(){
            var R=this.player.Tilemapper._map.name;
            if(R=="bb")return(IslandType.Boia);else
            if(R=="ridgewood map tex")return(IslandType.Ridgewood);else
            if(R.StartsWith("Oloia"))return(IslandType.Trailhead);else
            ///if(R=="Oloia 2023_03_20_18_26 1")return(IslandType.Trailhead);else
            return(IslandType.None);
        }
        public void getRecipe(string recipeName,CraftType craftType,Action<Recipe>callback){
            var ui=GameObject.FindObjectOfType<UiManager>();
            recipeName=recipeName.ToLower();
            if(ui==null){
                callback(null);
                return;
            }
            getRecipeTimer=new Timer(()=>{
                var F=false;
                var slots=GameObject.FindObjectsOfType<UiCraftingSlot>();
                foreach(UiCraftingSlot slot in slots){
                    var A=slot?.Recipe??null;
                    if(A==null)continue;
                    var B=A.GetRecipeName().ToLower();
                    if(B==recipeName){
                        callback(A);
                        F=true;
                        break;
                    }
                }
                if(!F)callback(null);
                ui.Crafting.Close();
            },100);
            ui.OpenCraftingMenu(craftType);
            getRecipeTimer.Start();
        }
        public void craft(Recipe recipe,int quantity,CraftType craftType,Action<bool>callback,bool force=false,bool openCraftingMenu=true){
            var cb=callback;
            var guid=recipe.Guid;
            var ui=GameObject.FindObjectOfType<UiManager>();
            void call(bool c){if(cb!=null){try{cb(c);}catch(Exception){}}}
            if(ui==null){
                call(false);
                return;
            }
            craftTimer=new Timer(()=>{
                var F=false;
                try{
                    var U=GameObject.FindObjectOfType<UiManager>();
                    if(U==null){
                        MelonLogger.Msg("craft: UiManager missing in timer");
                        call(false);
                        return;
                    }
                    var slots=GameObject.FindObjectsOfType<UiCraftingSlot>();
                    foreach(UiCraftingSlot slot in slots){
                        var A=slot?.Recipe??null;
                        if(A!=null&&A.Guid==guid){
                            slot.Craft(quantity);
                            call(true);
                            F=true;
                            break;
                        }
                    }
                    U.Crafting.Close();
                    if(!F)call(false);
                }catch(Exception e){
                    if(!F)call(false);
                    return;
                }
            },300);
            if(openCraftingMenu)ui.OpenCraftingMenu(craftType);
            craftTimer.Start();
        }
        public bool canCraft(Recipe recipe,int quantity){
            return(this.player.Controller.CanCraft(recipe,quantity));
        }
        public MoveData getLastMoveData(){
            return(this.player.Controller._lastMoveData);
        }
        public bool shouldAbuelo(){
            return(this.inventory.caps()>=50)&&(this.getHealth()<=(this.getMaxHealth()/1.5)||(!this.player.Controller.ToggleSprint&&this.getStamina()<MINIMUM_STAMINA_FOR_RUNNING));
        }
        public bool allowRunIfEligible(){
            var R=this.getStamina()>=Player.GET_MINIMUM_STAMINA_FOR_RUNNING();
            if(R)this.player.Controller.ToggleSprint=true;
            return(R);
        }
    }
}