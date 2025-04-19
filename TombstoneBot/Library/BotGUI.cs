using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
namespace TombstoneAI.Library
{
    public static class BotGUI
    {
        static bool isGuiOpen;
        static readonly object _lockisGuiOpen=new object();
        public static void open(){
            lock(_lockisGuiOpen)isGuiOpen=true;
        }
        public static void close(){
            lock(_lockisGuiOpen)isGuiOpen=false;
        }
        public static bool isOpen(){
            lock(_lockisGuiOpen)return(isGuiOpen);
        }
        public static void Button(Rect position,string text,Action callback){
            if(GUI.Button(position,text))callback();
        }
    }
}