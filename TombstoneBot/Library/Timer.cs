using MelonLoader;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Timer{
    bool loop;
    float seconds;
    object coroutine;
    System.Action callback;
    Dictionary<string,object>data=new Dictionary<string,object>();
    /// <summary>
    /// Creates a timer that waits for a specified duration, then invokes a callback.
    /// </summary>
    /// <param name="callback">The method to call after the delay.</param>
    /// <param name="milliseconds ">Delay in milliseconds between calls.</param>
    /// <param name="interval">If true, the callback repeats at each interval.</param>
    public Timer(System.Action callback,double milliseconds,bool interval=false){
        this.seconds=(float)milliseconds/1000f;
        this.callback=callback;
        this.loop=interval;
    }
    public void Start(){
        if(coroutine!=null)Stop();
        coroutine=MelonCoroutines.Start(Run());
    }
    public void Stop(){
        if(coroutine!=null){
            this.loop=false;
            MelonCoroutines.Stop(coroutine);
        }
    }
    public void SetDelay(double milliseconds){
        this.seconds=(float)milliseconds/1000f;
    }
    public void SetValue(string key,object value){
        data[key]=value;
    }
    public object GetValue(string key){
        return(data.TryGetValue(key,out var value)?value:null);
    }
    private IEnumerator Run(){
        do{
            yield return new WaitForSeconds(seconds);
            callback?.Invoke();
        }while(loop);
    }
}