using System;
using System.Linq;
using System.Text;
using Il2CppInventory;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MelonLoader;
using Il2CppInventory.Items;
using UnityEngine;
using Il2CppTombstone.Banking;
using Il2CppSystem.Linq;
namespace TombstoneAI.Library
{
    public class Bank
    {
        public class Tab
        {
            int index;
            List<NetInventoryItem>items;
            public Tab(int index,List<NetInventoryItem>items){
                this.index=index;
                this.items=items;
            }
            public int getIndex(){return(this.index);}
            public List<NetInventoryItem> getItems(){return(this.items);}
        }
        static bool sayOnce;///DEBUG
        public Player player;
        static Inventory _inventory;
        static List<Tab>_items=new List<Tab>();
        static readonly object _lockItems=new object();
        static readonly object _lockShared=new object();
        static Il2CppEntities.Players.PlayerBank _shared;
        static readonly object _lockInventory=new object();
        public static void SetItems(Il2CppFishNet.Object.Synchronizing.SyncList<Il2CppSystem.Collections.Generic.List<NetInventoryItem>>items){
            var list=new List<Tab>();/*TODO: Figure out how to get the damned InventoryItem(s)*/
            var iitems=GameObject.FindObjectsOfType<Item>();
            if(!sayOnce){
                foreach(var item in iitems){
                    MelonLogger.Msg($"Item: {item.name} ({item._guid})");
                }
                sayOnce=true;
            }
            for(int i=0;i<items.Count;i++){
                var item=items[i];
                var L=new List<NetInventoryItem>();
                for(int j=0;j<item.Count;j++){
                    var netItem=item[j];
                    //var data=new IItemData(netItem.Pointer);
                    //data=data.FromJson(netItem.DataJson);
                    //var itm=iitems.FirstOrDefault(x=>{x.==netItem.ItemId);
                    //InventoryItem inv=new InventoryItem(itm,netItem.Quantity,netItem.IsNoted,data);
                    L.Add(netItem);
                }
                list.Add(new Tab(i,L));
            }
            lock(_lockItems)_items=list;
        }
        public static List<Tab>GetItems(){
            lock(_lockItems)return(_items);
        }
        public static void Set(Il2CppEntities.Players.PlayerBank shared){
            lock(_lockShared)_shared=shared;
        }
        public static Il2CppEntities.Players.PlayerBank Get(){
            lock(_lockShared)return(_shared);
        }
        public static void SetInventory(Inventory inventory){
            lock(_lockInventory)_inventory=inventory;
        }
        public static Inventory GetInventory(){
            lock(_lockInventory)return(_inventory);
        }
        public Bank(Il2CppEntities.Players.Player player,Inventory inventory){
            Bank.SetItems(player.Bank.Items);
            Bank.SetInventory(inventory);
            Bank.Set(player.Bank);
            Hook();
        }
        public static void BankItemsPoll(){
            var inv=Bank.Get().Items;
            if(inv!=null){
                Bank.SetItems(inv);
            }
        }
        void Hook(){
            //Timer t=new Timer(Bank.BankItemsPoll,1E3,true);
            //t.Start();
        }
        public static bool inBank(){return(_shared.InBank);}
        public static bool DepositAll(){
            if(!inBank())return(false);
            var bank=Bank.Get();
            try{
                bank.CmdDepositAllInventory(0);
                return(true);
            }catch{return(false);}
        }
        public static bool Deposit(string name,int quantity=1,bool containsName=false){
            if(!inBank())return(false);
            var bank=Bank.Get();
            if(quantity<0)quantity=1;
            var inv=Bank.GetInventory();
            var i=inv.getItem(name,containsName);
            if(i==null)return(false);
            try{
                if(quantity==0){
                    var it=i.get();
                    bank.CmdDepositAllOfItem(0,it.Item.Guid);
                }else{
                    bank.CmdDeposit(0,(byte)i.getSlot(),quantity);
                }
                return(true);
            }catch{return(false);}
        }
        public static bool DepositAllExceptTools(bool exceptCaps=false){
            if(!inBank())return(false);
            var bank=Bank.Get();
            var i=Inventory.GetItems();
            var inv=Bank.GetInventory();
            if(i==null||i.Count==0)return(false);
            try{
                foreach(var item in i){
                    if(item.Item.IsTool()||(exceptCaps&&item.Item.name=="Primer Caps"))continue;
                    bank.CmdDepositAllOfItem(0,item.Item.Guid);
                }
                return(true);
            }catch{return(false);}
        }
        public static bool DepositAllExceptCaps(){
            if(!inBank())return(false);
            var bank=Bank.Get();
            var i=Inventory.GetItems();
            var inv=Bank.GetInventory();
            if(i==null||i.Count==0)return(false);
            try{
                foreach(var item in i){
                    if(item.Item.name=="Primer Caps")continue;
                    bank.CmdDepositAllOfItem(0,item.Item.Guid);
                }
                return(true);
            }catch{return(false);}
        }
        public static bool Withdraw(string name,int quantity=1,bool containsName=false){
            /*TODO: Once we figure out how to get the InventoryItem(s) we can withdraw too!*/
            return(false);
        }
    }
}