using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Il2CppTileObjects.Variables;
namespace TombstoneAI.Library
{
    public class DoopstrapV1
    {
        public DoopstrapV1(){}
        private static Position getResolution()
        {
            var R=Il2CppGameOptions.Settings.GetOption(Il2CppGameOptions.GameOption.ScreenResolution).ToString().Split('x');
            return new Position(Int32.Parse(R[0]),Int32.Parse(R[1]));
        }
        private protected static float[]COLUMNS={8.3333f,16.6667f,25f,33.3333f,41.6667f,50f,58.3333f,66.6667f,75f,83.3333f,91.6667f,100f};
        public static Rect col(int width,int height=0,float marginWidthPercentage=0,float marginHeightPercentage=0){
            return(col(width,height,marginWidthPercentage,marginHeightPercentage,new Rect(0,0,0,0)));
        }
        public static Rect col(int width,int height,float marginWidthPercentage,Rect parentElement){
            return(col(width,height,marginWidthPercentage,0,parentElement));
        }
        public static Rect col(int width,int height,Rect parentElement){
            return(col(width,height,0,0,parentElement));
        }
        public static Rect col(int width,Rect parentElement){
            return(col(width,0,0,0,parentElement));
        }
        public static Rect col(int width,int height=0,float marginWidthPercentage=0,float marginHeightPercentage=0,object parentElement=null){
            bool daehp=false;
            Rect parent=(Rect)parentElement;
            if(width<1)width=1;else if(width>12)width=12;
            if(height<1)height=width;else if(height>12)height=12;
            int a=marginWidthPercentage<0?(int)Math.Abs(marginWidthPercentage):1;
            bool marginXWasNegative=marginWidthPercentage<0&&marginWidthPercentage>=-12;
            if(marginXWasNegative){
                if(marginWidthPercentage<-1)daehp=true;
                int p=Math.Abs((int)marginWidthPercentage)-2;
                marginWidthPercentage=p<0?0:COLUMNS[p];
            }
            if(marginHeightPercentage<0&&marginHeightPercentage>=-12){
                int p=Math.Abs((int)marginHeightPercentage)-2;
                marginHeightPercentage=p<0?0:COLUMNS[p];
            }else if(marginHeightPercentage==0&&!marginXWasNegative)marginHeightPercentage=marginWidthPercentage;
            Position R=getResolution();
            float w=COLUMNS[width-1];
            float h=COLUMNS[height-1];
            float W=R.x*(w/100f);
            float H=R.y*(h/100f);
            if(parent.size.x!=0)W=parent.size.x*(w/100f);
            if(parent.size.y!=0)H=parent.size.y*(h/100f);
            float X=R.y*(marginWidthPercentage/100f);
            float Y=R.x*(marginHeightPercentage/100f);
            if(parent.y!=0)X+=parent.y;
            if(!daehp)X+=(R.y*(3.33f/100f)*a);
            if(parent.x!=0){
                var padding=R.x*(3.33f/100f);
                Y+=parent.x+padding;
                W-=padding*2;
            }
            return new Rect(Y,X,W,H);
        }
        public static Rect ProgressionBox(string title,string text,bool focus=false,bool draw=true){
            int fontSize=16;
            float padding=10f;
            var style=new GUIStyle();
            style.normal=GUI.skin.box.normal;
            style.border=GUI.skin.box.border;
            style.alignment=GUI.skin.box.alignment;
            style.padding=GUI.skin.box.padding;
            style.margin=GUI.skin.box.margin;
            style.fontSize=fontSize;
            style.wordWrap=true;
            var titleContent=new GUIContent(title);
            var textContent=new GUIContent(text);
            var titleSize=style.CalcSize(titleContent);
            titleSize.x/=2;
            titleSize.y/=2;
            var textSize=style.CalcSize(textContent);
            float width=Mathf.Max(titleSize.x,textSize.x)+padding*2;
            float height=titleSize.y+textSize.y+padding*3;
            var rect=col(3,3,-3,3);
            rect.width=width;
            rect.height=height;
            if(!draw)return(rect);
            if(focus){
                var bg=new Texture2D(1,1);
                var color=new Color(0,0,0,0.9f);
                bg.SetPixel(0,0,color);
                bg.Apply();
                style.normal.background=bg;
            }
            GUI.Box(rect,title,style);
            style.normal.background=null;
            style.alignment=TextAnchor.MiddleLeft;
            GUI.Label(new Rect(rect.x+padding,rect.y+padding+titleSize.y+padding,textSize.x,textSize.y),text,style);
            return(rect);
        }
    }
}