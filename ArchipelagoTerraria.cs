using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using TerrariaArchipelago.Common;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System;
using Archipelago.Items;

namespace Archipelago
{
    public class ArchipelagoTerraria : Mod
    {
        public static ArchipelagoTerraria instance;
        public static ArchipelagoSession session;

        public override void Load()
        {
            instance = this;
            On.Terraria.WorldGen.Hooks.WorldLoaded += OnWorldLoaded;
            On.Terraria.WorldGen.SaveAndQuit += OnSaveAndQuit;
        }

        public override void Unload()
        {
            instance = null;
            On.Terraria.WorldGen.Hooks.WorldLoaded -= OnWorldLoaded;
            On.Terraria.WorldGen.SaveAndQuit -= OnSaveAndQuit;
        }

        public override void PostAddRecipes()
        {
            ItemManager.PostAddRecipes();
        }

        public static void OnWorldLoaded(On.Terraria.WorldGen.Hooks.orig_WorldLoaded orig)
        {
            orig();
            Achievements.AchievementManager.Load();
            ItemManager.Load();
        }

        // Disconnect from the server when the player saves and quits
        public static void OnSaveAndQuit(On.Terraria.WorldGen.orig_SaveAndQuit orig, Action callback)
        {
            orig(callback);
            Achievements.AchievementManager.Unload();
            ItemManager.Unload();
            Commands.DisconnectCommand.Disconnect();
        }

        // Checks for player sent messages
        public static void OnTerrariaChatMessage(On.Terraria.Chat.ChatCommandProcessor.orig_ProcessIncomingMessage orig,
            Terraria.Chat.ChatCommandProcessor self, Terraria.Chat.ChatMessage message, int clientid)
        {
            orig(self, message, clientid);
            session.Socket.SendPacket(new SayPacket() { Text = message.Text });
        }

        // Main.NewText to avoid sending the message to the Archipelago server
        public static void OnPacketReceived(ArchipelagoPacketBase packet)
        {
            ArchipelagoPacketType type = packet.PacketType;
            if (type == ArchipelagoPacketType.Print)
            {
                PrintPacket received = (PrintPacket)packet;
                if(!received.Text.Contains("Space:"))
                    TextUtils.SendText(received.Text);
                return;
            }
            if (type == ArchipelagoPacketType.RoomUpdate)
            {
                RoomUpdatePacket received = (RoomUpdatePacket)packet;
                return;
            }
            if (type == ArchipelagoPacketType.PrintJSON)
            {
                PrintJsonPacket receivedPacket = (PrintJsonPacket)packet;
                string text = "";
                Color color = new Color(255, 255, 255);
                foreach(JsonMessagePart part in receivedPacket.Data)
                {
                    // Build the message string
                    string add = "";
                    if (part.Type == JsonMessagePartType.PlayerId)
                    {
                        // Get player name from server
                        add = session.Players.GetPlayerAlias(Int32.Parse(part.Text));
                    } else if (part.Type == JsonMessagePartType.ItemId)
                    {
                        // Get item name from server
                        add = session.Items.GetItemName(Int64.Parse(part.Text));
                    } else if(part.Type == JsonMessagePartType.LocationId)
                    {
                        // Get location name from server
                        add = session.Locations.GetLocationNameFromId(Int64.Parse(part.Text));
                    } else if(part.Type == JsonMessagePartType.Color)
                    {
                        // Change the color of the full text
                        switch (part.Color)
                        {
                            case JsonMessagePartColor.Black:
                                color = new Color(127, 127, 127); // Using gray instead of pure black to make it easier to read
                                break;
                            case JsonMessagePartColor.Red:
                                color = Color.Red;
                                break;
                            case JsonMessagePartColor.Green:
                                color = Color.Green;
                                break;
                            case JsonMessagePartColor.Yellow:
                                color = Color.Yellow;
                                break;
                            case JsonMessagePartColor.Blue:
                                color = Color.Blue;
                                break;
                            case JsonMessagePartColor.Magenta:
                                color = Color.Magenta;
                                break;
                            case JsonMessagePartColor.Cyan:
                                color = Color.Cyan;
                                break;
                            default:
                                /*
                                 * Font styles (underline, bold), and background colors (<color>_bg) aren't supported by Terraria
                                 * So we just default to sending the text in white.
                                 * The actual white color also falls through this, since it's the same operation
                                 */
                                color = Color.White;
                                break;
                        }
                    }
                }
                // Send the text
                TextUtils.SendText(text, color);
                return;
            }
            if(type == ArchipelagoPacketType.ReceivedItems)
            {
                ReceivedItemsPacket received = (ReceivedItemsPacket)packet;
                return;
            }
            // This packet comes first when connecting, do not put any terraria main thread actions here such as NewText
            // The Connected packet will stop the main thread from sleeping, so if you put any actions here that can only be done
            // On the main thread, then the game will be slept until the login result times out
            if(type == ArchipelagoPacketType.RoomInfo)
            {
                RoomInfoPacket received = (RoomInfoPacket)packet;
                return;
            }
            if (type == ArchipelagoPacketType.Connected)
            {
                ConnectedPacket received = (ConnectedPacket)packet;
                Achievements.AchievementManager.CompleteLocationChecks();
                return;
            }
            TextUtils.SendText("Received Unchecked packet type: " + type, Color.White);
        }
    }
}