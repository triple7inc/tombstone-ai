using TombstoneAI.Library;
using Il2CppEntities.Stats;
namespace TombstoneAI.Scripts
{
    [ToolInfo("[ Stop ]","This is tool will stop all active bots","0.0.1","triple7inc",Stat.Health,Stat.Defense)]
    public class StopAllBots:ToolScriptBase{
        public override bool Requirements(){
            ///continue requirements here
            return base.Requirements();
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
        }
        protected override void Program(){
            Log("StopAllBots");
        }
    }
}