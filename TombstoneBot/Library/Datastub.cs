using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Il2CppEntities.Players;
using Il2CppTileObjects.SkillingNodes;
using UnityEngine;
using Il2CppEntities.Stats;
using UnityEngine.Serialization;
using Il2CppTileObjects.Crafting;
using System.Runtime.CompilerServices;
using MelonLoader;
using Il2CppInventory.Items.Equipment;
using Il2CppEntities.Enemies;
using Il2CppTileObjects.Npcs;
using System.Xml.Linq;
using Il2CppTileObjects.ItemDrops;
using Harmony;
namespace TombstoneAI.Library
{
    public static class Datastub
    {
        public enum IslandType{None,Trailhead,Boia,Ridgewood}
        public enum MaterialType{None,Pine,Oak,Hickory,Willow,Birch,Ebony}
        public static IslandType GetIslandType(string islandName){
            return(Enum.TryParse(islandName,out IslandType type)?type:IslandType.None);
        }
        public static MaterialType GetMaterialType(string materialName){
            return(Enum.TryParse(materialName,out MaterialType type)?type:MaterialType.None);
        }
        public static string[]GetIslandNames(string suffix,bool reverseArray=false){
            var R=Enum.GetValues(typeof(IslandType))
                .Cast<IslandType>()
                .Where(x=>x!=IslandType.None)
                .Select(x=>x+" "+suffix);
            if(reverseArray)R=R.Reverse();
            return(R.ToArray());
        }
        public static string[]GetMaterialNames(string suffix,bool reverseArray=false){
            var R=Enum.GetValues(typeof(MaterialType))
                .Cast<MaterialType>()
                .Where(x=>x!=MaterialType.None)
                .Select(x=>x+" "+suffix);
            if(reverseArray)R=R.Reverse();
            return(R.ToArray());
        }
        public static class Trees
        {
            public static int[]Levels={60,48,36,24,12,1};
            public static string[]Names=GetMaterialNames("Tree",true);
            public static int GetLevel(MaterialType materialType){
                for(int x=0;x<Names.Length;x++){
                    if(Names[x]==materialType.ToString()){
                        return(Levels[x]);
                    }
                }
                return(0);
            }
            public static string[]Available(Player player){
                var R=new List<string>();
                int L=player.getLevel(Stat.Survival);
                for(int x=0;x<Names.Length;x++){
                    if(Levels[x]<=L)R.Add(Names[x]);
                }
                return(R.Count==0?null:R.ToArray());
            }
            public static SkillingNode[]FindAll(MaterialType materialType){
                var R=new List<SkillingNode>();
                var name=materialType.ToString()+" Tree";
                var F=GameObject.FindObjectsOfType<SkillingNode>();
                if(F.Count==0)return(null);
                foreach(var i in F){
                    if(i==null||!i.gameObject.active||!i.gameObject.activeInHierarchy||!i.IsSpawned||!i.isActiveAndEnabled)continue;
                    if(i.gameObject.name==name)R.Add(i);
                }
                return(R.Count==0?null:R.ToArray());
            }
            public static SkillingNode isActive(Position position){
                var F=GameObject.FindObjectsOfType<SkillingNode>();
                if(F.Count==0)return(null);
                foreach(var i in F){
                    if(i==null||!i.gameObject.active||!i.gameObject.activeInHierarchy||!i.IsSpawned||!i.isActiveAndEnabled)continue;
                    if(position.Equals(i.TilePosition))return(i);
                }
                return(null);
            }
            public static SkillingNode Find(Player player){
                float max=1E3f;
                bool fbreak=false;
                SkillingNode node=null;
                var R=Available(player);
                if(R==null)return(null);
                var F=GameObject.FindObjectsOfType<SkillingNode>();
                if(F.Count==0)return(null);
                var pos=player.getPosition();
                foreach(var i in F){
                    if(i==null)continue;
                    var A=pos.Distance(i.TilePosition);
                    for(int x=0;x<R.Length;x++){
                        if(!i.gameObject.active||!i.gameObject.activeInHierarchy||!i.IsSpawned||!i.isActiveAndEnabled||i.gameObject.name!=R[x])continue;
                        if(A<3){
                            fbreak=true;
                            node=i;
                            break;
                        }
                        if(A<max){
                            max=A;
                            node=i;
                        }
                    }
                    if(fbreak)break;
                }
                return(node);
            }
        }
        public static class WorkbenchesOld
        {
            public static Dictionary<CraftType,string>Names=new Dictionary<CraftType,string>(){
                {CraftType.Anvil,"Anvil"},
                {CraftType.CampfireLarge,"Campfire"},
                {CraftType.CampfireSmall,"Campfire"},
                {CraftType.DryingRack,"Drying Rack"},
                {CraftType.FletchingBench,"Fletching Bench"},
                {CraftType.Furnace,"Furnace"},
                {CraftType.Inventory,null},
                {CraftType.JewelryBench,"JewelryMaking Bench"},
                {CraftType.Kitchen,"Cooking Range"},
                {CraftType.LeatherWorkbench,"Leatherworking Bench"},
                {CraftType.Loom,"Loom"},
                {CraftType.MixingTable,"Mixing Table"},
                {CraftType.Sawmill,"Sawmill"},
                {CraftType.SoakingTub,"Soaking Tub"},
                {CraftType.SpinningWheel,"Spinning Wheel"},
                {CraftType.TinkeringBench,"Tinkering Bench"},
                {CraftType.WoodworkingBench,"Woodworking Bench"}};
            public static AutoCrafter Find(Player player,CraftType craftType){
                Names.TryGetValue(craftType,out string R);
                if(R==null)return(null);
                AutoCrafter closest=null;
                var A=new List<AutoCrafter>();
                var F=GameObject.Find(R);
                if(F!=null)closest=F.GetComponent<AutoCrafter>();
                return(closest);
            }
        }
        public static class Wells
        {
            private static Dictionary<IslandType,List<Position>>positions=new Dictionary<IslandType,List<Position>>{
            {IslandType.Ridgewood,new List<Position>{
            }},
            {IslandType.Boia,new List<Position>{
                new Position(313,2),///SALADO
                new Position(447,-342),///FRONTIER
                new Position(211,-423),///CALDWELL
            }},
            {IslandType.Trailhead,new List<Position>{
                new Position(-41,43),///NEW WILLIAMSBURG
            }}};
            public static Position Find(Player player,int maxDistance=150){
                Position R=null;
                var pos=player.getPosition();
                var islandType=player.getIsland();
                float max=maxDistance<1?1E6f:maxDistance;
                positions.TryGetValue(islandType,out var poss);
                if(poss==null)return(null);
                foreach(var position in poss){
                    var distance=pos.Distance(position);
                    if(distance<=max){
                        max=distance;
                        R=position;
                    }
                }
                return(R);
            }
        }
        public static class Banks
        {
            private static Dictionary<IslandType,List<Position>>positions=new Dictionary<IslandType,List<Position>>{
            {IslandType.Ridgewood,new List<Position>{
                new Position(112,97),///NORTH
                new Position(-123,-14),///JUNIPER
                new Position(47,-161),///BANDIT CAMP
            }},
            {IslandType.Boia,new List<Position>{
                new Position(-77,317),///NORTHWELL
                new Position(185,256),///ODDYSEY
                new Position(87,99),///BAYOU
                new Position(302,-8),///SALADO
                new Position(633,31),///RUSTWATER
                new Position(296,-146),///CHERYTOL
                new Position(431,-302),///FRONTIER (NORTH)
                new Position(468,-349),///FRONTIER (SOUTH)
                new Position(471,-349),///FRONTIER (SOUTH)
                new Position(211,-411),///CALDWELL
            }},
            {IslandType.Trailhead,new List<Position>{
                new Position(-90,51),///NEW WILLIAMSBURG
            }}};
            public static Position Find(Player player,int maxDistance=150){
                Position R=null;
                var pos=player.getPosition();
                var islandType=player.getIsland();
                float max=maxDistance<1?1E6f:maxDistance;
                positions.TryGetValue(islandType,out var poss);
                if(poss==null)return(null);
                foreach(var position in poss){
                    var distance=pos.Distance(position);
                    if(distance<=max){
                        max=distance;
                        R=position;
                    }
                }
                return(R);
            }
        }
        public static class Workbenches
        {
            private static Dictionary<CraftType,Dictionary<IslandType,List<Position>>>positions=new Dictionary<CraftType,Dictionary<IslandType,List<Position>>>(){
                {CraftType.Anvil,
                new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(201,256),///ODDYSEY
                        new Position(326,148),///CHERYTOL
                        new Position(440,-280),///FRONTIER
                        new Position(201,-421),///CALDWELL
                    }},
                    {IslandType.Trailhead,new List<Position>{
                        new Position(-64,52),///NEW WILLIAMSBURG
                    }}
                }},
                {CraftType.CampfireLarge,null},
                {CraftType.CampfireSmall,null},
                {CraftType.DryingRack,
                new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(132,116),///BAYOU
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(85,79),///BANDIT CAMP
                    }}
                }},
                {CraftType.FletchingBench,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(188,290),///ODDYSEY
                        new Position(470,-286)///FRONTIER
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(96,100),///NORTH
                    }}
                }},
                {CraftType.Furnace,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(597,26),///RUSTWATER
                        new Position(438,-282),///FRONTIER
                    }},
                    {IslandType.Trailhead,new List<Position>{
                        new Position(-67,53),///NEW WILLIAMSBURG
                        new Position(25,-51),///NEW WILLIAMSBURG
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(-101,-20),///JUNIPER
                    }}
                }},
                {CraftType.Inventory,null},
                {CraftType.JewelryBench,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(319,-148),///CHERYTOL
                        new Position(511,-327),///FRONTIER
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(29,-168),///BANDIT CAMP
                    }}
                }},
                {CraftType.Kitchen,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(119,84),///BAYOU
                        new Position(316,18),///SALADO (NORTH)
                        new Position(316,-6),///SALADO (SOUTH)
                        new Position(312,-146),///CHERYTOL
                        new Position(411,-327),///FRONTIER
                    }},
                    {IslandType.Trailhead,new List<Position>{
                        new Position(-63,61),///NEW WILLIAMSBURG
                        new Position(-15,-41),///NEW WILLIAMSBURG
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(-84,-29),///JUNIPER
                    }}
                }},
                {CraftType.LeatherWorkbench,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(124,121),///BAYOU
                        new Position(335,12),///SALADO
                        new Position(401,-345),///FRONTIER
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(84,82),///NORTH
                    }}
                }},
                {CraftType.Loom,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(335,-149),///CHERYTOL
                        new Position(415,-316),///FRONTIER
                    }},
                    {IslandType.Trailhead,new List<Position>{
                        new Position(10,-48),///NEW WILLIAMSBURG
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(18,-166),///BANDIT CAMP
                    }}
                }},
                {CraftType.MixingTable,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(-20,316),///NORTHWELL
                        new Position(490,-289),///FRONTIER
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(-89,-54),///JUNIPER
                    }}
                }},
                {CraftType.Sawmill,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(213,260),///ODDYSEY
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(55,-143),///BANDIT CAMP
                    }}
                }},
                {CraftType.SoakingTub,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(127,116),///BAYOU
                        new Position(405,-353),///FRONTIER
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(80,78),///NORTH
                    }}
                }},
                {CraftType.SpinningWheel,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(107,122),///BAYOU
                        new Position(615,31),///RUSTWATER
                        new Position(403,-334),///FRONTIER
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(15,-166),///BANDIT CAMP
                    }}
                }},
                {CraftType.TinkeringBench,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(-12,309),///NORTHWELL
                        new Position(157,273),///ODDYSEY
                        new Position(505,-353),///FRONTIER
                    }},
                    {IslandType.Trailhead,new List<Position>{
                        new Position(19,-55),///NEW WILLIAMSBURG
                    }},
                    {IslandType.Ridgewood,new List<Position>{
                        new Position(57,-167),///BANDIT CAMP
                    }}
                }},
                {CraftType.WoodworkingBench,
                    new Dictionary<IslandType,List<Position>>{
                    {IslandType.Boia,new List<Position>{
                        new Position(119,103),///BAYOU
                        new Position(591,32),///RUSTWATER
                    }}
                }}};
            public static Position Find(Player player,CraftType craftType,int maxDistance=150){
                positions.TryGetValue(craftType,out var poss1);
                if(poss1==null)return(null);
                var islandType=player.getIsland();
                poss1.TryGetValue(islandType,out var poss);
                Position R=null;
                if(poss==null)return(null);
                var pos=player.getPosition();
                float max=maxDistance<1?1E6f:maxDistance;
                foreach(var position in poss){
                    var distance=pos.Distance(position);
                    if(distance<=max){
                        max=distance;
                        R=position;
                    }
                }
                return(R);
            }
        }
        public static class Skills
        {
            private static Dictionary<Stat,string>Names=new Dictionary<Stat,string>{
                {Stat.Agriculture,"Agriculture"},
                {Stat.Bounty,"Bounty"},
                {Stat.Cooking,"Cooking"},
                {Stat.Defense,"Defense"},
                {Stat.Fishing,"Fishing"},
                {Stat.Harvesting,"Harvesting"},
                {Stat.Health,"Health"},
                {Stat.Hunting,"Hunting"},
                {Stat.JewelryMaking,"Lapidary"},
                {Stat.Karma,"Karma"},
                {Stat.Leatherwork,"Leatherwork"},
                {Stat.Melee,"Melee"},
                {Stat.Mining,"Mining"},
                {Stat.Mixology,"Mixology"},
                {Stat.Movement,"Movement"},
                {Stat.Ranged,"Ranged"},
                {Stat.Robbery,"Robbery"},
                {Stat.Smithing,"Smithing"},
                {Stat.Survival,"Survival"},
                {Stat.Tailor,"Tailor"},
                {Stat.Technomancy,"Technomancy"},
                {Stat.Tinkerer,"Tinkerer"},
                {Stat.Woodwork,"Woodwork"}
            };
            public static string Find(Stat stat){
                return(Names.TryGetValue(stat,out var R)?R:stat.ToString());
            }
        }
        public static class Fishing
        {
            public enum FishSpotType
            {
                TRAP,
                ROD
            }
            public static SkillingNode Find(Player player,FishSpotType fishSpotType){
                float max=1E6f;
                SkillingNode R=null;
                var pos=player.getPosition();
                var nodes=GameObject.FindObjectsOfType<SkillingNode>();
                foreach(var node in nodes){
                    if(node.NodeData.ToolReq!=(fishSpotType==FishSpotType.TRAP?ToolFlags.FishingTrap:ToolFlags.FishingRod))continue;
                    var A=pos.Distance(new Position(node.TilePosition));
                    if(A<max){
                        max=A;
                        R=node;
                    }
                }
                return(R);
            }
        }
        public static class Enemies
        {
            public static Enemy Find(Player player,params string[]names){
                return(Find(player,names));
            }
            public static Enemy Find(Player player,bool includeShiny=false,params string[]names){
                return(Find(player,names,includeShiny));
            }
            private static Enemy Find(Player player,string[]names,bool includeShiny=false){
                var nodes=GameObject.FindObjectsOfType<Enemy>();
                var pos=player.getPosition();
                var len=names.Length;
                bool fbreak=false;
                float max=1E6f;
                Enemy R=null;
                if(len==1){
                    var name=names[0].ToLower();
                    foreach(var node in nodes){
                        if(!node.IsSpawned||!node.isActiveAndEnabled||node.Health.CurrentHealth==0||node.Health.CurrentHealth<node.Health.MaxHealth)continue;
                        var n=node.name.ToLower();
                        var rare=n==(name+" (rare)");
                        if(n!=name&&((includeShiny&&!rare)||(!includeShiny&&rare)))continue;
                        if(rare){
                            R=node;
                            break;
                        }
                        var A=pos.Distance(new Position(node.TilePosition));
                        if(A<3){
                            R=node;
                            break;
                        }
                        if(A<max){
                            max=A;
                            R=node;
                        }
                    }
                }else{
                    for(int x=0;x<names.Length;x++)names[x]=names[x].ToLower();
                    foreach(var node in nodes){
                        if(!node.IsSpawned||!node.isActiveAndEnabled||node.Health.CurrentHealth==0||node.Health.CurrentHealth<node.Health.MaxHealth)continue;
                        var poss=new Position(node.TilePosition);
                        var name=node.name.ToLower();
                        var A=pos.Distance(poss);
                        for(int x=0;x<names.Length;x++){
                            var rare=name==(names[x]+" (rare)");
                            if(name!=names[x]&&((includeShiny&&!rare)||(!includeShiny&&rare)))continue;
                            if(rare||A<3){
                                fbreak=true;
                                R=node;
                                break;
                            }
                            if(A<max){
                                max=A;
                                R=node;
                            }
                        }
                        if(fbreak)break;
                    }
                }
                return(R);
            }
        }
        public static class Abuelo
        {
            private static Dictionary<IslandType,List<Position>>positions=new Dictionary<IslandType,List<Position>>{
            {IslandType.Ridgewood,new List<Position>{
                new Position(98,93),///NORTH
                new Position(-137,-41),///JUNIPER
                new Position(29,-158),///BANDIT CAMP
            }},
            {IslandType.Boia,new List<Position>{
                new Position(-62,308),///NORTHWELL
                new Position(184,247),///ODDYSEY
                new Position(101,74),///BAYOU
                new Position(293,-14),///SALADO
                new Position(618,31),///RUSTWATER
                new Position(309,-147),///CHERYTOL
                new Position(480,-369),///FRONTIER
                new Position(198,-413),///CALDWELL
            }},
            {IslandType.Trailhead,new List<Position>{
                new Position(-95,62),///NEW WILLIAMSBURG
            }}};
            public static Position Find(Player player,int maxDistance=150){
                Position R=null;
                var pos=player.getPosition();
                var islandType=player.getIsland();
                float max=maxDistance<1?1E6f:maxDistance;
                positions.TryGetValue(islandType,out var poss);
                if(poss==null)return(null);
                foreach(var position in poss){
                    var distance=pos.Distance(position);
                    if(distance<=max){
                        max=distance;
                        R=position;
                    }
                }
                return(R);
            }
        }
        public static class Loot
        {
            public static ItemDrop Find(Player player,params string[]names){
                var nodes=GameObject.FindObjectsOfType<ItemDrop>();
                var drops=new Dictionary<string,bool>();
                var inv=player.getInventory();
                var pos=player.getPosition();
                bool everything=false;
                var len=names.Length;
                bool fbreak=false;
                ItemDrop R=null;
                float max=1E6f;
                foreach(var name in names){
                    if(name=="*"){
                        len=1;
                        everything=true;
                    }
                }
                if(len==1){
                    var name="*";
                    bool endswith=false;
                    bool startswith=false;
                    if(!everything){
                        name=names[0].ToLower();
                        if(name=="caps")name="primer caps";
                        else{
                            endswith=name.StartsWith("*");
                            startswith=name.EndsWith("*");
                            if(endswith)name=name.Split(new char[]{'*'},2)[1].Trim();else
                            if(startswith)name=name.Split(new char[]{'*'},2)[0].Trim();
                        }
                    }
                    foreach(var node in nodes){
                        if(!node.IsSpawned||!node.isActiveAndEnabled)continue;
                        var n=node.name.ToLower();
                        if(everything||n==name||(startswith&&n.StartsWith(name))||(endswith&&n.EndsWith(name))){
                            if(!drops.ContainsKey(n))drops[n]=inv.canLoot(node);
                            if(!drops[n])continue;
                            var A=pos.Distance(new Position(node.TilePosition));
                            if(A<2){
                                R=node;
                                break;
                            }
                            if(A<max){
                                max=A;
                                R=node;
                            }
                        }
                    }
                }else{
                    for(int x=0;x<names.Length;x++)names[x]=names[x].ToLower();
                    foreach(var node in nodes){
                        if(!node.IsSpawned||!node.isActiveAndEnabled)continue;
                        var n=node.name.ToLower();
                        if(!drops.ContainsKey(n))drops[n]=inv.canLoot(node);
                        if(!drops[n])continue;
                        var poss=new Position(node.TilePosition);
                        var A=pos.Distance(poss);
                        for(int x=0;x<names.Length;x++){
                            var name=names[x];
                            bool endswith=false;
                            bool startswith=false;
                            if(name=="caps")name="primer caps";
                            else{
                                endswith=name.StartsWith("*");
                                startswith=name.EndsWith("*");
                                if(endswith)name=name.Split(new char[]{'*'},2)[1].Trim();else
                                if(startswith)name=name.Split(new char[]{'*'},2)[0].Trim();
                            }
                            if(n==name||(startswith&&n.StartsWith(name))||(endswith&&n.EndsWith(name))){
                                if(A<2){
                                    fbreak=true;
                                    R=node;
                                    break;
                                }
                                if(A<max){
                                    max=A;
                                    R=node;
                                }
                            }
                        }
                        if(fbreak)break;
                    }
                }
                return(R);
            }
            public static ItemDrop[]FindAll(Player player,params string[]names){
                var nodes=GameObject.FindObjectsOfType<ItemDrop>();
                var drops=new Dictionary<string,bool>();
                List<ItemDrop>R=new List<ItemDrop>();
                var inv=player.getInventory();
                var pos=player.getPosition();
                Position position=null;
                bool everything=false;
                var len=names.Length;
                float max=1E6f;
                foreach(var name in names){
                    if(name=="*"){
                        len=1;
                        everything=true;
                    }
                }
                if(len==1){
                    var name="*";
                    bool endswith=false;
                    bool startswith=false;
                    if(!everything){
                        name=names[0].ToLower();
                        if(name=="caps")name="primer caps";
                        else{
                            endswith=name.StartsWith("*");
                            startswith=name.EndsWith("*");
                            if(endswith)name=name.Split(new char[]{'*'},2)[1].Trim();else
                            if(startswith)name=name.Split(new char[]{'*'},2)[0].Trim();
                        }
                    }
                    foreach(var node in nodes){
                        if(!node.IsSpawned||!node.isActiveAndEnabled)continue;
                        var n=node.name.ToLower();
                        if(everything||n==name||(startswith&&n.StartsWith(name))||(endswith&&n.EndsWith(name))){
                            if(!drops.ContainsKey(n))drops[n]=inv.canLoot(node);
                            if(!drops[n])continue;
                            var poss=new Position(node.TilePosition);
                            if(position!=null){
                                if(!poss.Equals(position))continue;
                                R.Add(node);
                            }else{
                                var A=pos.Distance(poss);
                                if(A<2){
                                    position=poss;
                                    R.Add(node);
                                    continue;
                                }
                                if(A<max){
                                    max=A;
                                    R.Add(node);
                                    position=poss;
                                }
                            }
                        }
                    }
                }else{
                    for(int x=0;x<names.Length;x++)names[x]=names[x].ToLower();
                    foreach(var node in nodes){
                        if(!node.IsSpawned||!node.isActiveAndEnabled)continue;
                        var n=node.name.ToLower();
                        if(!drops.ContainsKey(n))drops[n]=inv.canLoot(node);
                        if(!drops[n])continue;
                        var poss=new Position(node.TilePosition);
                        if(position!=null&&!poss.Equals(position))continue;
                        var A=pos.Distance(poss);
                        for(int x=0;x<names.Length;x++){
                            var name=names[x];
                            bool endswith=false;
                            bool startswith=false;
                            if(name=="caps")name="primer caps";
                            else{
                                endswith=name.StartsWith("*");
                                startswith=name.EndsWith("*");
                                if(endswith)name=name.Split(new char[]{'*'},2)[1].Trim();else
                                if(startswith)name=name.Split(new char[]{'*'},2)[0].Trim();
                            }
                            if(n==name||(startswith&&n.StartsWith(name))||(endswith&&n.EndsWith(name))){
                                if(position!=null)R.Add(node);
                                else{
                                    if(A<2){
                                        R.Add(node);
                                        continue;
                                    }
                                    if(A<max){
                                        max=A;
                                        R.Add(node);
                                    }
                                }
                            }
                        }
                    }
                }
                return(R.Count==0?null:R.ToArray());
            }
        }
    }
}