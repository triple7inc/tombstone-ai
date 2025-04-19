using System;
using UnityEngine;
using TombstoneAI.Library;
using Il2CppEntities.Stats;
using TombstoneAI.Library.Utilities;
using Il2CppTileObjects.SkillingNodes;
using static TombstoneAI.Library.Datastub;
namespace TombstoneAI.Scripts
{
    [BotInfo("Powerkiller","Powerkills enemies, loots items, harvests beehives, banks full inventories, and heals at the nearest Abuelo","0.1.9b","triple7inc",Stat.Health,Stat.Defense,Stat.Ranged)]
    public class PowerKillCombat:CombatBotScriptBase{
        Position lastFightPos;
        SkillingNode beehive;
        Position lastPos;
        bool canHarvest;
        bool attackRare;
        bool beehiver;
        bool lastBool;
        Timer gTimer;
        Timer fTimer;
        public override bool Requirements(){
            this.wait=new WaitForSeconds(0.9f);
            return(true);
        }
        public override void Start(){
            base.Start();
            state=ScriptState.RUNNING;
            Doopstrap.AlertInput("Enemies","Enter a comma-separated list of enemies to attack\nFor example: Mouse, Rat, Settler",R=>{
                if(R==null){
                    Error("There are no enemies defined");
                    return;
                }
                var A=R.Split(',');
                if(A.Length==0||(A.Length==1&&String.IsNullOrEmpty(A[0]))){
                    Error("There are no enemies defined");
                    return;
                }
                for(var x=0;x<A.Length;x++){
                    A[x]=A[x].Trim();
                }
                setEnemies(A);
                Doopstrap.Confirmation("Enemies","Do you want to attack SHINY enemies?",OK=>{
                    attackRare=OK;
                    Doopstrap.Confirmation("Loot","Do you want to harvest BEEHIVES?",OK1=>{
                        beehiver=OK1;
                        Doopstrap.AlertInput("Loot","Enter a comma-separated list of items to loot\nFor example: Caps, * Seeds, Raw *",RR=>{
                            A=RR?.Split(',')??new string[]{};
                            if(A.Length==0||(A.Length==1&&String.IsNullOrEmpty(A[0])))clearLoot();
                            else{
                                for(var x=0;x<A.Length;x++){
                                    A[x]=A[x].Trim();
                                }
                                setLoot(A);
                            }
                            state=ScriptState.IDLE;
                            if(gTimer!=null)gTimer.Stop();
                            gTimer=new Timer(()=>{
                                if(player==null||!isActive()||isPaused())return;
                                if(player.getAnimation()!=Player.Animation.IDLE||player.getCurrentSkill()!=Stat.Movement)return;
                                if(player.getPosition().Equals(lastPos)){
                                    state=ScriptState.IDLE;
                                    enemyTarget=null;
                                }
                                lastPos=player.getPosition();
                            },6E4,true);
                            gTimer.Start();
                        });
                    });
                });
            });
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
            lastFightPos=null;
            enemyTarget=null;
            lootTarget=null;
            beehive=null;
        }
        protected override void Program(){
            if(state==ScriptState.IDLE){
                beehive=null;
                lootTarget=null;
                enemyTarget=null;
                if(timer!=null)timer.Stop();
                if(fTimer!=null)fTimer.Stop();
                progression.setText("Waiting");
                if(player.getHealth()<(player.getMaxHealth()/3))return;
                progression.setText("Searching");
                enemyTarget=Datastub.Enemies.Find(player,attackRare,this.getEnemiesAsArray());
                if(enemyTarget==null)return;
                progression.setText("Attacking");
                try{
                    canHarvest=player.isToolEquipped(ToolType.Knife)||player.getInventory().hasItemPrecise("Knife");
                    state=ScriptState.TASK_6;
                    player.allowRunIfEligible();
                    player.attack(enemyTarget);
                }catch{state=ScriptState.IDLE;}
                return;
            }else
            if(state==ScriptState.RUNNING){
            }else
            if(state==ScriptState.TASK_6){
                if(enemyTarget==null&&lastFightPos!=null&&!player.isNear(lastFightPos,12)){
                    state=ScriptState.TASK_9;
                    progression.setText("Running");
                    player.run(lastFightPos,()=>{
                        state=ScriptState.IDLE;
                    },maxDistance:6);
                    return;
                }
                if(enemyTarget==null){
                    state=ScriptState.IDLE;
                    return;
                }
                lastFightPos=null;
                state=ScriptState.TASK_1;
                if(fTimer!=null)fTimer.Stop();
                fTimer=new Timer(()=>{
                    var R=!player.isRunning()&&player.getAnimation()==Player.Animation.WALK&&player.getCurrentSkill()==Stat.Movement;
                    if(R&&lastBool){
                        state=ScriptState.TASK_6;
                        fTimer.Stop();
                    }else lastBool=R;
                },12E4,true);
            }else
            if(state==ScriptState.TASK_1){
                if(player.getCurrentSkill()==Stat.Movement){
                    if(enemyTarget!=null&&enemyTarget.IsSpawned&&enemyTarget.isActiveAndEnabled&&enemyTarget.Health.CurrentHealth>=1){
                        player.attack(enemyTarget);
                        if(timer!=null)timer.Stop();
                        timer=new Timer(()=>state=ScriptState.IDLE,6E4);
                        timer.Start();
                        return;
                    }
                    enemyTarget=null;
                    if(beehiver){
                        var bh=GameObject.Find("Beehive");
                        if(bh!=null){
                            beehive=bh.GetComponent<SkillingNode>();
                            if(beehive!=null){
                                lastFightPos=player.getPosition().Copy();
                                player.interact(beehive.TilePosition);
                                progression.setText("Harvesting");
                                state=ScriptState.TASK_8;
                                return;
                            }
                        }
                    }
                    if(getLoot().Count!=0){
                        state=ScriptState.TASK_4;
                    }else
                    if(player.shouldAbuelo()){
                        state=ScriptState.TASK_2;
                    }else{
                        state=ScriptState.IDLE;
                    }
                    progression.setText("Searching");
                    lastFightPos=player.getPosition().Copy();
                }
            }else
            if(state==ScriptState.TASK_2){
                if(fTimer!=null)fTimer.Stop();
                var abuelo=Datastub.Abuelo.Find(player,60);
                if(abuelo==null)state=ScriptState.IDLE;
                else{
                    if(timer!=null)timer.Stop();
                    progression.setText("Healing");
                    player.interact(abuelo,DNID.QUICK_HEAL);
                    timer=new Timer(()=>{
                        var bank=Datastub.Banks.Find(player,30);
                        state=player.shouldAbuelo()?ScriptState.TASK_2:(bank!=null&&player.getInventory().FreeSpace()<6?ScriptState.TASK_5:ScriptState.TASK_6);
                    },1E4);
                    timer.Start();
                    state=ScriptState.TASK_3;
                }
            }else
            if(state==ScriptState.TASK_3){
                if(!player.shouldAbuelo()){
                    if(timer!=null)timer.Stop();
                    var bank=Datastub.Banks.Find(player,30);
                    state=bank!=null&&player.getInventory().FreeSpace()<6?ScriptState.TASK_5:ScriptState.TASK_6;
                }
            }else
            if(state==ScriptState.TASK_4){
                if(fTimer!=null)fTimer.Stop();
                if(player.getInventory().isFull()){
                    state=ScriptState.TASK_5;
                    return;
                }
                lootTargets=Datastub.Loot.FindAll(player,getLootAsArray());
                if(lootTargets==null){
                    progression.setText("Searching");
                    state=player.shouldAbuelo()?ScriptState.TASK_2:ScriptState.IDLE;
                }else{
                    progression.setText("Looting");
                    if(timer!=null)timer.Stop();
                    state=ScriptState.RUNNING;
                    lootTarget=lootTargets[0];
                    player.loot(lootTarget);
                    timer=new Timer(()=>{
                        if(lootTarget!=null&&lootTarget.IsSpawned&&lootTarget.isActiveAndEnabled)return;
                        if(lootTargets!=null&&lootTargets.Length>1){
                            for(int x=1;x<lootTargets.Length;x++){
                                lootTarget=lootTargets[x];
                                if(lootTarget==null)continue;
                                player.loot(lootTarget);
                            }
                            lootTargets=null;
                            return;
                        }
                        state=ScriptState.TASK_4;
                        timer.Stop();
                    },100,true);
                    timer.Start();
                }
            }else
            if(state==ScriptState.TASK_5){
                var bank=Datastub.Banks.Find(player,60);
                if(bank==null){
                    progression.setText("Searching");
                    state=player.shouldAbuelo()?ScriptState.TASK_2:ScriptState.IDLE;
                }else{
                    state=ScriptState.TASK_7;
                    if(timer!=null)timer.Stop();
                    progression.setText("Banking");
                    player.interact(bank,DNID.OPEN_BANK);
                    timer=new Timer(()=>{
                        state=player.getInventory().isFull()?ScriptState.TASK_5:(getLoot().Count!=0?ScriptState.TASK_4:ScriptState.TASK_6);
                    },1E4);
                    timer.Start();
                }
            }else
            if(state==ScriptState.TASK_7){
                if(!Bank.DepositAllExceptCaps())return;
                state=ScriptState.TASK_4;
            }else
            if(state==ScriptState.TASK_8){
                if(player.getAnimation()!=Player.Animation.IDLE)return;
                state=ScriptState.TASK_6;
            }else
            if(state==ScriptState.TASK_9){
                var anim=player.getAnimation();
                if(anim==Player.Animation.WALK)return;
                if(anim==Player.Animation.IDLE&&player.getCurrentSkill()==Stat.Movement){
                    state=ScriptState.TASK_6;
                    return;
                }
                if(lastFightPos!=null&&!player.isNear(lastFightPos,12))return;
                state=ScriptState.IDLE;
                lastFightPos=null;
            }
        }
    }
}