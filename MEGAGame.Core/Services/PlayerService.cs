        using System.IO;
        using System.Text.Json;
        using MEGAGame.Core.Models;

        namespace MEGAGame.Core.Services;

        public static class PlayerService
        {
            private const string PlayersFile = "players.json";
    
            public static void SavePlayer(Player player)
            {
                var players = LoadAllPlayers();
                players.Add(player);
        
                File.WriteAllText(PlayersFile, 
                    JsonSerializer.Serialize(players));
            }
    
            public static List<Player> LoadAllPlayers()
            {
                if (!File.Exists(PlayersFile)) return new List<Player>();
        
                var json = File.ReadAllText(PlayersFile);
                return JsonSerializer.Deserialize<List<Player>>(json) ?? new List<Player>();
            }
        }