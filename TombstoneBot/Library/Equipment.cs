using System;
using System.Linq;
using System.Text;
using Il2CppInventory;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Il2CppInventory.Items.Equipment;
namespace TombstoneAI.Library
{
    public class Equipment
    {
        static readonly object _lockItems=new object();
        static readonly object _lockShared=new object();
        static Il2CppEntities.Players.PlayerEquipment _shared;
        static List<NetInventoryItem>_items=new List<NetInventoryItem>();
        public static void SetItems(Il2CppFishNet.Object.Synchronizing.SyncList<NetInventoryItem>items){
            var list=new List<NetInventoryItem>();
            for(int i=0;i<items.Count;i++)list.Add(items[i]);
            lock(_lockItems)_items=list;
        }
        public static List<NetInventoryItem>GetItems(){
            lock(_lockItems)return(_items);
        }
        public static void Set(Il2CppEntities.Players.PlayerEquipment shared){
            lock(_lockShared)_shared=shared;
        }
        public static Il2CppEntities.Players.PlayerEquipment Get(){
            lock(_lockShared)return(_shared);
        }
        public Equipment(Il2CppEntities.Players.Player player){
            Equipment.SetItems(player.Equipment.Equips);
            Equipment.Set(player.Equipment);
            Hook();
        }
        public static void EquipmentItemsPoll(){
            var inv=Equipment.Get().Equips;
            if(inv!=null){
                Equipment.SetItems(inv);
            }
        }
        void Hook(){
            Timer t=new Timer(Equipment.EquipmentItemsPoll,1E3,true);
            t.Start();
        }
        public bool hasEquipment(string name){return(this.isEquipping(name));}
        public bool isEquipping(string name){
            name=name.ToLower();
            var items=Equipment.GetItems();
            foreach(NetInventoryItem i in items){
                ///if(i?.Item?.name?.ToLower()==name)return(true);
            }
            return(false);
        }
    }
}