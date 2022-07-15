using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Chat;
using Terraria.ID;
using Terraria.Localization;

namespace TerrariaArchipelago.Common
{
    public class TextUtils
    {
        public static void SendText(string text, Color color = default(Color))
        {
            SendText(text, color.R, color.G, color.B);
        }

        public static void SendText(string text, byte r, byte g, byte b)
        {
            /*
             * Multiplayer compatibility: NewText should not be run in the server
             * So instead, we check if it's single player. If it is, we use NewText;
             * Otherwise, we use BroadcastChatMessage
             */
            if (Main.netMode == NetmodeID.SinglePlayer) Main.NewText(text, r, g, b);
            else if (Main.netMode == NetmodeID.Server) ChatHelper.BroadcastChatMessage(NetworkText.FromLiteral(text), new Color(r, g, b));
        }
    }
}
