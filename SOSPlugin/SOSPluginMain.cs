using System;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using OTAPI;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Terraria.Localization;
using Terraria.ID;
using System.IO;

namespace SOSPlugin
{
    [ApiVersion(2, 1)]
    public class SOSPluginMain : TerrariaPlugin
    {
        #region Identity

        public override string Author => "Joenastan ProD";
        public override string Description => "Utility Plugin to save your life by JT";
        public override string Name => "SOSPlugin";
        public override Version Version => new Version(1, 0, 0, 1);

        #endregion
        
        private LastPlaceOfDeath lastPlaceOfDeathData = null; // in Server Data holder

        public SOSPluginMain(Main game) : base(game)
        {
            Order += 10;
        }

        public override void Initialize()
        {
            // Commands
            Commands.ChatCommands.Add(new Command("tshock.whisper", SaveMoney, "savemoney"));
            Commands.ChatCommands.Add(new Command("tshock.whisper", InfoLastDeath, "lastdeadinfo"));
            Commands.ChatCommands.Add(new Command("tshock.whisper", TeleportLastDeath, "lastdeadport"));

            // Events
            GetDataHandlers.ItemDrop += OnDroppedEvent;
            ServerApi.Hooks.NetGreetPlayer.Register(this, OnPlayerWorldConnect);
            ServerApi.Hooks.NetGetData.Register(this, OnServerDataEvent);
        }

        #region Command Functions
 
        private void SaveMoney(CommandArgs arg)
        {
            TSPlayer player = arg.Player;
            lastPlaceOfDeathData.UpdateSaveMoneyUser(player.UUID);
            if (lastPlaceOfDeathData.GetUserSavedMoney(player.UUID))
                player.SendMessage("Save Money Mode has been Enabled", Color.GreenYellow);
            else
                player.SendMessage("Save Money Mode has been Disabled", Color.GreenYellow);
        }

        private void InfoLastDeath(CommandArgs arg)
        {
            TSPlayer player = arg.Player;
            SOSKey sos = lastPlaceOfDeathData.GetUserDeathInfo(player.UUID);

            if (sos.deathCount > 0)
            {
                string text = String.Format("Location (X, Y): ({0}, {1})\nDescription: {2}\nDeath Count: {3}", 
                                            sos.lastDeathPlaceX, sos.lastDeathPlaceY, sos.lastDeathMessage,
                                            sos.deathCount);
                player.SendMessage(text, Color.YellowGreen);
            }
            else
                player.SendMessage("You haven't died yet in the server.", Color.YellowGreen);
        }

        private void TeleportLastDeath(CommandArgs arg)
        {
            TSPlayer player = arg.Player;
            SOSKey sos = lastPlaceOfDeathData.GetUserDeathInfo(player.UUID);

            if (sos.deathCount > 0)
            {
                player.Teleport(sos.lastDeathPlaceX, sos.lastDeathPlaceY);
                player.SendMessage("You have teleported to your last corpse.", Color.YellowGreen);
            }
            else
                player.SendMessage("You haven't died yet in the server.", Color.YellowGreen);
        }

        #endregion

        #region Events

        private void DeathEvent(TSPlayer player, MemoryStream data)
        {
            // Read in Byte
            data.ReadByte();
            PlayerDeathReason death = PlayerDeathReason.FromReader(new BinaryReader(data));
            string reason = death.GetDeathText(player.TPlayer.name).ToString();

            // Update player last death event
            lastPlaceOfDeathData.UpdateLocationUser(player.UUID, new Vector2(player.X, player.Y), reason);
        }

        private void OnPlayerWorldConnect(GreetPlayerEventArgs arg)
        {
            // Load settings
            if (lastPlaceOfDeathData == null)
                lastPlaceOfDeathData = new LastPlaceOfDeath("./tshock/sosplugin.json");

            // Check world match, if not match change world ID in instance
            if (lastPlaceOfDeathData.WorldID != Main.worldID)
                lastPlaceOfDeathData.WorldID = Main.worldID;
        }

        private void OnDroppedEvent(object sender, GetDataHandlers.ItemDropEventArgs arg)
        {
            TSPlayer player = arg.Player;
            // Check if user has info and he/she in died state
            if (lastPlaceOfDeathData.GetUserSavedMoney(player.UUID) && player.TPlayer.DeadOrGhost)
            {
                // Check if the Coin Dropped after death
                if (arg.Type <= ItemID.PlatinumCoin && arg.Type >= ItemID.CopperCoin)
                {
                    Item coin = ItemManager.GetTerrariaItemByID(arg.Type);
                    coin.stack = arg.Stacks;
                    player.SendMessage("You have dropped " + coin.stack + " " + coin.Name, Color.OrangeRed);
                }
            }
        }

        private void OnServerDataEvent(GetDataEventArgs arg)
        {
            if (arg.Handled) return;

            TSPlayer player = TShock.Players[arg.Msg.whoAmI];
            switch (arg.MsgID)
            {
                // Death Event
                case PacketTypes.PlayerDeathV2:
                    DeathEvent(player, new MemoryStream(arg.Msg.readBuffer, arg.Index, arg.Length - 1));
                    break;

                default:
                    break;
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lastPlaceOfDeathData.SaveProgress();
                ServerApi.Hooks.NetGetData.Deregister(this, OnServerDataEvent);
                ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnPlayerWorldConnect);
                GetDataHandlers.ItemDrop -= OnDroppedEvent;
            }
            base.Dispose(disposing);
        }
    }
}