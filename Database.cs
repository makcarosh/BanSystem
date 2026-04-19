using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExampleUnturnedPlugin
{
    public class Database
    {
        public List<PlayerData> Players { get; set; } = new List<PlayerData>();
    }

    public class PlayerData
    {
        public List<ulong> SteamIds { get; set; } = new List<ulong>();
        public List<string> PlayerIps { get; set; } = new List<string>();
        public List<string> SteamNames { get; set; } = new List<string>();
        public List<byte[]> PlayerHwids { get; set; } = new List<byte[]>();
        public string BanReason { get; set; }

        public DateTime? BanIssuedAtUtc { get; set; }
        public DateTime? BanExpiresAtUtc { get; set; }
    }
    
    public class DatabaseManager
    {
        private readonly string _path;
        public Database Data { get; set; }

        public DatabaseManager()
        {
            _path = Path.Combine(Plugin.Instance.Directory, "database.json");
        }

        public void Load()
        {
            if (!File.Exists(_path))
            {
                Data = new Database();
                Save();
                return;
            }

            string json = File.ReadAllText(_path);

            if (string.IsNullOrWhiteSpace(json))
            {
                Data = new Database();
                Save();
                return;
            }

            Data = JsonConvert.DeserializeObject<Database>(json);

            if (Data == null)
                Data = new Database();
        }

        public void Save()
        {
            string json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            File.WriteAllText(_path, json);
        }

        public void AddOrUpdatePlayer(ulong steamId, string ip, string steamName, byte[] hwid)
        {
            if (Data == null)
                Data = new Database();

            PlayerData existingPlayer = Data.Players.FirstOrDefault(p =>
                (steamId != 0 && p.SteamIds.Contains(steamId)) ||
                (!string.IsNullOrWhiteSpace(ip) && p.PlayerIps.Contains(ip)) ||
                HasHwid(p, hwid)
            );

            if (existingPlayer == null)
            {
                existingPlayer = new PlayerData();
                Data.Players.Add(existingPlayer);
            }

            if (steamId != 0 && !existingPlayer.SteamIds.Contains(steamId))
                existingPlayer.SteamIds.Add(steamId);

            if (!string.IsNullOrWhiteSpace(ip) && !existingPlayer.PlayerIps.Contains(ip))
                existingPlayer.PlayerIps.Add(ip);

            if (!string.IsNullOrWhiteSpace(steamName) && !existingPlayer.SteamNames.Contains(steamName))
                existingPlayer.SteamNames.Add(steamName);

            AddHwidIfMissing(existingPlayer, hwid);
        }

        public PlayerData FindPlayer(ulong steamId, string ip, byte[] hwid)
        {
            if (Data == null)
                return null;

            return Data.Players.FirstOrDefault(p =>
                (steamId != 0 && p.SteamIds.Contains(steamId)) ||
                (!string.IsNullOrWhiteSpace(ip) && p.PlayerIps.Contains(ip)) ||
                HasHwid(p, hwid)
            );
        }

        private bool HasHwid(PlayerData player, byte[] hwid)
        {
            if (player == null || player.PlayerHwids == null || hwid == null || hwid.Length == 0)
                return false;

            return player.PlayerHwids.Any(savedHwid =>
                savedHwid != null && savedHwid.SequenceEqual(hwid));
        }

        private void AddHwidIfMissing(PlayerData player, byte[] hwid)
        {
            if (player == null || hwid == null || hwid.Length == 0)
                return;

            if (!HasHwid(player, hwid))
                player.PlayerHwids.Add((byte[])hwid.Clone());
        }

        public ulong GetRemainingBanSeconds(PlayerData player)
        {
            if (player == null)
                return 0;

            if (!player.BanIssuedAtUtc.HasValue || !player.BanExpiresAtUtc.HasValue)
                return 0;

            DateTime nowUtc = DateTime.UtcNow;

            if (player.BanExpiresAtUtc.Value <= nowUtc)
                return 0;

            ulong duration = (ulong)(player.BanExpiresAtUtc.Value - nowUtc).TotalSeconds;
            return duration;
        }

        public string FormatBanTime(ulong duration)
        {
            if (duration == 0)
                return "0 секунд";

            ulong weeks = duration / (7UL * 24UL * 60UL * 60UL);
            duration %= (7UL * 24UL * 60UL * 60UL);

            ulong days = duration / (24UL * 60UL * 60UL);
            duration %= (24UL * 60UL * 60UL);

            ulong hours = duration / (60UL * 60UL);
            duration %= (60UL * 60UL);

            ulong minutes = duration / 60UL;
            ulong seconds = duration % 60UL;

            List<string> parts = new List<string>();

            if (weeks > 0)
                parts.Add($"{weeks} {GetWordForm(weeks, "неделя", "недели", "недель")}");

            if (days > 0)
                parts.Add($"{days} {GetWordForm(days, "день", "дня", "дней")}");

            if (hours > 0)
                parts.Add($"{hours} {GetWordForm(hours, "час", "часа", "часов")}");

            if (minutes > 0)
                parts.Add($"{minutes} {GetWordForm(minutes, "минута", "минуты", "минут")}");

            if (seconds > 0)
                parts.Add($"{seconds} {GetWordForm(seconds, "секунда", "секунды", "секунд")}");

            return string.Join(" ", parts);
        }

        private string GetWordForm(ulong value, string form1, string form2, string form5)
        {
            ulong v = value % 100;
            ulong n = v % 10;

            if (v > 10 && v < 20)
                return form5;

            if (n > 1 && n < 5)
                return form2;

            if (n == 1)
                return form1;

            return form5;
        }
    }
}