using Rocket.API;
using System;
namespace ExampleUnturnedPlugin
{
    public class Configuration : IRocketPluginConfiguration
    {
        public string webhookBan;
        public string webhookUnBan;
        public string webhookBanPlayerConnected;
        public string serverName;

        public void LoadDefaults()
        {
            webhookBan = "https://discord.com/api/webhooks/...";
            webhookUnBan = "https://discord.com/api/webhooks/...";
            webhookBanPlayerConnected = "https://discord.com/api/webhooks/...";
            serverName = "Server Name";
        }
    }
}