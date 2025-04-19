using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Il2CppEntities.Players;
using Il2CppTombstone.Client;
namespace TombstoneAI.Library
{
    public class Doopstrap
    {
        private Canvas canvas;
        private GameObject root;
        private static Font loadedFont;
        public static void SetFont(string fontName="Arial.ttf"){
            loadedFont=Resources.GetBuiltinResource<Font>(fontName);
        }
        private static string RandomString(int length=12){
            const string chars="abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var sb=new System.Text.StringBuilder(length);
            var rng=new System.Random();
            for(int i=0;i<length;i++)sb.Append(chars[rng.Next(chars.Length)]);
            return(sb.ToString());
        }
        public Doopstrap(){
            if(loadedFont==null)SetFont();
            root=new GameObject(RandomString());
            canvas=root.AddComponent<Canvas>();
            var scaler=root.AddComponent<CanvasScaler>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            scaler.referenceResolution=new Vector2(1920,1080);
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            root.AddComponent<GraphicRaycaster>();
        }
        public GameObject Panel(Position size,Position anchor,bool scrollable=false,GameObject parent=null,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null){
            return(Doopstrap.Panel(size,anchor,scrollable,parent??root,anchorMin,anchorMax,pivot,anchoredPos,false));
        }
        public static GameObject Panel(Position size,Position anchor,bool scrollable=false,GameObject parent=null,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null,bool _static=true){
            var panel=new GameObject(RandomString());
            panel.transform.SetParent(parent.transform);

            var rt=panel.AddComponent<RectTransform>();
            rt.sizeDelta=size.ToVector2();
            rt.anchorMin=anchorMin??new Vector2(anchor.x/100f,anchor.y/100f);
            rt.anchorMax=anchorMax??new Vector2(anchor.x/100f,anchor.y/100f);
            rt.pivot=pivot??new Vector2(0.5f,0.5f);
            rt.anchoredPosition=anchoredPos??Vector2.zero;

            var img=panel.AddComponent<Image>();
            img.color=new Color(0,0,0,0.5f);

            if(scrollable){
                var scroll=panel.AddComponent<ScrollRect>();
                var content=new GameObject(RandomString());
                content.transform.SetParent(panel.transform);
                var contentRT=content.AddComponent<RectTransform>();
                contentRT.sizeDelta=size.ToVector2();
                contentRT.anchorMin=new Vector2(0,1);
                contentRT.anchorMax=new Vector2(1,1);
                contentRT.pivot=new Vector2(0.5f,1);
                scroll.content=contentRT;
                scroll.horizontal=false;
                scroll.vertical=true;
                var mask=panel.AddComponent<Mask>();
                mask.showMaskGraphic=false;
            }
            var scroller=panel.GetComponent<ScrollRect>();
            return(scroller!=null?scroller.content.gameObject:panel);
        }
        public GameObject Button(GameObject parent,Position size,string text,Action onClick,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null){
            return(Doopstrap.Button(parent??root,size,text,onClick,anchorMin,anchorMax,pivot,anchoredPos,false));
        }
        public static GameObject Button(GameObject parent,Position size,string text,Action onClick,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null,bool _static=true){
            var btnObj=new GameObject(RandomString());
            btnObj.transform.SetParent(parent.transform);
            var rt=btnObj.AddComponent<RectTransform>();
            rt.sizeDelta=size.ToVector2();
            rt.anchorMin=anchorMin??new Vector2(0.5f,0.5f);
            rt.anchorMax=anchorMax??new Vector2(0.5f,0.5f);
            rt.pivot=pivot??new Vector2(0.5f,0.5f);
            rt.anchoredPosition=anchoredPos??Vector2.zero;

            var img=btnObj.AddComponent<Image>();
            img.color=Color.gray;

            var btn=btnObj.AddComponent<Button>();
            btn.onClick.AddListener(onClick);

            var colors=btn.colors;
            colors.normalColor=Color.gray;
            colors.highlightedColor=new Color(0.75f,0.75f,0.75f);///hover
            colors.pressedColor=new Color(0.5f,0.5f,0.5f);///click
            colors.selectedColor=Color.green;///if selected
            colors.disabledColor=Color.black;
            btn.colors=colors;

            var nav=btn.navigation;
            nav.mode=Navigation.Mode.None;
            btn.navigation=nav;

            var txtObj=new GameObject(RandomString());
            txtObj.transform.SetParent(btnObj.transform);
            var txt=txtObj.AddComponent<Text>();
            txt.text=text;
            txt.font=loadedFont;
            txt.color=Color.white;
            txt.resizeTextMinSize=14;
            txt.resizeTextMaxSize=28;
            txt.resizeTextForBestFit=true;
            txt.alignment=TextAnchor.MiddleCenter;
            txt.rectTransform.anchoredPosition=Vector2.zero;
            txt.rectTransform.sizeDelta=new Vector2(size.x-10,size.y-4);
            return(btnObj);
        }
        public static void SetButtonActive(GameObject btn,bool selected){
            var img=btn.GetComponent<Image>();
            var b=btn.GetComponent<Button>();
            var colors=b.colors;
            img.color=selected?colors.selectedColor:colors.normalColor;
        }

        public static void SetButtonEnabled(GameObject btn,bool enabled){
            var img=btn.GetComponent<Image>();
            var b=btn.GetComponent<Button>();
            b.interactable=enabled;
            var colors=b.colors;
            img.color=enabled?colors.normalColor:colors.disabledColor;
        }
        public GameObject Input(GameObject parent,Position size,out InputField field,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null){
            return(Doopstrap.Input(parent??root,size,out field,anchorMin,anchorMax,pivot,anchoredPos,false));
        }
        public static GameObject Input(GameObject parent,Position size,out InputField field,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null,bool _static=true){
            var inputObj=new GameObject(RandomString());
            inputObj.transform.SetParent(parent.transform);
            var rt=inputObj.AddComponent<RectTransform>();
            rt.sizeDelta=size.ToVector2();
            rt.anchorMin=anchorMin??new Vector2(0.5f,0.5f);
            rt.anchorMax=anchorMax??new Vector2(0.5f,0.5f);
            rt.pivot=pivot??new Vector2(0.5f,0.5f);
            rt.anchoredPosition=anchoredPos??Vector2.zero;

            var img=inputObj.AddComponent<Image>();
            img.color=Color.white;

            field=inputObj.AddComponent<InputField>();

            var placeholderObj=new GameObject(RandomString());
            placeholderObj.transform.SetParent(inputObj.transform);
            var placeholder=placeholderObj.AddComponent<Text>();
            placeholder.text="Enter text...";
            placeholder.color=Color.gray;
            placeholder.font=loadedFont;
            placeholder.alignment=TextAnchor.MiddleLeft;
            placeholder.rectTransform.sizeDelta=size.ToVector2();

            var textObj=new GameObject(RandomString());
            textObj.transform.SetParent(inputObj.transform);
            var text=textObj.AddComponent<Text>();
            text.color=Color.black;
            text.font=loadedFont;
            text.alignment=TextAnchor.MiddleLeft;
            text.rectTransform.sizeDelta=size.ToVector2();

            field.textComponent=text;
            field.placeholder=placeholder;

            return(inputObj);
        }
        public GameObject Text(GameObject parent,Position size,string content,TextAnchor alignment=TextAnchor.UpperLeft,Color? color=null,int fontSize=21,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null){
            return(Doopstrap.Text(parent??root,size,content,alignment,color,fontSize,anchorMin,anchorMax,pivot,anchoredPos,false));
        }
        public static GameObject Text(GameObject parent,Position size,string content,TextAnchor alignment=TextAnchor.UpperLeft,Color? color=null,int fontSize=21,Vector2? anchorMin=null,Vector2? anchorMax=null,Vector2? pivot=null,Vector2? anchoredPos=null,bool _static=true){
            var textObj=new GameObject(RandomString());
            textObj.transform.SetParent(parent.transform);
            var txt=textObj.AddComponent<Text>();
            txt.text=content;
            txt.font=loadedFont;
            txt.color=color??Color.white;
            txt.fontSize=fontSize;
            txt.alignment=alignment;
            txt.rectTransform.sizeDelta=size.ToVector2();
            txt.rectTransform.anchorMin=anchorMin??new Vector2(0.5f,0.5f);
            txt.rectTransform.anchorMax=anchorMax??new Vector2(0.5f,0.5f);
            txt.rectTransform.pivot=pivot??new Vector2(0.5f,0.5f);
            txt.rectTransform.anchoredPosition=anchoredPos??Vector2.zero;
            return(textObj);
        }
        public static void AlertInput(string title,string description,Action<string>onResult){
            var canvasGO=new GameObject(RandomString());
            var canvas=canvasGO.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            var bg=new GameObject(RandomString());
            bg.transform.SetParent(canvasGO.transform);
            var bgImg=bg.AddComponent<Image>();
            bgImg.color=new Color(0,0,0,0.75f);
            var bgRT=bg.GetComponent<RectTransform>();
            bgRT.anchorMin=Vector2.zero;
            bgRT.anchorMax=Vector2.one;
            bgRT.offsetMin=Vector2.zero;
            bgRT.offsetMax=Vector2.zero;

            var panel=new GameObject(RandomString());
            panel.transform.SetParent(canvasGO.transform);
            var panelImg=panel.AddComponent<Image>();
            panelImg.color=Color.black;
            var panelRT=panel.GetComponent<RectTransform>();
            panelRT.sizeDelta=new Vector2(400,220);
            panelRT.anchorMin=new Vector2(0.5f,0.5f);
            panelRT.anchorMax=new Vector2(0.5f,0.5f);
            panelRT.pivot=new Vector2(0.5f,0.5f);
            panelRT.anchoredPosition=Vector2.zero;

            var titleGO=new GameObject(RandomString());
            titleGO.transform.SetParent(panel.transform);
            var titleText=titleGO.AddComponent<Text>();
            titleText.text=title;
            titleText.fontSize=20;
            titleText.font=loadedFont;
            titleText.color=Color.white;
            titleText.alignment=TextAnchor.UpperCenter;
            titleText.rectTransform.sizeDelta=new Vector2(380,30);
            titleText.rectTransform.anchoredPosition=new Vector2(0,-10);
            titleText.rectTransform.anchorMin=new Vector2(0.5f,1);
            titleText.rectTransform.anchorMax=new Vector2(0.5f,1);
            titleText.rectTransform.pivot=new Vector2(0.5f,1);

            var descGO=new GameObject(RandomString());
            descGO.transform.SetParent(panel.transform);
            var descText=descGO.AddComponent<Text>();
            descText.text=description;
            descText.font=loadedFont;
            descText.fontSize=14;
            descText.color=Color.white;
            descText.alignment=TextAnchor.UpperLeft;
            descText.rectTransform.sizeDelta=new Vector2(360,40);
            descText.rectTransform.anchoredPosition=new Vector2(0,-50);
            descText.rectTransform.anchorMin=new Vector2(0.5f,1);
            descText.rectTransform.anchorMax=new Vector2(0.5f,1);
            descText.rectTransform.pivot=new Vector2(0.5f,1);

            var inputGO=new GameObject(RandomString());
            inputGO.transform.SetParent(panel.transform);
            var inputRT=inputGO.AddComponent<RectTransform>();
            inputRT.sizeDelta=new Vector2(360,30);
            inputRT.anchoredPosition=new Vector2(0,-100);
            inputRT.anchorMin=new Vector2(0.5f,1);
            inputRT.anchorMax=new Vector2(0.5f,1);
            inputRT.pivot=new Vector2(0.5f,1);
            var inputImg=inputGO.AddComponent<Image>();
            inputImg.color=Color.black;
            var inputField=inputGO.AddComponent<InputField>();

            var inputTextGO=new GameObject(RandomString());
            inputTextGO.transform.SetParent(inputGO.transform);
            var inputText=inputTextGO.AddComponent<Text>();
            inputText.font=loadedFont;
            inputText.fontSize=14;
            inputText.color=Color.white;
            inputText.alignment=TextAnchor.MiddleLeft;
            inputField.textComponent=inputText;
            inputText.rectTransform.sizeDelta=new Vector2(360,30);
            inputText.rectTransform.anchoredPosition=Vector2.zero;

            var placeholderGO=new GameObject(RandomString());
            placeholderGO.transform.SetParent(inputGO.transform);
            var placeholder=placeholderGO.AddComponent<Text>();
            placeholder.text="Type here...";
            placeholder.font=loadedFont;
            placeholder.fontSize=14;
            placeholder.color=Color.gray;
            inputField.placeholder=placeholder;
            placeholder.alignment=TextAnchor.MiddleLeft;
            placeholder.rectTransform.sizeDelta=new Vector2(360,30);
            placeholder.rectTransform.anchoredPosition=Vector2.zero;
            
            var inputBlockerGO=UnityEngine.GameObject.FindObjectOfType<PlayerInputController>()?.gameObject;
            if(inputBlockerGO!=null)inputBlockerGO.SetActive(false);

            void Close(){
                if(inputBlockerGO!=null)inputBlockerGO.SetActive(true);
                UnityEngine.GameObject.Destroy(canvasGO);
            }

            void Resolve(bool ok){
                var val=inputField.text.Trim();
                Close();
                if(!ok||string.IsNullOrEmpty(val))onResult(null);
                else onResult(val);
            }

            var okBtn=Button(panel,new Position(120,30),"OK",()=>Resolve(true),
                anchorMin:new Vector2(1,0),
                anchorMax:new Vector2(1,0),
                pivot:new Vector2(1,0),
                anchoredPos:new Vector2(-10,10)
            );

            var cancelBtn=Button(panel,new Position(120,30),"Cancel",()=>Resolve(false),
                anchorMin:new Vector2(0,0),
                anchorMax:new Vector2(0,0),
                pivot:new Vector2(0,0),
                anchoredPos:new Vector2(10,10)
            );
            EventSystem.current.SetSelectedGameObject(inputGO);
        }
        public static void Confirmation(string title,string description,Action<bool>onResult){
            var canvasGO=new GameObject(RandomString());
            var canvas=canvasGO.AddComponent<Canvas>();
            canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();

            var bg=new GameObject(RandomString());
            bg.transform.SetParent(canvasGO.transform);
            var bgImg=bg.AddComponent<Image>();
            bgImg.color=new Color(0,0,0,0.75f);
            var bgRT=bg.GetComponent<RectTransform>();
            bgRT.anchorMin=Vector2.zero;
            bgRT.anchorMax=Vector2.one;
            bgRT.offsetMin=Vector2.zero;
            bgRT.offsetMax=Vector2.zero;

            var panel=new GameObject(RandomString());
            panel.transform.SetParent(canvasGO.transform);
            var panelImg=panel.AddComponent<Image>();
            panelImg.color=Color.black;
            var panelRT=panel.GetComponent<RectTransform>();
            panelRT.sizeDelta=new Vector2(400,180);
            panelRT.anchorMin=new Vector2(0.5f,0.5f);
            panelRT.anchorMax=new Vector2(0.5f,0.5f);
            panelRT.pivot=new Vector2(0.5f,0.5f);
            panelRT.anchoredPosition=Vector2.zero;

            var titleGO=new GameObject(RandomString());
            titleGO.transform.SetParent(panel.transform);
            var titleText=titleGO.AddComponent<Text>();
            titleText.text=title;
            titleText.fontSize=20;
            titleText.font=loadedFont;
            titleText.color=Color.white;
            titleText.alignment=TextAnchor.UpperCenter;
            titleText.rectTransform.sizeDelta=new Vector2(380,30);
            titleText.rectTransform.anchoredPosition=new Vector2(0,-10);
            titleText.rectTransform.anchorMin=new Vector2(0.5f,1);
            titleText.rectTransform.anchorMax=new Vector2(0.5f,1);
            titleText.rectTransform.pivot=new Vector2(0.5f,1);

            var descGO=new GameObject(RandomString());
            descGO.transform.SetParent(panel.transform);
            var descText=descGO.AddComponent<Text>();
            descText.text=description;
            descText.font=loadedFont;
            descText.fontSize=14;
            descText.color=Color.white;
            descText.alignment=TextAnchor.UpperLeft;
            descText.rectTransform.sizeDelta=new Vector2(360,60);
            descText.rectTransform.anchoredPosition=new Vector2(0,-50);
            descText.rectTransform.anchorMin=new Vector2(0.5f,1);
            descText.rectTransform.anchorMax=new Vector2(0.5f,1);
            descText.rectTransform.pivot=new Vector2(0.5f,1);

            var inputBlockerGO=UnityEngine.GameObject.FindObjectOfType<PlayerInputController>()?.gameObject;
            if(inputBlockerGO!=null)inputBlockerGO.SetActive(false);

            void Close(){
                if(inputBlockerGO!=null)inputBlockerGO.SetActive(true);
                UnityEngine.GameObject.Destroy(canvasGO);
            }

            void Resolve(bool res){
                Close();
                onResult(res);
            }

            Button(panel,new Position(120,30),"YES",()=>Resolve(true),
                anchorMin:new Vector2(1,0),
                anchorMax:new Vector2(1,0),
                pivot:new Vector2(1,0),
                anchoredPos:new Vector2(-10,10)
            );

            Button(panel,new Position(120,30),"NO",()=>Resolve(false),
                anchorMin:new Vector2(0,0),
                anchorMax:new Vector2(0,0),
                pivot:new Vector2(0,0),
                anchoredPos:new Vector2(10,10)
            );
        }
        public static GameObject GameObject()=>new GameObject(RandomString(12));
        public GameObject RootObject()=>root;
    }
}