using System;
using System.Linq;
using TombstoneAI;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TombstoneAI.Library;
using System.Collections.Generic;
using Il2CppTileObjects.SkillingNodes;
[assembly:MelonInfo(typeof(TombstoneBot),"Tombstone AI","0.7.8","triple7inc")]
[assembly:MelonGame]
namespace TombstoneAI
{
    public class TombstoneBot:MelonMod
    {
        Text Debug;
        bool relogin;
        Timer tDebug;
        Timer playBot;
        Doopstrap hud;
        Timer playBot0;
        Button runBotBtn;
        bool unlockedOnce;
        GUIStyle tmpStyle;
        Player player=null;
        GameObject hudPanel;
        GameObject infoPanel;
        GameObject debugPanel;
        Timer loginCheck=null;
        GameObject centerPanel;
        MelonInfoAttribute info;
        Library.Logger logger=null;
        Progression lastProgression;
        Text botNameText,botInfoText,botDescText,botButtonText;
        Dictionary<string,IBotScript>scripts=new Dictionary<string,IBotScript>();
        Dictionary<string,GameObject>buttons=new Dictionary<string,GameObject>();
        private protected string[]bots=new string[]{"StopAllBots","PowerKillCombat","TreeShaft","BenchShaft","PowerTrapFish","DayTimeChanger","EbonyTreeBanker"};
        private IBotScript getBotFromClass(string className){
            Type t=Type.GetType("TombstoneAI.Scripts."+className);
            if(t!=null&&typeof(IBotScript).IsAssignableFrom(t)){
                return(IBotScript)Activator.CreateInstance(t);
            }
            return(null);
        }
        private void LoadScripts(){
            scripts.Clear();
            foreach(string b in bots){
                IBotScript bot=this.getBotFromClass(b);
                if(bot!=null){
                    bot.OnMessage+=msg=>{
                        if(msg.EndsWith("StopAllBots")){
                            StopAllBots();
                            return;
                        }
                        if(!bot.isTool()&&msg.Contains(": (Error)")){
                            if(botButtonText.text=="Restart Bot")botButtonText.text="Start Bot";
                            Doopstrap.SetButtonActive(buttons[b],false);
                        }
                        logger.Msg("[Bot] "+msg);
                    };
                    MelonLogger.Msg("Script \""+b+"\" loaded.");
                    scripts.Add(b,bot);
                }
            }
        }
        private void StopAllBots(){
            foreach(KeyValuePair<string,IBotScript>script in scripts){
                if(script.Value.isTool())continue;
                script.Value.Stop();
                var btn=buttons[script.Key];
                Doopstrap.SetButtonActive(btn,false);
            }
        }
        private bool StopAllBotsExcept(string name){
            bool R=false;
            foreach(KeyValuePair<string,IBotScript>script in scripts){
                var btn=buttons[script.Key];
                if(script.Value.isTool())continue;
                if(script.Key==name&&script.Value.isActive()){
                    Doopstrap.SetButtonActive(btn,true);
                    R=true;
                    continue;
                }
                script.Value.Stop();
                Doopstrap.SetButtonActive(btn,false);
            }
            return(R);
        }
        private IBotScript[]GetActiveBots(){
            var R=new List<IBotScript>();
            foreach(KeyValuePair<string,IBotScript>script in scripts){
                if(script.Value.isTool())continue;
                if(script.Value.isActive()){
                    R.Add(script.Value);
                }
            }
            return(R.ToArray());
        }
        private IBotScript GetActiveBot(){
            foreach(KeyValuePair<string,IBotScript>script in scripts){
                if(script.Value.isTool())continue;
                if(script.Value.isActive()){
                    return(script.Value);
                }
            }
            return(null);
        }
        private string GetActiveBotName(){
            foreach(KeyValuePair<string,IBotScript>script in scripts){
                if(script.Value.isTool())continue;
                if(script.Value.isActive()){
                    return(script.Key);
                }
            }
            return(null);
        }
        private IBotScript GetBot(string name){
            IBotScript bot;
            var R=this.scripts.TryGetValue(name,out bot);
            return(R?bot:null);
        }
        private void LoginCheck(){
            ///var expired=GameObject.FindObjectOfType<Il2CppWitchslayer.Playfab.PlayfabSessionManager>()?.GetSession()?.IsExpired()??false;
            var panel=GameObject.FindObjectOfType<Il2CppTombstone.Client.LoggedInPanel>();
            if(panel==null/*||(panel!=null&&expired)*/)return;
            if(player==null)return;
            var bot=GetActiveBot();
            var username=""+player.getUsername();
            player=null;
            if(bot!=null){
                bot.Pause();
                relogin=true;
                bot.ResetTarget();
                bot.ClearMemory();
                bot.setState(ScriptState.IDLE);
                lastProgression=bot.getProgression();
            }
            var login=panel._loginMenu;
            if(login){
                playBot0=new Timer(()=>{
                    login._userInput.text=username;
                    login.LogoutAndForgetCredentials();
                    login._loginButton.onClick.Invoke();
                    playBot=new Timer(()=>{
                        var pnl=GameObject.FindObjectOfType<Il2CppTombstone.Client.LoggedInPanel>();
                        if(pnl?._loginMenu?._loggedInPlayButton?.onClick!=null)pnl._loginMenu._loggedInPlayButton.onClick.Invoke();
                    },3E3);
                    playBot.Start();
                },3E3);
                playBot0.Start();
            }
        }
        public void InitializeBotScriptPage(string className){
            var bot=GetBot(className);
            if(bot==null)return;
            var A=bot.GetBotInfo();
            if(A==null)return;
            var B=A.Description;
            if(!B.EndsWith("."))B+=".";
            botDescText.text=B;
            botNameText.text=A.Name;
            botInfoText.text=A.Author+" • "+A.Version;
            runBotBtn.onClick.RemoveAllListeners();
            void RunSelectedBot(){
                RunBot(className);
                if(bot.isTool())return;
                botButtonText.text="Restart Bot";
                foreach(var btn in buttons){
                    Doopstrap.SetButtonActive(btn.Value,btn.Key==className);
                }
            }
            botButtonText.text=bot.isTool()?"Run Tool":((bot.isActive()?"Restart":"Start")+" Bot");
            runBotBtn.onClick.AddListener((UnityAction)RunSelectedBot);
            centerPanel.SetActive(true);
            infoPanel.SetActive(false);
        }
        public void InitializeHUD(bool force=false){
            if(force||hud==null){
                hud=null;
                Doopstrap.SetFont();
                hud=new Doopstrap();
                hudPanel=hud.Panel(new Position(500,600),new Position(50,50));
                hudPanel.SetActive(BotGUI.isOpen());
                /*Debug panel (right)*/
                debugPanel=hud.Panel(
                    new Position(300,600),        ///width=300, height same as hudPanel
                    new Position(0,50),           ///anchor ignored when anchorMin is set
                    false,                        ///scrollable
                    hudPanel,                     ///parent
                    anchorMin:new Vector2(1,0.5f),
                    anchorMax:new Vector2(1,0.5f),
                    pivot:new Vector2(0,0.5f),
                    anchoredPos:new Vector2(0,0)
                );
                var debugLabel=hud.Text(
                    debugPanel,
                    new Position(260,600),
                    "Loading...",
                    TextAnchor.UpperLeft,
                    Color.white,
                    18,
                    anchorMin:new Vector2(0,1),
                    anchorMax:new Vector2(0,1),
                    pivot:new Vector2(0,1),
                    anchoredPos:new Vector2(20,-10)
                );
                Debug=debugLabel.GetComponent<Text>();
                Debug.verticalOverflow=VerticalWrapMode.Overflow;
                Debug.horizontalOverflow=HorizontalWrapMode.Wrap;
                /*Menu buttons panel (left)*/
                var leftPanel=hud.Panel(
                    new Position(300,600),
                    new Position(0,50),
                    false,
                    hudPanel,
                    anchorMin:new Vector2(0,0.5f),
                    anchorMax:new Vector2(0,0.5f),
                    pivot:new Vector2(1,0.5f),
                    anchoredPos:new Vector2(0,0)
                );
                int i=0;
                foreach(var botName in bots){
                    var bot=GetBot(botName);
                    var btn=hud.Button(
                        leftPanel,
                        new Position(280,40),
                        bot?.GetBotInfo()?.Name??botName,
                        ()=>InitializeBotScriptPage(botName),
                        anchorMin:new Vector2(0.5f,1),
                        anchorMax:new Vector2(0.5f,1),
                        pivot:new Vector2(0.5f,1),
                        anchoredPos:new Vector2(0,-10-(i*45))
                    );
                    buttons[botName]=btn;
                    i++;
                }
                var activeBotName=GetActiveBotName();
                if(activeBotName!=null)Doopstrap.SetButtonActive(buttons[activeBotName],true);
                /*Bot page info (center)*/
                centerPanel=hud.Panel(
                    new Position(500,600),
                    new Position(50,50),
                    false,
                    hudPanel,
                    anchorMin:new Vector2(0.5f,0.5f),
                    anchorMax:new Vector2(0.5f,0.5f),
                    pivot:new Vector2(0.5f,0.5f),
                    anchoredPos:Vector2.zero
                );
                ///Bot Name (big title)
                var titleObj=hud.Text(
                    centerPanel,
                    new Position(480,50),
                    "Bot Name",
                    TextAnchor.UpperCenter,
                    Color.white,
                    28,
                    anchorMin:new Vector2(0.5f,1),
                    anchorMax:new Vector2(0.5f,1),
                    pivot:new Vector2(0.5f,1),
                    anchoredPos:new Vector2(0,-30)
                );
                botNameText=titleObj.GetComponent<Text>();
                ///Bot Author + Version (small title)
                var infoObj=hud.Text(
                    centerPanel,
                    new Position(480,30),
                    "Author • Version",
                    TextAnchor.UpperCenter,
                    Color.gray,
                    18,
                    anchorMin:new Vector2(0.5f,1),
                    anchorMax:new Vector2(0.5f,1),
                    pivot:new Vector2(0.5f,1),
                    anchoredPos:new Vector2(0,-60)
                );
                botInfoText=infoObj.GetComponent<Text>();
                ///Bot Description
                var descObj=hud.Text(
                    centerPanel,
                    new Position(460,200),
                    "This is a bot that does stuff.",
                    TextAnchor.UpperLeft,
                    Color.white,
                    21,
                    anchorMin:new Vector2(0.5f,1),
                    anchorMax:new Vector2(0.5f,1),
                    pivot:new Vector2(0.5f,1),
                    anchoredPos:new Vector2(0,-105)
                );
                botDescText=descObj.GetComponent<Text>();
                ///Run Button
                var btnObj=hud.Button(
                    centerPanel,
                    new Position(260,40),
                    "Start Bot",
                    ()=>RunBot("YourBotName"),
                    anchorMin:new Vector2(0.5f,0),
                    anchorMax:new Vector2(0.5f,0),
                    pivot:new Vector2(0.5f,0),
                    anchoredPos:new Vector2(0,20)
                );
                botButtonText=btnObj.GetComponentInChildren<Text>();
                runBotBtn=btnObj.GetComponent<Button>();
                centerPanel.SetActive(false);
                /*Home page info (center)*/
                infoPanel=hud.Panel(
                    new Position(500,600),
                    new Position(50,50),
                    false,
                    hudPanel,
                    anchorMin:new Vector2(0.5f,0.5f),
                    anchorMax:new Vector2(0.5f,0.5f),
                    pivot:new Vector2(0.5f,0.5f),
                    anchoredPos:Vector2.zero
                );
                ///Big left-aligned "WELCOME TO"
                var welcomeObj=hud.Text(
                    infoPanel,
                    new Position(480,40),
                    "WELCOME TO",
                    TextAnchor.MiddleCenter,
                    Color.white,
                    30,
                    anchorMin:new Vector2(0,1),
                    anchorMax:new Vector2(0,1),
                    pivot:new Vector2(0,1),
                    anchoredPos:new Vector2(10,-15)
                );
                ///Right-aligned "Tombstone AI"
                var titleObj1=hud.Text(
                    infoPanel,
                    new Position(480,40),
                    info?.Name??"Tombstone AI",
                    TextAnchor.UpperRight,
                    Color.white,
                    24,
                    anchorMin:new Vector2(1,1),
                    anchorMax:new Vector2(1,1),
                    pivot:new Vector2(1,1),
                    anchoredPos:new Vector2(-10,-50)
                );
                ///Version text
                var versionObj=hud.Text(
                    infoPanel,
                    new Position(480,20),
                    "v"+(info?.Version??"BETA"),
                    TextAnchor.UpperRight,
                    Color.gray,
                    14,
                    anchorMin:new Vector2(1,1),
                    anchorMax:new Vector2(1,1),
                    pivot:new Vector2(1,1),
                    anchoredPos:new Vector2(-10,-75)
                );
                ///Description text
                var descObj1=hud.Text(
                    infoPanel,
                    new Position(460,200),
                    "Tombstone AI is a modular AI framework that lets you run and manage bot scripts in real-time.\n\nSelect a bot on the left to begin.",
                    TextAnchor.UpperLeft,
                    Color.white,
                    18,
                    anchorMin:new Vector2(0,1),
                    anchorMax:new Vector2(0,1),
                    pivot:new Vector2(0,1),
                    anchoredPos:new Vector2(10,-110)
                );
            }
            if(tDebug!=null)tDebug.Stop();
            tDebug=new Timer(()=>{
                if(player==null||!player.isActive()||Il2CppEntities.Players.Player.LocalPlayer==null)BotGUI.close();
                if(Debug==null)return;
                string R="\n";
                R+="> Player:\n";
                var bot=GetActiveBot();
                var pos=player.getPosition();
                R+="Stamina: "+player.getStamina()+"%\n";
                R+="Animation: "+player.getRawAnimation()+"\n";
                R+="Current Skill: "+player.getCurrentSkill().ToString()+"\n";
                R+="Location: "+pos.ToString()+"\n";
                R+="\nMap: "+this.player.basePlayer().Tilemapper._map.name+"\n";
                var p=player.getLastMoveData();
                R+="\n> Last Move:\n";
                if(p==null)R+="No last move";
                else{
                    R+=$"Dialogue ID: {p.DialogueNodeId}\n";
                    R+=$"Location: {p.ToPoint.x}, {p.ToPoint.y}\n";
                }
                R+="\n> Bot:\n";
                R+="State: "+(bot==null?"None":bot.getState().ToString())+"\n";
                Debug.text=R;
            },1E3,true);
            tDebug.Start();
            MelonLogger.Msg("HUD is initialized");
        }
        public override void OnApplicationStart(){
            MelonLogger.Msg("TombstoneAI initializing...");
            LoadScripts();
            MelonLogger.Msg("TombstoneAI loaded.");
            info=(MelonInfoAttribute)typeof(TombstoneBot).Assembly
            .GetCustomAttributes(typeof(MelonInfoAttribute),false)
            .FirstOrDefault();
            ///OpenRecipeHook.Setup();
            tmpStyle=new GUIStyle();
            tmpStyle.alignment=TextAnchor.MiddleLeft;
            tmpStyle.normal.textColor=Color.white;
            tmpStyle.fontStyle=FontStyle.Bold;
            tmpStyle.fontSize=18;
        }
        public override void OnUpdate(){
            if((player==null||!player.isActive())&&Il2CppEntities.Players.Player.LocalPlayer!=null){
                player=new Player(Il2CppEntities.Players.Player.LocalPlayer);
                logger=new Library.Logger(player);
                ///CraftHook.Setup(player.basePlayer());
                ///CmdCraftHook.Setup(player.basePlayer());
                if(loginCheck==null){
                    loginCheck=new Timer(LoginCheck,1E3,true);
                    loginCheck.Start();
                }
                if(relogin){
                    relogin=false;
                    var bot=GetActiveBot();
                    lastProgression.setPlayer(player);
                    if(bot!=null){
                        bot.ClearHandlers();
                        var botName=GetActiveBotName();
                        lastProgression.increaseRuntime(12);
                        bot.setPlayer(player,lastProgression);
                        try{Doopstrap.SetButtonActive(buttons[botName],true);}catch{}
                        bot.OnMessage+=msg=>{
                            if(msg.EndsWith("StopAllBots")){
                                StopAllBots();
                                return;
                            }
                            if(!bot.isTool()&&msg.Contains(": (Error)")){
                                if(botButtonText.text=="Restart Bot")botButtonText.text="Start Bot";
                                Doopstrap.SetButtonActive(buttons[botName],false);
                            }
                            logger.Msg("[Bot] "+msg);
                        };
                        playBot=new Timer(()=>{
                            bot.Play();
                            bot.RestartCoroutines();
                        },6E3);
                        playBot.Start();
                    }
                }else InitializeHUD();
                return;
            }
            if(Event.current!=null&&Event.current.type==EventType.KeyDown){
                if(Event.current.keyCode==KeyCode.F10){
                    if(hud.RootObject()==null)InitializeHUD(true);
                    if(BotGUI.isOpen())BotGUI.close();
                    else BotGUI.open();
                }else
                if(Event.current.keyCode==KeyCode.F9){
                    debugPanel.SetActive(!debugPanel.active);
                }
            }
            if(hudPanel)hudPanel.SetActive(BotGUI.isOpen());
        }
        public override void OnGUI(){
            if(player==null||!player.isActive())return;
            ///GUI.Label(DoopstrapV1.col(1,2,-8,3),player.getPosition().ToString(),tmpStyle);
            var bot=GetActiveBot();
            if(bot!=null){
                var prog=bot.getProgression();
                var text=prog.getText();
                var print=prog.Print();
                var rect=DoopstrapV1.ProgressionBox(text,print,false,false);
                var hovered=rect.Contains(Event.current.mousePosition);
                DoopstrapV1.ProgressionBox(text,print,hovered,true);
            }
            /*
            if(BotGUI.isOpen()){
                var parent=DoopstrapV1.col(6,8,-2,-4);
                GUI.Box(parent,"Tombstone AI");
                BotGUI.Button(DoopstrapV1.col(12,1,parent),"Debug",()=>{
                    var pos=player.getPosition();
                    string R="\n";
                    R+="> Player:\n";
                    R+="Stamina: "+player.getStamina()+"%\n";
                    R+="Animation: "+player.getRawAnimation()+"\n";
                    R+="Current Skill: "+player.getCurrentSkill().ToString()+"\n";
                    R+="Location: "+pos.ToString()+"\n";
                    var p=player.getLastMoveData();
                    R+="> Last Move:\n";
                    if(p==null)R+="No last move";
                    else{
                        R+=$"Dialogue ID: {p.DialogueNodeId}\n";
                        R+=$"Location: {p.ToPoint.x}, {p.ToPoint.y}\n";
                    }
                    logger.Msg("Debug: "+R);
                });
                BotGUI.Button(DoopstrapV1.col(12,1,-2,parent),"Stop All Bots",()=>{
                    StopAllBots();
                    logger.Msg("[Bots] Stopped");
                });
                BotGUI.Button(DoopstrapV1.col(12,1,-3,parent),"Survival: Tree Shafts",()=>RunBot("TreeShaft"));
                BotGUI.Button(DoopstrapV1.col(12,1,-4,parent),"Woodwork: Bench Arrows",()=>RunBot("BenchShaft"));
                BotGUI.Button(DoopstrapV1.col(12,1,-5,parent),"Fishing: Powerlevel Trap",()=>RunBot("PowerTrapFish"));
                BotGUI.Button(DoopstrapV1.col(12,1,-6,parent),"Combat: Powerlevel Enemies",()=>{
                    var pkc=this.GetBot("PowerKillCombat") as IBotScriptCombat;
                    if(pkc==null){
                        logger.Msg("Bot not found");
                        return;
                    }
                    pkc.setEnemies("Settler");
                    RunBot("PowerKillCombat");
                });
                BotGUI.Button(DoopstrapV1.col(12,1,-7,parent),"Walk to the Nearest Bank",()=>{
                    var F=Datastub.Banks.Find(player);
                    if(F!=null){
                        StopAllBots();
                        logger.Msg("[Bots] Stopped");
                        logger.Msg("Walking to the nearest bank...");
                        player.run(F,()=>{
                            logger.Msg($"Welcome to the bank, {player.getName()}!");
                            if(!Bank.inBank())player.interact(F,DNID.OPEN_BANK);
                        },false,2);
                    }
                });
                BotGUI.Button(DoopstrapV1.col(12,1,-8,parent),"Drop All Inventory Items",()=>{
                    player.getInventory().dropAll();
                    logger.Msg("Dropped entire inventory");
                });
            }
            */
        }
        private string tmp1(){
            var ggo=GameObject.Find("Willow Tree");
            if(ggo==null)return"No instance";
            var go=GameObject.Instantiate(ggo);
            var sk=go.GetComponent<SkillingNode>();
            if(sk==null)return"No instance";
            var pos=player.getPosition();
            sk._interactRangeOverride=10000000;
            sk.TilePosition=new Vector2Int(pos.x-1,pos.y);
            go.transform.position=new Vector3(pos.x-1,pos.y);
            return"Spawned!";
        }
        private string tmp(){
            var R="";
            var p=player.basePlayer();
            var aopt=GameObject.FindObjectOfType<Il2CppUI.RightClickMenu.UiRightClickMenu>();
            if(aopt==null)return"No instance";
            var aoptt=aopt._extraOptions;
            foreach(var opt in aoptt){
                R+="\nOption: "+opt.Option.Text;
                if(opt.Option.Text=="Open Bank"){
                    opt.Option.Callback?.Invoke();
                }
            }
            return("Tmp: "+R);
        }
        private void RunBot(string name){
            var bot=this.GetBot(name);
            if(bot.isTool()){
                if(!bot.Requirements())return;
                bot.setPlayer(this.player);
                bot.Start();
                return;
            }
            bool R=StopAllBotsExcept(name);
            if(bot!=null){
                if(!R&&!bot.Requirements())return;
                bot.setPlayer(this.player);
                if(R)bot.Restart();
                else bot.Start();
            }
        }
        private void RunBot(string name,Progression progression){
            bool R=StopAllBotsExcept(name);
            var bot=this.GetBot(name);
            if(bot!=null){
                if(!R&&!bot.Requirements())return;
                bot.setPlayer(this.player,progression);
                if(R)bot.Restart();
                else bot.Start();
            }
        }
    }
}