using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExampleUnturnedPlugin.Commands
{
    public class SunbanCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "sunban";
        public string Help => "Unban player";
        public string Syntax => "/sunban [steamid] [reason]";
        public List<string> Aliases => new List<string> { "sub", "unbanplayer" };
        public List<string> Permissions => new List<string> { "sunban" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer admin = (UnturnedPlayer)caller;

            if (command.Length < 1)
            {
                UnturnedChat.Say(admin, $"Использование: {Syntax}", Color.red);
                return;
            }

            if (!ulong.TryParse(command[0], out ulong targetSteamId) || targetSteamId == 0)
            {
                UnturnedChat.Say(admin, "Укажи корректный SteamID.", Color.red);
                return;
            }

            string reason = command.Length >= 2
                ? string.Join(" ", command, 1, command.Length - 1)
                : "Разбан без причины";

            DatabaseManager db = Plugin.Instance.DatabaseManager;

            PlayerData playerData = db.FindPlayer(targetSteamId, string.Empty, null);

            if (playerData == null)
            {
                UnturnedChat.Say(admin, "Игрок не найден в базе.", Color.red);
                return;
            }

            playerData.BanIssuedAtUtc = null;
            playerData.BanExpiresAtUtc = null;
            playerData.BanReason = reason;

            db.Save();

            UnturnedChat.Say(admin, $"Игрок {targetSteamId} разбанен. Причина: {reason}", Color.green);

            Plugin.WriteDiscordWebHook(
                Plugin.Instance.Configuration.Instance.webhookUnBan,
                "unbanplayer",
                "Игрок",
                $"https://steamcommunity.com/profiles/{targetSteamId}",
                admin,
                admin.CSteamID.m_SteamID,
                Plugin.Instance.Configuration.Instance.serverName
            );
        }
    }
}