using System;
using System.Linq;
using System.Text;
using Il2CppInventory;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Il2CppInventory.Items.Components;
using static TombstoneAI.Library.Datastub;
using Il2CppTileObjects.ItemDrops;
namespace TombstoneAI.Library
{
    public class Inventory
    {
        public class Item{
            private int index;
            private InventoryItem item;
            private Datastub.MaterialType materialType;
            public Item(InventoryItem item){
                this.materialType=Datastub.GetMaterialType(item.Item.name.Split(' ')[0]);
                this.item=item;
                this.index=-1;
            }
            public Item(InventoryItem item,int index){
                this.materialType=Datastub.GetMaterialType(item.Item.name.Split(' ')[0]);
                this.index=index;
                this.item=item;
            }
            public int getSlot(){return(this.index);}
            public InventoryItem get(){return(this.item);}
            public MaterialType getMaterialType(){return(this.materialType);}
        }
        static readonly object _lockItems=new object();
        static readonly object _lockShared=new object();
        static Il2CppEntities.Players.PlayerInventory _shared;
        static List<InventoryItem>_items=new List<InventoryItem>();
        public static void SetItems(Il2CppSystem.Collections.Generic.List<InventoryItem>items){
            var list=new List<InventoryItem>();
            for(int i=0;i<items.Count;i++)list.Add(items[i]);
            lock(_lockItems)_items=list;
        }
        public static List<InventoryItem>GetItems(){
            lock(_lockItems)return(_items);
        }
        public static void Set(Il2CppEntities.Players.PlayerInventory shared){
            lock(_lockShared)_shared=shared;
        }
        public static Il2CppEntities.Players.PlayerInventory Get(){
            lock(_lockShared)return(_shared);
        }
        public Inventory(Il2CppEntities.Players.Player player){
            Inventory.SetItems(player.Inventory.LoadedItems);
            Inventory.Set(player.Inventory);
            Hook();
        }
        public static void InventoryItemsPoll(){
            var inv=Inventory.Get().LoadedItems;
            if(inv!=null){
                Inventory.SetItems(inv);
            }
        }
        void Hook(){
            Timer t=new Timer(Inventory.InventoryItemsPoll,1E3,true);
            t.Start();
        }
        public int FreeSpace(){
            return(Inventory.Get().CountFreeSpace());
        }
        public bool isFull(){
            return(Inventory.Get().IsFull());
        }
        public bool isEmpty(){
            return(Inventory.Get().IsEmpty());
        }
        public bool hasItem(InventoryItem item,bool includeEquipped){
            return(Inventory.Get().HasItem(item,includeEquipped));
        }
        public bool hasItem(NetInventoryItem item,bool includeEquipped){
            return(Inventory.Get().HasItem(item,includeEquipped));
        }
        public bool hasItem(int slot){
            return(Inventory.Get().HasQuantity(slot,1));
        }
        public int caps(){
            return(Inventory.Get().GetCaps());
        }
        public bool hasQuantity(int slot,int amount=1){
            return(Inventory.Get().HasQuantity(slot,amount));
        }
        public bool hasItem(string name){
            if(string.IsNullOrEmpty(name))return(false);
            var items=Inventory.GetItems();
            name=name.ToLower();
            foreach(InventoryItem i in items){
                if(i?.Item?.name?.ToLower()==name)return(true);
            }
            return(false);
        }
        public bool hasItemPrecise(string name){
            if(string.IsNullOrEmpty(name))return(false);
            var items=Inventory.GetItems();
            name=name.ToLower();
            foreach(InventoryItem i in items){
                if(i?.Item?.name==name)return(true);
            }
            return(false);
        }
        public bool hasQuantity(string name,int amount=1){
            if(string.IsNullOrEmpty(name))return(false);
            if(amount==1)return(this.hasItem(name));
            var items=Inventory.GetItems();
            name=name.ToLower();
            int R=0;
            foreach(InventoryItem i in items){
                if(i?.Item?.name?.ToLower()==name){
                    if(amount==0)return(false);
                    R+=i.Quantity;
                }
                if(R>=amount)return(true);
            }
            return(R>=amount);
        }
        public bool hasItem(string name,bool containsName){
            if(!containsName)return(this.hasItem(name));
            if(string.IsNullOrEmpty(name))return(false);
            name=name.ToLower();
            var items=Inventory.GetItems();
            foreach(InventoryItem i in items){
                var R=i?.Item?.name?.ToLower();
                if(!string.IsNullOrEmpty(R)&&R.Contains(name))return(true);
            }
            return(false);
        }
        public Item getItem(string name,bool containsName=false){
            if(string.IsNullOrEmpty(name))return(null);
            int x=-1;
            name=name.ToLower();
            var items=Inventory.GetItems();
            foreach(InventoryItem i in items){
                x++;
                var R=i?.Item?.name?.ToLower();
                if((!containsName&&R==name)||(containsName&&!string.IsNullOrEmpty(R)&&R.Contains(name)))return new Item(i,x);
            }
            return(null);
        }
        public Item[] getItems(string name,bool containsName=false){
            if(string.IsNullOrEmpty(name))return(null);
            int x=-1;
            name=name.ToLower();
            var A=new List<Item>();
            var items=Inventory.GetItems();
            foreach(InventoryItem i in items){
                x++;
                var R=i?.Item?.name?.ToLower();
                if((!containsName&&R==name)||(containsName&&!string.IsNullOrEmpty(R)&&R.Contains(name)))A.Add(new Item(i,x));
            }
            return(A.Count==0?null:A.ToArray());
        }
        public static Item[] GetAllItems(){
            int x=0;
            var A=new List<Item>();
            var items=Inventory.GetItems();
            foreach(InventoryItem i in items){
                A.Add(new Item(i,x++));
            }
            return(A.Count==0?null:A.ToArray());
        }
        public bool hasQuantity(string name,int amount,bool containsName){
            if(!containsName)return(this.hasQuantity(name,amount));
            if(string.IsNullOrEmpty(name))return(false);
            if(amount==1)return(this.hasItem(name));
            var items=Inventory.GetItems();
            name=name.ToLower();
            int R=0;
            foreach(InventoryItem i in items){
                if((i?.Item?.name?.ToLower()).Contains(name)){
                    if(amount==0)return(false);
                    R+=i.Quantity;
                }
                if(R>=amount)return(true);
            }
            return(R>=amount);
        }
        public int getQuantity(string name){
            int R=0;
            name=name.ToLower();
            var items=Inventory.GetItems();
            foreach(InventoryItem i in items){
                if(i?.Item?.name?.ToLower()==name)R+=i.Quantity;
            }
            return(R);
        }
        public int getSlot(string name){
            int x=0;
            name=name.ToLower();
            var items=Inventory.GetItems();
            foreach(InventoryItem i in items){
                if(i?.Item?.name?.ToLower()==name)return(x);
                x++;
            }
            return-1;
        }
        public void dropItem(int slot){
            byte b=(byte)slot;
            _shared.CmdDrop(b);
        }
        public void dropAll(){
            for(byte b=0;b<28;b++){
                _shared.CmdDrop(b);
            }
        }
        public bool canLoot(ItemDrop itemDrop){
            if(itemDrop==null||!this.isFull())return(true);
            var name=itemDrop.InvItem.Item.name;
            var item=this.getItem(name);
            if(item==null)return(false);
            var it=item.get();
            var i=it.Item;
            if(i.name=="Primer Caps")return(true);
            if(!i.IsStackable||it.IsNoted)return(false);
            if(i.MaxStack<2)return(true);
            if(i.MaxStack==it.Quantity){
                var items=this.getItems(name);
                foreach(var itm in items){
                    var itt=itm.get();
                    var ii=itt.Item;
                    if((itt.Quantity+itemDrop.InvItem.Quantity)<ii.MaxStack)return(true);
                }
            }
            return(false);
        }
    }
}