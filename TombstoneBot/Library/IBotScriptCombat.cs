using Il2CppTileObjects.ItemDrops;
using System.Collections.Generic;
using Il2CppEntities.Enemies;
namespace TombstoneAI.Library
{
    public interface IBotScriptCombat:IBotScript
    {
        void removeEnemies(params string[]enemies);
        void addEnemies(params string[]enemies);
        void setEnemies(params string[]enemies);
        void setEnemies(List<string>enemies);
        void removeEnemy(string enemy);
        void addEnemy(string enemy);
        string[]getEnemiesAsArray();
        List<string>getEnemies();
        void removeLoot(params string[]items);
        void addLoot(params string[]items);
        void setLoot(params string[]items);
        void setLoot(List<string>items);
        void removeLoot(string item);
        void addLoot(string item);
        string[]getLootAsArray();
        List<string>getLoot();
        void clearLoot();
    }
    public abstract class CombatBotScriptBase:BotScriptBase,IBotScriptCombat
    {
        protected Enemy enemyTarget;
        protected ItemDrop lootTarget;
        protected ItemDrop[]lootTargets;
        private List<string>enemies=new List<string>();
        private List<string>lootables=new List<string>();
        public void clearLoot(){this.lootables.Clear();}
        
        public List<string>getEnemies(){return(this.enemies);}
        public string[]getEnemiesAsArray(){return(this.enemies.ToArray());}
        public void addEnemy(string enemy){this.enemies.Add(enemy.ToLower());}
        public void removeEnemy(string enemy){this.enemies.Remove(enemy.ToLower());}
        public void setEnemies(List<string>enemies){
            this.enemies=enemies;
            for(var e=0;e<this.enemies.Count;e++){
                this.enemies[e]=this.enemies[e].ToLower();
            }
        }
        public void removeEnemies(params string[]enemies){
            for(int x=0;x<enemies.Length;x++){
                enemies[x]=enemies[x].ToLower();
                if(!this.enemies.Contains(enemies[x]))continue;
                this.enemies.Remove(enemies[x]);
            }
        }
        public void addEnemies(params string[]enemies){
            for(int x=0;x<enemies.Length;x++){
                enemies[x]=enemies[x].ToLower();
                if(this.enemies.Contains(enemies[x]))continue;
                this.enemies.Add(enemies[x]);
            }
        }
        public void setEnemies(params string[]enemies){
            this.enemies.Clear();
            for(int x=0;x<enemies.Length;x++){
                enemies[x]=enemies[x].ToLower();
                this.enemies.Add(enemies[x]);
            }
        }
        public List<string>getLoot(){return(this.lootables);}
        public string[]getLootAsArray(){return(this.lootables.ToArray());}
        public void addLoot(string item){this.lootables.Add(item.ToLower());}
        public void removeLoot(string item){this.lootables.Remove(item.ToLower());}
        public void setLoot(List<string>items){
            this.lootables=lootables;
            for(var e=0;e<this.lootables.Count;e++){
                this.lootables[e]=this.lootables[e].ToLower();
            }
        }
        public void removeLoot(params string[]lootables){
            for(int x=0;x<lootables.Length;x++){
                lootables[x]=lootables[x].ToLower();
                if(!this.lootables.Contains(lootables[x]))continue;
                this.lootables.Remove(lootables[x]);
            }
        }
        public void addLoot(params string[]lootables){
            for(int x=0;x<lootables.Length;x++){
                lootables[x]=lootables[x].ToLower();
                if(this.lootables.Contains(lootables[x]))continue;
                this.lootables.Add(lootables[x]);
            }
        }
        public void setLoot(params string[]lootables){
            this.lootables.Clear();
            for(int x=0;x<lootables.Length;x++){
                lootables[x]=lootables[x].ToLower();
                this.lootables.Add(lootables[x]);
            }
        }
    }
}