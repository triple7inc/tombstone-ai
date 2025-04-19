using UnityEngine;
using TombstoneAI.Library;
using Il2CppMapping.Lighting;
namespace TombstoneAI.Scripts
{
    [ToolInfo("B01A's Weather App","This is the weather application used by the AI ‘B01A’ to monitor and manage climate conditions across the island-nation of Boia","80.1.4","B01A")]
    public class DayTimeChanger:ToolScriptBase{
        private enum TOD:byte{
            Dawn,
            Day,
            Dusk,
            Night,
            None
        }
        DayNightCycle P;
        TOD current=TOD.None;
        TOD previous=TOD.None;
        public DayTimeChanger():base(){
            if(timer!=null)timer.Stop();
            timer=new Timer(()=>{
                if(player==null||current==TOD.None)return;
                if(((TOD)(byte)P.TimeOfDay)==current)return;
                P.TimeOfDay=(TimeOfDay)(byte)current;
            },100,true);
            timer.Start();
        }
        TOD GetTOD(byte hour){
            if(hour>=5&&hour<12)return(TOD.Dawn);
            if(hour>=12&&hour<17)return(TOD.Day);
            if(hour>=17&&hour<20)return(TOD.Dusk);
            return(TOD.Night);
        }
        public override bool Requirements(){
            ///continue requirements here
            P=GameObject.FindObjectOfType<DayNightCycle>();
            return(P!=null);
        }
        public override void ClearMemory(){
            ///continue clean memory variables here
        }
        protected override void Program(){
            ///continue logic here
            Doopstrap.AlertInput("Weather","Enter a value from 1 to 24 to set a custom hour, or enter 0 to follow the default routine.",R=>{
                if(previous==TOD.None)previous=(TOD)(byte)P.TimeOfDay;
                R=R.Trim();
                int X=int.Parse(R);
                if(X<1){
                    P.TimeOfDay=(TimeOfDay)(byte)previous;
                    previous=TOD.None;
                    current=TOD.None;
                }else{
                    current=GetTOD((byte)X);
                    P.TimeOfDay=(TimeOfDay)(byte)current;
                }
            });
        }
    }
}