using System;
using System.Collections.Generic;
using System.Linq;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;

namespace MEGAGame.Core.Services
{
    public static class AchievementService
    {
        public static void InitializeAchievements()
        {
            using (var context = new GameDbContext())
            {
                if (!context.Achievements.Any())
                {
                    var achievements = new List<Achievement>
                    {
                        new Achievement { AchievementId = 1, Name = "Мастер пакета(Все вопросы в пакете отвечены верно)", Description = "Все вопросы в пакете отвечены верно." },
                        new Achievement { AchievementId = 2, Name = "Тройной удар(3 подряд верных ответа)", Description = "3 подряд верных ответа." },
                        new Achievement { AchievementId = 3, Name = "Пятерка(5 подряд верных ответов)", Description = "5 подряд верных ответов." },
                        new Achievement { AchievementId = 4, Name = "Десятка(10 подряд верных ответов)", Description = "10 подряд верных ответов." },
                        new Achievement { AchievementId = 5, Name = "Победитель сложного бота(Победил сложного бота)", Description = "Победил сложного бота." },
                        new Achievement { AchievementId = 6, Name = "Победитель простого бота(Победил простого бота)", Description = "Победил простого бота." },
                        new Achievement { AchievementId = 7, Name = "Победитель среднего бота(Победил среднего бота)", Description = "Победил среднего бота." },
                        new Achievement { AchievementId = 8, Name = "Рейтинг 2000+(Рейтинг больше 2000)", Description = "Рейтинг больше 2000." },
                        new Achievement { AchievementId = 9, Name = "Рейтинг 3000+(Рейтинг больше 3000)", Description = "Рейтинг больше 3000." },
                        new Achievement { AchievementId = 10, Name = "Рейтинг 5000+(Рейтинг больше 5000)", Description = "Рейтинг больше 5000." }
                    };
                    context.Achievements.AddRange(achievements);
                    context.SaveChanges();
                }
            }
        }

        public static List<Achievement> GetAllAchievements()
        {
            using (var context = new GameDbContext())
            {
                return context.Achievements.ToList();
            }
        }

        public static List<PlayerAchievement> GetPlayerAchievements(int playerId)
        {
            using (var context = new GameDbContext())
            {
                return context.PlayerAchievements
                    .Where(pa => pa.PlayerId == playerId)
                    .ToList();
            }
        }

        public static void AwardAchievement(int playerId, int achievementId)
        {
            using (var context = new GameDbContext())
            {
                if (!context.PlayerAchievements.Any(pa => pa.PlayerId == playerId && pa.AchievementId == achievementId))
                {
                    var playerAchievement = new PlayerAchievement
                    {
                        PlayerId = playerId,
                        AchievementId = achievementId,
                        AchievedDate = DateTime.Now
                    };
                    context.PlayerAchievements.Add(playerAchievement);
                    context.SaveChanges();
                }
            }
        }
    }
}