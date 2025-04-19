using TombstoneAI.Library;
using Il2CppEntities.Stats;
namespace TombstoneAI.Scripts
{
    [BotInfo("BlankScriptCombat","This is a boiler template script for combat.","0.0.1","triple7inc",Stat.Health,Stat.Defense)]
    public class BlankScriptCombat:CombatBotScriptBase{
        public override void ClearMemory(){
            ///continue clean memory variables here
        }
        protected override void Program(){
            ///continue logic here
        }
    }
}