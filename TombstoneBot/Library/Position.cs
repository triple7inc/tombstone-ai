using UnityEngine;
using UnityEngine.Tilemaps;
using Il2CppUI.RightClickMenu;
using System.Collections.Generic;
using Il2CppEntities.Players.Cameras;
namespace TombstoneAI.Library
{
    public class Position
    {
        public int x;
        public int y;
        private TileBase tile=null;
        private Tilemapper tilemapper;
        public Position(int x,int y){
            this.x=x;
            this.y=y;
        }
        public Position(Vector2 vector){
            this.x=(int)vector.x;
            this.y=(int)vector.y;
        }
        public Position(Vector3 vector){
            this.x=(int)vector.x;
            this.y=(int)vector.y;
        }
        public Position(Vector4 vector){
            this.x=(int)vector.x;
            this.y=(int)vector.y;
        }
        public Position(Vector2Int vector){
            this.x=vector.x;
            this.y=vector.y;
        }
        public Position(Vector3Int vector){
            this.x=vector.x;
            this.y=vector.y;
        }
        public TileBase GetTile(){
            if(tile==null){
                tilemapper=GameObject.FindObjectOfType<Tilemapper>();
                if(tilemapper==null)return(null);
                tile=tilemapper.GetCurrentTile(new Vector3Int(x,y));
            }
            return(tile);
        }
        public List<RightClickOption>GetRightClickOptions(Player player){
            ///TODO: Fix this
            var tile=GetTile();
            TileData data=default;
            if(tile==null)return(null);
            tile.GetTileData(new Vector3Int(x,y),tilemapper._tilemap,ref data);
            var A=data.gameObject.GetComponents<RightClickOption>();
            var logger=new Logger(player);
            foreach(var R in A){
                logger.Msg("Option: "+R.Text);
            }
            return(null);
        }
        public int Distance(Player to){
            var pos=to.getPosition();
            return(Mathf.Max(Mathf.Abs(x-pos.x),Mathf.Abs(y-pos.y)));
        }
        public int Distance(Position to){
            return(Mathf.Max(Mathf.Abs(x-to.x),Mathf.Abs(y-to.y)));
        }
        public int Distance(int x,int y){
            return(Mathf.Max(Mathf.Abs(this.x-x),Mathf.Abs(this.y-y)));
        }
        public int Distance(float x,float y){
            return(this.Distance((int)x,(int)y));
        }
        public int Distance(Vector2 to){
            return(this.Distance(to.x,to.y));
        }
        public int Distance(Vector3 to){
            return(this.Distance(to.x,to.y));
        }
        public int Distance(Vector4 to){
            return(this.Distance(to.x,to.y));
        }
        public int Distance(Vector2Int to){
            return(this.Distance(to.x,to.y));
        }
        public int Distance(Vector3Int to){
            return(this.Distance(to.x,to.y));
        }
        public override string ToString(){
            return(this.x.ToString()+", "+this.y.ToString());
        }
        public override bool Equals(object obj){
            if(obj is Position p)return(x==p.x&&y==p.y);
            return(false);
        }
        public override int GetHashCode(){
            return(x*397^y);
        }
        public Position Copy()=>new Position(x,y);
        public Vector2 ToVector2()=>new Vector2(x,y);
        public Vector2Int ToVector2Int()=>new Vector2Int(x,y);
    }
}