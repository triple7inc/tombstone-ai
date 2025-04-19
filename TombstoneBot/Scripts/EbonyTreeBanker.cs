using TombstoneAI.Library;
using Il2CppEntities.Stats;
using TombstoneAI.Library.Utilities;
namespace TombstoneAI.Scripts
{
    [BotInfo("Ebony Lumberjack","Safely chops Ebony Tree in Ridgewood and banks logs","0.0.1","triple7inc",Stat.Survival,Stat.Movement)]
    public class EbonyTreeBanker:BotScriptBase{
        Position standPos=new Position(47,-102);
        Position treePos=new Position(48,-101);
        Position walkPos=new Position(0,-149);
        private bool shouldRunaway(){
            return(player.getHealth()<(player.getMaxHealth()/2));
        }
        public override bool Requirements(){
            ///continue requirements here
            integer=Datastub.Trees.GetLevel(Datastub.MaterialType.Ebony);
            return base.Requirements();
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
        }
        protected override void Program(){
            ///continue logic here
            if(state==ScriptState.IDLE){
                if(player.getIsland()!=Datastub.IslandType.Ridgewood){
                    Error("You are not in Ridgewood!");
                    return;
                }
                if(integer==0||player.getLevel(Stat.Survival)<integer){
                    Error("Your level is too low to chop the Ebony Tree!");
                    return;
                }
                if(!player.isToolEquipped(ToolType.Hatchet)&&!player.getInventory().hasItem("axe",true)){
                    Error("You don't have an AXE equipped or in your inventory!");
                    return;
                }
                if(!player.isNear(standPos,30)){
                    Error("Please stand by the second-lowest Ebony Tree to begin.");
                    return;
                }
                state=ScriptState.RUNNING;
                progression.setText("Running");
                if(player.getPosition().Equals(standPos)){
                    progression.setText("Waiting");
                    state=ScriptState.TASK_1;
                }else{
                    player.run(standPos,()=>{
                        state=ScriptState.TASK_1;
                        progression.setText("Waiting");
                    },maxDistance:0);
                }
            }else
            if(state==ScriptState.TASK_1){
                if(shouldRunaway()){
                    state=ScriptState.TASK_4;
                    return;
                }
                target=Datastub.Trees.isActive(treePos);
                if(target!=null)state=ScriptState.TASK_2;
            }else
            if(state==ScriptState.TASK_2){
                if(shouldRunaway()||player.getInventory().isFull()){
                    state=ScriptState.TASK_4;
                    return;
                }
                var anim=player.getAnimation();
                progression.setText("Chopping");
                if(anim==Player.Animation.WOODCUTTING){
                    state=ScriptState.TASK_3;
                }else{
                    player.interact(target.TilePosition);
                    state=ScriptState.TASK_3;
                }
            }else
            if(state==ScriptState.TASK_3){
                if(player.getAnimation()!=Player.Animation.WOODCUTTING){
                    state=player.getInventory().isFull()?ScriptState.TASK_4:ScriptState.TASK_1;
                }
            }else
            if(state==ScriptState.TASK_4){
                state=ScriptState.RUNNING;
                progression.setText("Running");
                player.run(walkPos,()=>{
                    state=ScriptState.TASK_5;
                },maxDistance:3);
            }else
            if(state==ScriptState.TASK_5){
                var bank=Datastub.Banks.Find(player,0);
                if(bank==null){
                    Error("Something went horribly wrong! Dayum...");
                    return;
                }
                player.interact(bank,DNID.OPEN_BANK);
                progression.setText("Banking");
                if(timer!=null)timer.Stop();
                timer=new Timer(()=>{
                    state=Bank.inBank()?ScriptState.TASK_6:ScriptState.TASK_5;
                },1E4);
                timer.Start();
                state=ScriptState.RUNNING;
            }else
            if(state==ScriptState.TASK_6){
                progression.setText("Banking");
                if(Bank.DepositAllExceptTools()){
                    state=ScriptState.TASK_7;
                }
            }else
            if(state==ScriptState.TASK_7){
                state=player.getInventory().isFull()?ScriptState.TASK_6:ScriptState.TASK_8;
            }else
            if(state==ScriptState.TASK_8){
                state=ScriptState.RUNNING;
                progression.setText("Running");
                player.run(walkPos,()=>{
                    state=ScriptState.TASK_9;
                },maxDistance:0);
            }else
            if(state==ScriptState.TASK_9){
                state=ScriptState.RUNNING;
                player.run(standPos,()=>{
                    state=ScriptState.TASK_1;
                    progression.setText("Waiting");
                },maxDistance:0);
            }
        }
    }
}