using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class AchievementsWindow : Window
    {
        private readonly Player currentPlayer;

        public AchievementsWindow(Player player)
        {
            InitializeComponent();
            currentPlayer = player;
            LoadAchievements();
        }

        private void LoadAchievements()
        {
            var allAchievements = AchievementService.GetAllAchievements();
            var playerAchievements = AchievementService.GetPlayerAchievements(currentPlayer.PlayerId);

            var achievementsDisplay = allAchievements.Select(a => new
            {
                Name = a.Name,
                IsAchieved = playerAchievements.Any(pa => pa.AchievementId == a.AchievementId)
            }).ToList();

            AchievementsList.ItemsSource = achievementsDisplay;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainMenu = new MainMenuWindow(currentPlayer);
            mainMenu.Show();
            this.Close();
        }
    }
}