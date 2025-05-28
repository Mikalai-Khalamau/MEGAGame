using System;
using System.Collections.Generic;
using System.Linq;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;

namespace MEGAGame.Core.Services
{
    public static class PlayerService
    {
        public static void SavePlayer(Player player)
        {
            using (var context = new GameDbContext())
            {
                context.Players.Add(player);
                context.SaveChanges();
            }
        }

        public static List<Player> LoadAllPlayers()
        {
            using (var context = new GameDbContext())
            {
                return context.Players.ToList();
            }
        }

        public static Player GetPlayerByUsername(string username)
        {
            using (var context = new GameDbContext())
            {
                return context.Players.FirstOrDefault(p => p.Username == username);
            }
        }

        public static Player GetPlayerByEmail(string email)
        {
            using (var context = new GameDbContext())
            {
                return context.Players.FirstOrDefault(p => p.Email == email);
            }
        }

        public static void UpdatePlayer(Player player)
        {
            using (var context = new GameDbContext())
            {
                context.Players.Update(player);
                context.SaveChanges();
            }
        }
    }
}