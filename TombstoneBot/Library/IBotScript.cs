using Il2CppEntities.Stats;
using Il2CppSystem.Security.Util;
using Il2CppTileObjects.SkillingNodes;
using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace TombstoneAI.Library
{
    public enum ScriptState{
        IDLE,
        RUNNING,
        TASK_1,
        TASK_2,
        TASK_3,
        TASK_4,
        TASK_5,
        TASK_6,
        TASK_7,
        TASK_8,
        TASK_9
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ToolInfoAttribute:BotInfoAttribute
    {
        public ToolInfoAttribute(string name,string description,string version,string author,params Stat[] skills):base(name,description,version,author,skills)
        {
            Description=description;
            isToolScript=true;
            Version=version;
            Skills=skills;
            Author=author;
            Name=name;
        }

    }
    public class BotInfoAttribute:Attribute
    {
        public string Name{get;set;}
        public string Author{get;set;}
        public Stat[] Skills{get;set;}
        public string Version{get;set;}
        public bool isToolScript{get;set;}
        public string Description{get;set;}

        public BotInfoAttribute(string name,string description,string version,string author,params Stat[]skills)
        {
            Description=description;
            isToolScript=false;
            Version=version;
            Skills=skills;
            Author=author;
            Name=name;
        }
    }
    public interface IBotScript{
        void setPlayer(Player player,Progression progression);
        void setProgression(Progression progression);
        void Settings(Dictionary<string,object>map);
        void setState(ScriptState state);
        void setPlayer(Player player);
        event Action<string>OnMessage;
        BotInfoAttribute GetBotInfo();
        Progression getProgression();
        void RestartCoroutines();
        ScriptState getState();
        void ClearHandlers();
        bool Requirements();
        Player getPlayer();
        void ClearMemory();
        void ResetTarget();
        bool isActive();
        bool isPaused();
        void Restart();
        bool isTool();
        void Pause();
        void Start();
        void Play();
        void Stop();
    }
    public abstract class BotScriptBase:IBotScript
    {
        private bool active;
        private bool paused;
        protected int integer;
        protected Timer timer;
        protected object token;
        protected Player player;
        protected bool isToolScript;
        protected ScriptState state;
        protected SkillingNode target;
        protected WaitForSeconds wait;
        private BotInfoAttribute botInfo;
        protected Progression progression;
        public event Action<string>OnMessage;
        protected Dictionary<string,object>config;
        public BotScriptBase(){
            this.botInfo=this.GetBotInfo();
            this.wait=new WaitForSeconds(1f);
        }
        public void setPlayer(Player player){
            var p=this.botInfo==null?new Stat[]{}:this.botInfo.Skills;
            this.progression=new Progression(player,p);
            this.player=player;
        }
        public void setPlayer(Player player,Progression progression){
            this.progression=progression;
            this.player=player;
        }
        public bool isActive(){return(this.active);}
        public bool isPaused(){return(this.paused);}
        public Player getPlayer(){return(this.player);}
        public bool isTool(){return(this.isToolScript);}
        public ScriptState getState(){return(this.state);}
        public void setActive(bool active){this.active=active;}
        public void setState(ScriptState state){this.state=state;}
        public Progression getProgression(){return(this.progression);}
        public void setProgression(Progression progression){this.progression=progression;}
        public BotInfoAttribute GetBotInfo()=>GetType().GetCustomAttribute<BotInfoAttribute>();
        protected void Log(string msg){OnMessage?.Invoke((this.botInfo.Name??GetType().Name)+": "+msg);}
        public void ClearHandlers(){this.OnMessage=null;}
        public void ResetTarget(){this.target=null;}
        public void Pause(){this.paused=true;}
        public void Play(){this.paused=false;}
        public void RestartCoroutines(){
            MelonCoroutines.Stop(this.token);
            this.token=MelonCoroutines.Start(Main());
        }
        public virtual void Start(){
            if(active)return;
            this.token=MelonCoroutines.Start(Main());
            if(!isTool())Log("started");
            this.state=ScriptState.IDLE;
            this.target=null;
            active=true;
        }
        public virtual void Restart(){
            if(!active){
                Start();
                return;
            }
            active=true;
            ClearMemory();
            this.state=ScriptState.IDLE;
            if(!isTool())Log("restarted");
        }
        public virtual void Stop(){
            if(!active)return;
            MelonCoroutines.Stop(this.token);
            if(!isTool())Log("stopped");
            this.state=ScriptState.IDLE;
            this.target=null;
            ClearMemory();
            active=false;
        }
        public void Error(string msg){
            Stop();
            Log("(Error) "+msg);
        }
        public void Settings(Dictionary<string,object>map){
            Log("updated");
            config=map;
        }
        public virtual bool Requirements(){
            ///continue requirements here
            return(true);
        }
        public virtual void ClearMemory(){
            ///continue clean memory variables here
        }
        protected abstract void Program();
        private IEnumerator Main(){
            if(!active||paused)yield return new WaitForEndOfFrame();
            while(active&&!paused){
                Program();
                yield return wait;
            }
        }
    }
    public abstract class ToolScriptBase:BotScriptBase,IBotScript
    {
        public ToolScriptBase():base(){
            isToolScript=true;
        }
        private IEnumerator Main(){
            if(!isActive()||isPaused())yield return new WaitForEndOfFrame();
            if(isActive()&&!isPaused()){
                Program();
                Stop();
                yield return wait;
            }
        }
        public override void Start(){
            if(isActive())return;
            this.token=MelonCoroutines.Start(Main());
            this.state=ScriptState.IDLE;
            this.target=null;
            setActive(true);
        }
    }
}