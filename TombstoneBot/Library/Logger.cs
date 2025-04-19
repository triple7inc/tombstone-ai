namespace TombstoneAI.Library
{
    public class Logger
    {
        Player player;
        public Logger(Player player){
            this.player=player;
        }
        public void Msg(string msg){
            this.player.basePlayer().Messages.ChatLog(msg,Il2CppWitchslayer.Chat.ChatChannel.System);
        }
    }
}