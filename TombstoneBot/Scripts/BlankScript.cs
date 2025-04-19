using TombstoneAI.Library;
using Il2CppEntities.Stats;
namespace TombstoneAI.Scripts
{
    [BotInfo("BlankScript","This is a boiler template script.","0.0.1","triple7inc",Stat.Health,Stat.Defense)]
    public class BlankScript:BotScriptBase{
        public override bool Requirements(){
            ///continue requirements here
            return base.Requirements();
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
        }
        protected override void Program(){
            ///continue logic here
        }
    }
}