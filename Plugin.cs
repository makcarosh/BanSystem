using Newtonsoft.Json;
using Rocket.API.Collections;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Net.Http;
using System.Text;

namespace ExampleUnturnedPlugin
{
    public class Plugin : RocketPlugin<Configuration>
    {
        public static Plugin Instance;
        public DatabaseManager DatabaseManager { get; set; }

        protected override void Load()
        {
            Instance = this;

            Logger.Log("BanSystem is load\nDeveloper discord:makcarosh");

            DatabaseManager = new DatabaseManager();
            DatabaseManager.Load();

            U.Events.OnBeforePlayerConnected += Events_OnBeforePlayerConnected;
        }

        private void Events_OnBeforePlayerConnected(UnturnedPlayer player)
        {
            byte[] hwid = player.Player.channel.owner.playerID.hwid;
            string ip = player.IP;
            string steamNick = player.SteamName;
            ulong steamID = player.CSteamID.m_SteamID;
            string steamPlayerLink = $"https://steamcommunity.com/profiles/{steamID}";
            DatabaseManager.AddOrUpdatePlayer(steamID, ip, steamNick, hwid);

            PlayerData dbPlayer = DatabaseManager.FindPlayer(steamID, ip, hwid);
            ulong duration = DatabaseManager.GetRemainingBanSeconds(dbPlayer);
            
            if (duration > 0)
            {
                DatabaseManager.Save();

                string formattedTime = DatabaseManager.FormatBanTime(duration);
                string reason = string.IsNullOrWhiteSpace(dbPlayer.BanReason) ? "Причина не указана" : dbPlayer.BanReason;

                string banMessage = $"Вы забанены на {formattedTime}! Причина: {reason}";

                player.Kick(banMessage);
                WriteDiscordWebHook(
                    Configuration.Instance.webhookBanPlayerConnected,
                    "banplayer_connected",
                    steamNick,
                    steamPlayerLink,
                    Configuration.Instance.serverName
                );
                
                return;
            }

            DatabaseManager.Save();
        }

        public static async void WriteDiscordWebHook(string webhook, string message, params object[] forMessage)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(webhook))
                {
                    Rocket.Core.Logging.Logger.LogWarning("Webhook не указан в конфиге.");
                    return;
                }

                using (var webhookClient = new HttpClient())
                {
                    var payload = new
                    {
                        content = Translation.DefaultTranslations.Translate(message, forMessage)
                    };

                    string json = JsonConvert.SerializeObject(payload);
                    using (var content = new StringContent(json, Encoding.UTF8, "application/json"))
                    {
                        HttpResponseMessage response = await webhookClient.PostAsync(webhook, content);

                        if (response.IsSuccessStatusCode)
                        {
                            Rocket.Core.Logging.Logger.Log("Сообщение отправлено в WebHook.");
                        }
                        else
                        {
                            string responseText = await response.Content.ReadAsStringAsync();
                            Rocket.Core.Logging.Logger.LogWarning($"Ошибка отправки WebHook: {response.StatusCode} | {responseText}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Rocket.Core.Logging.Logger.LogError($"Ошибка отправки в Discord: {ex}");
            }
        }

        public override TranslationList DefaultTranslations => Translation.DefaultTranslations;

        protected override void Unload()
        {
            U.Events.OnBeforePlayerConnected -= Events_OnBeforePlayerConnected;

            DatabaseManager?.Save();
            DatabaseManager = null;

            Logger.Log("BanSystem is unload\nDeveloper discord:makcarosh");

            Instance = null;
        }
    }
}