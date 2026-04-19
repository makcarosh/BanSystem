using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ExampleUnturnedPlugin.Commands
{
    public class TestCommand : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "superban";
        public string Help => "Ban player";
        public string Syntax => "/superban [nick/steamID] [time: 30s, 10m, 5h, 2d, 1w] [reason]";
        public List<string> Aliases => new List<string> { "sb", "sban" };
        public List<string> Permissions => new List<string> { "sban" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer admin = (UnturnedPlayer)caller;

            if (command.Length < 2)
            {
                UnturnedChat.Say(admin, $"Использование: {Syntax}", Color.red);
                return;
            }

            string targetArg = command[0];
            string timeArg = command[1];
            string reason = command.Length >= 3
                ? string.Join(" ", command.Skip(2))
                : "Причина не указана";

            ulong duration = ParseDurationToSeconds(timeArg);

            if (duration == 0)
            {
                UnturnedChat.Say(admin, "Неверный формат времени. Примеры: 30s, 10m, 5h, 2d, 1w", Color.red);
                return;
            }

            UnturnedPlayer targetPlayer = null;
            ulong targetSteamId = 0;
            string ip = string.Empty;
            string steamName = string.Empty;
            byte[] hwid = null;

            if (ulong.TryParse(targetArg, out ulong parsedSteamId))
            {
                targetSteamId = parsedSteamId;
                targetPlayer = UnturnedPlayer.FromCSteamID(new CSteamID(targetSteamId));
            }
            else
            {
                targetPlayer = UnturnedPlayer.FromName(targetArg);

                if (targetPlayer != null)
                    targetSteamId = targetPlayer.CSteamID.m_SteamID;
            }

            if (targetSteamId == 0)
            {
                UnturnedChat.Say(admin, "Игрок не найден.", Color.red);
                return;
            }

            if (targetPlayer != null)
            {
                ip = targetPlayer.IP;
                steamName = targetPlayer.SteamName;
                hwid = targetPlayer.Player.channel.owner.playerID.hwid;
            }

            DatabaseManager db = Plugin.Instance.DatabaseManager;

            PlayerData playerData = db.FindPlayer(targetSteamId, ip, hwid);

            if (playerData == null)
            {
                db.AddOrUpdatePlayer(targetSteamId, ip, steamName, hwid);
                playerData = db.FindPlayer(targetSteamId, ip, hwid);
            }
            else
            {
                db.AddOrUpdatePlayer(targetSteamId, ip, steamName, hwid);
                playerData = db.FindPlayer(targetSteamId, ip, hwid);
            }

            if (playerData == null)
            {
                UnturnedChat.Say(admin, "Не удалось создать или найти запись игрока в базе.", Color.red);
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;
            playerData.BanIssuedAtUtc = nowUtc;
            playerData.BanExpiresAtUtc = nowUtc.AddSeconds(duration);
            playerData.BanReason = reason;

            db.Save();

            string formattedDuration = db.FormatBanTime(duration);
            UnturnedChat.Say(admin, $"Игрок забанен на {formattedDuration}. Причина: {reason}", Color.green);

            string targetNameForWebhook = targetPlayer != null
                ? targetPlayer.SteamName
                : targetSteamId.ToString();

            Plugin.WriteDiscordWebHook(
                Plugin.Instance.Configuration.Instance.webhookUnBan,
                "banplayer",
                targetNameForWebhook,
                $"https://steamcommunity.com/profiles/{targetSteamId}",
                formattedDuration,
                admin,
                admin.CSteamID.m_SteamID,
                Plugin.Instance.Configuration.Instance.serverName
            );

            if (targetPlayer != null)
            {
                targetPlayer.Kick($"Вы забанены на {formattedDuration}! Причина: {reason}");
            }
        }

        private ulong ParseDurationToSeconds(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return 0;

            input = input.Trim().ToLower();

            ulong multiplier = 0;
            string numberPart = "";

            if (input.EndsWith("sec"))
            {
                multiplier = 1;
                numberPart = input.Substring(0, input.Length - 3);
            }
            else if (input.EndsWith("min"))
            {
                multiplier = 60;
                numberPart = input.Substring(0, input.Length - 3);
            }
            else if (input.EndsWith("s"))
            {
                multiplier = 1;
                numberPart = input.Substring(0, input.Length - 1);
            }
            else if (input.EndsWith("m"))
            {
                multiplier = 60;
                numberPart = input.Substring(0, input.Length - 1);
            }
            else if (input.EndsWith("h"))
            {
                multiplier = 60 * 60;
                numberPart = input.Substring(0, input.Length - 1);
            }
            else if (input.EndsWith("d"))
            {
                multiplier = 60 * 60 * 24;
                numberPart = input.Substring(0, input.Length - 1);
            }
            else if (input.EndsWith("w"))
            {
                multiplier = 60 * 60 * 24 * 7;
                numberPart = input.Substring(0, input.Length - 1);
            }
            else
            {
                return 0;
            }

            if (!ulong.TryParse(numberPart, out ulong value))
                return 0;

            if (value == 0)
                return 0;

            return value * multiplier;
        }
    }
}