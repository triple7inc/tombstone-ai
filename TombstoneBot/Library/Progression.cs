using System;
using System.Collections.Generic;
using System.Globalization;
using Il2CppEntities.Stats;
namespace TombstoneAI.Library
{
    public class Progression
    {
        string text;
        long runtime;
        Player player;
        List<Skill>skills;
        public Progression(Player player,params Stat[]skills){
            this.skills=new List<Skill>();
            foreach(var sk in skills){
                this.skills.Add(new Skill(player,sk));
            }
            this.runtime=Timestamp();
            this.player=player;
            this.text="Idle";
        }
        public string Print(){
            string R=$"[ {player.getName()} ]\n";
            R+=$"Runtime: {ToHMS(getSeconds())}\n\n";
            foreach(var sk in skills){
                var dt=sk.get();
                R+=$"({sk.getName()})\n";
                var cur=sk.getCurrent(true);
                if(dt.getLevel()==0)R+=$"Level: {cur.getLevel()}\n";
                else R+=$"Level: {cur.getLevel()} (+{dt.getLevel()})\n";
                R+=$"Exp: +{dt.getParsedExp()}\n\n";
            }
            return(R==""?null:R.TrimEnd('\n'));
        }
        public void setPlayer(Player player){
            this.player=player;
            foreach(var sk in skills){
                sk.setPlayer(player);
            }
        }
        public string getText(){return(text);}
        public long getRuntime(){return(runtime);}
        public void setText(string text){this.text=text;}
        public int getElapsedTime(){return(getSeconds());}
        public int getSeconds(){return(int)(Timestamp()-runtime);}
        public void increaseRuntime(int seconds){runtime+=seconds*2;}
        public static long Timestamp(){return(DateTimeOffset.UtcNow.ToUnixTimeSeconds());}
        public static string ToHMS(int seconds){var t=TimeSpan.FromSeconds(seconds);return($"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}");}
        public static string ToHMS(long seconds){var t=TimeSpan.FromSeconds(seconds);return($"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}");}
        public static string ToHMS(float seconds){var t=TimeSpan.FromSeconds(seconds);return($"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}");}
        public static string ToHMS(double seconds){var t=TimeSpan.FromSeconds(seconds);return($"{(int)t.TotalHours:00}:{t.Minutes:00}:{t.Seconds:00}");}
        public class Skill
        {
            Stat skill;
            Data start;
            string name;
            Data current;
            Player player;
            public class Data
            {
                int level;
                float exp;
                public Data(int level,float exp){
                    this.level=level;
                    this.exp=exp;
                }
                public float getExp(){return(exp);}
                public int getLevel(){return(level);}
                public string getParsedExp(){
                    return((int)exp).ToString("N0",CultureInfo.InvariantCulture);
                }
            }
            public Skill(Player player,Stat skill){
                this.start=new Data(player.getLevel(skill),player.getExp(skill));
                this.name=Datastub.Skills.Find(skill);
                this.player=player;
                this.skill=skill;
                Update();
            }
            private void Update(){
                try{
                    this.current=new Data(this.player.getLevel(this.skill),this.player.getExp(this.skill));
                }catch{}
            }
            public Data getCurrent(bool raw=false){
                if(!raw)Update();
                return(this.current);
            }
            public void setPlayer(Player player){
                this.player=player;
                Update();
            }
            public Data get(bool raw=false){
                if(!raw)Update();
                return new Data(this.current.getLevel()-this.start.getLevel(),this.current.getExp()-this.start.getExp());
            }
            public string getName(){
                return(name);
            }
        }
    }
}