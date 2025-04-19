using Il2CppInventory;
using TombstoneAI.Library;
using Il2CppEntities.Stats;
using Il2CppTileObjects.Crafting;
using TombstoneAI.Library.Utilities;
using Il2CppTileObjects.Crafting.Recipes;
namespace TombstoneAI.Scripts
{
    [BotInfo("Treeshafter","Chops the best nearby tree and cuts logs into arrowshafts.","0.1.8","triple7inc",Stat.Survival,Stat.Woodwork)]
    public class TreeShaft:BotScriptBase{
        bool active;
        bool paused;
        object token;
        Recipe recipe;
        string tarName;
        bool shouldClean;
        private void cleanInventory(){
            if(!shouldClean)return;
            var inv=Inventory.GetItems();
            int x=-1;
            foreach(InventoryItem A in inv){
                x++;
                var P=A?.Item?.name;
                if(P==null)continue;
                if(P=="Knife")continue;
                if(P=="Primer Caps")continue;
                if(P.EndsWith("Axe"))continue;
                if(P.EndsWith("Log"))continue;
                if(P.EndsWith("Bark"))continue;
                if(P.EndsWith("Arrowshafts"))continue;
                player.getInventory().dropItem(x);
            }
        }
        private void doGetRecipe(){
            if(recipe!=null)state=ScriptState.TASK_1;
            else{
                player.getRecipe(tarName+" arrowshafts",CraftType.Inventory,rec=>{
                    if(rec==null||rec.Level>player.getLevel(Il2CppEntities.Stats.Stat.Woodwork)){
                        Error($"Your level is too low to craft {tarName} Arrowshafts!");
                        return;
                    }
                    recipe=rec;
                    state=ScriptState.TASK_1;
                });
            }
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
            recipe=null;
        }
        protected override void Program(){
            if(state==ScriptState.IDLE){
                target=null;
                if(!player.isToolEquipped(ToolType.Hatchet)&&!player.getInventory().hasItem("axe",true)){
                    Error("You don't have an AXE equipped or in your inventory!");
                    return;
                }
                if(!player.isToolEquipped(ToolType.Knife)&&!player.getInventory().hasItem("knife")){
                    Error("You don't have a KNIFE equipped or in your inventory!");
                    return;
                }
                target=Datastub.Trees.Find(player);
                if(target==null){
                    Error("There are no trees nearby that you can chop!");
                    return;
                }
                state=ScriptState.RUNNING;
                progression.setText("Running");
                tarName=target.name.Split(' ')[0];
                shouldClean=tarName=="Pine"||tarName=="Oak";
                if(player.isNear(target.TilePosition)){
                    doGetRecipe();
                }else{
                    player.run(target.TilePosition,()=>{
                        if(recipe!=null)state=ScriptState.TASK_1;
                        else doGetRecipe();
                    });
                }
            }else
            if(state==ScriptState.RUNNING){
            }else
            if(state==ScriptState.TASK_1){
                int qty=player.getInventory().getQuantity(tarName+" log");
                if(player.getInventory().FreeSpace()<2&&qty<2){
                    Error("Your inventory is too full to continue!");
                    return;
                }
                var anim=player.getAnimation();
                progression.setText("Chopping");
                if(anim==Player.Animation.WOODCUTTING){
                    state=ScriptState.TASK_2;
                }else{
                    player.interact(target.TilePosition);
                    state=ScriptState.TASK_2;
                }
            }else
            if(state==ScriptState.TASK_2){
                if(player.getAnimation()!=Player.Animation.WOODCUTTING){
                    state=player.getInventory().isFull()?ScriptState.TASK_3:ScriptState.TASK_1;
                }
            }else
            if(state==ScriptState.TASK_3){
                int qty=player.getInventory().getQuantity(tarName+" log");
                if(qty==0)state=ScriptState.TASK_1;
                else{
                    progression.setText("Fletching");
                    player.craft(recipe,qty,CraftType.Inventory,R=>{
                        ///state=R?ScriptState.TASK_4:ScriptState.TASK_1;
                        state=ScriptState.TASK_4;
                    });
                    state=ScriptState.RUNNING;
                }
            }else
            if(state==ScriptState.TASK_4){
                var anim=player.getAnimation();
                if(anim!=Player.Animation.CRAFTING){
                    state=ScriptState.TASK_5;
                }
            }else
            if(state==ScriptState.TASK_5){
                progression.setText("Dropping");
                cleanInventory();
                state=ScriptState.TASK_1;
            }
        }
    }
}