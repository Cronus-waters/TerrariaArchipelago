using Terraria.ModLoader;
using TerrariaArchipelago.Common;

namespace Archipelago.Commands
{
	public class DisconnectCommand : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "disconnect";

		public override string Description
			=> "/disconnect";

		public override void Action(CommandCaller caller, string input, string[] args)
		{
			Disconnect();
		}

		public static void Disconnect()
        {
			if (ArchipelagoTerraria.session == null || !ArchipelagoTerraria.session.Socket.Connected)
			{
				TextUtils.SendText("Not Currently Connected to Archipelago Server.");
				return;
			}
			On.Terraria.Chat.ChatCommandProcessor.ProcessIncomingMessage -= ArchipelagoTerraria.OnTerrariaChatMessage;
			ArchipelagoTerraria.session.Socket.PacketReceived -= ArchipelagoTerraria.OnPacketReceived;
			ArchipelagoTerraria.session.Items.ItemReceived -= Items.ItemManager.OnItemReceived;
			ArchipelagoTerraria.session.Socket.Disconnect();
			ArchipelagoTerraria.session = null;
			TextUtils.SendText("Disconnected from the Archipelago server.");
		}
	}
}