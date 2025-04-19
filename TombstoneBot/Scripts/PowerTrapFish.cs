using TombstoneAI.Library;
using Il2CppEntities.Stats;
using TombstoneAI.Library.Utilities;
namespace TombstoneAI.Scripts
{
    [BotInfo("Power Trap Fishing","Powerskills trap fishing at any nearby spot and drops entire inventory.","0.0.6","triple7inc",Stat.Fishing)]
    public class PowerTrapFish:BotScriptBase{
        protected override void Program(){
            if(state==ScriptState.IDLE){
                target=null;
                if(!player.isToolEquipped(ToolType.FishingTrap)){
                    Error("You don't have a FISH TRAP equipped!");
                    return;
                }
                target=Datastub.Fishing.Find(player,Datastub.Fishing.FishSpotType.TRAP);
                if(target==null){
                    Error("There are no spots nearby to fish!");
                    return;
                }
                progression.setText("Running");
                if(player.isNear(target.TilePosition,9)){
                    state=ScriptState.TASK_1;
                }else{
                    state=ScriptState.RUNNING;
                    player.run(target.TilePosition,()=>{
                        state=ScriptState.TASK_1;
                    },false,9);
                }
            }else
            if(state==ScriptState.RUNNING){
            }else
            if(state==ScriptState.TASK_1){
                var x=-1;
                var inv=player.getInventory();
                var items=Inventory.GetItems();
                progression.setText("Dropping");
                foreach(var i in items){
                    x++;
                    var name=i?.Item?.name??null;
                    if(name!=null)inv.dropItem(x);
                }
                state=ScriptState.TASK_2;
            }else
            if(state==ScriptState.TASK_2){
                progression.setText("Fishing");
                var skill=player.getCurrentSkill();
                if(skill==Stat.Fishing){
                    state=ScriptState.TASK_3;
                }else{
                    player.interact(target.TilePosition);
                    state=ScriptState.TASK_3;
                }
            }else
            if(state==ScriptState.TASK_3){
                if(player.getCurrentSkill()!=Stat.Fishing){
                    state=ScriptState.TASK_1;
                }
            }
        }
    }
}