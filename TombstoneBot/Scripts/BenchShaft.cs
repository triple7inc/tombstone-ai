using TombstoneAI.Library;
using Il2CppEntities.Stats;
using Il2CppTileObjects.Crafting;
using Il2CppTileObjects.Crafting.Recipes;
namespace TombstoneAI.Scripts
{
    [BotInfo("Arrows Fletcher","Fletches arrowshafts and feathers into arrows on the fletching bench.","0.0.9","triple7inc",Stat.Woodwork)]
    public class BenchShaft:BotScriptBase{
        Inventory.Item item;
        Position position;
        Recipe recipe;
        private void doGetRecipe(){
            if(recipe!=null)state=ScriptState.TASK_1;
            else{
                var mat=item.getMaterialType();
                player.interact(position);
                timer=new Timer(()=>{
                    player.getRecipe(mat+" arrow",CraftType.FletchingBench,rec=>{
                        if(rec==null||rec.Level>player.getLevel(Stat.Woodwork)){
                            Error($"Your level is too low to craft {mat} Arrows!");
                            return;
                        }
                        recipe=rec;
                        state=ScriptState.TASK_1;
                    });
                },1E3);
                timer.Start();
            }
        }
        private bool hasMaterials(){
            int f=player.getInventory().getQuantity("feather");
            if(f==0)return(false);
            var mat=item.getMaterialType();
            int a=player.getInventory().getQuantity(mat+" arrowshafts");
            return(a!=0);
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
            recipe=null;
            item=null;
        }
        protected override void Program(){
            if(state==ScriptState.IDLE){
                position=null;
                var inv=player.getInventory();
                item=inv.getItem("arrowshafts",true);
                if(item==null){
                    Error("You don't have ARROWSHAFTS in your inventory!");
                    return;
                }
                if(!inv.hasItem("feather")){
                    Error("You don't have FEATHERS in your inventory!");
                    return;
                }
                int qty=player.getInventory().getQuantity(item.getMaterialType()+" arrow");
                if(inv.FreeSpace()==0&&qty==0){
                    Error("You require at least 1 free slot in your inventory!");
                    return;
                }
                position=Datastub.Workbenches.Find(player,CraftType.FletchingBench);
                if(position==null){
                    Error("There is no fletching bench nearby!");
                    return;
                }
                state=ScriptState.RUNNING;
                if(player.isNear(position)){
                    progression.setText("Preparing");
                    doGetRecipe();
                }else{
                    progression.setText("Running");
                    player.run(position,()=>{
                        if(recipe!=null)state=ScriptState.TASK_1;
                        else doGetRecipe();
                    });
                }
            }else
            if(state==ScriptState.RUNNING){
            }else
            if(state==ScriptState.TASK_1){
                var skill=player.getCurrentSkill();
                if(skill==Stat.Woodwork){
                    state=ScriptState.TASK_2;
                }else
                if(!hasMaterials()){
                    Error($"You've ran out of {item.getMaterialType().ToString().ToLower()} arrowshafts and/or feathers!");
                    return;
                }else{
                    progression.setText("Preparing");
                    player.interact(position);
                    state=ScriptState.RUNNING;
                    timer=new Timer(()=>{
                        player.craft(recipe,30,CraftType.FletchingBench,R=>{
                            state=R?ScriptState.TASK_2:ScriptState.TASK_1;
                            progression.setText("Fletching");
                        },openCraftingMenu:false);
                    },1E3);
                    timer.Start();
                }
            }else
            if(state==ScriptState.TASK_2){
                var skill=player.getCurrentSkill();
                if(skill!=Stat.Woodwork){
                    state=ScriptState.TASK_3;
                }
            }else
            if(state==ScriptState.TASK_3){
                if(player.isNear(position)){
                    state=ScriptState.TASK_1;
                    progression.setText("Preparing");
                }else{
                    progression.setText("Running");
                    player.run(position,()=>{
                        state=ScriptState.TASK_1;
                    });
                }
            }
        }
    }
}