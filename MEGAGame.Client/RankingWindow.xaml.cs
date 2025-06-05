using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Data;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class RankingWindow : Window
    {
        public RankingWindow()
        {
            InitializeComponent();
            LoadRanking();
        }

        private void LoadRanking()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var players = context.Players
                        .OrderByDescending(p => p.Rating)
                        .ToList();

                    if (!players.Any())
                    {
                        MessageBox.Show("Рейтинг пуст. Нет зарегистрированных игроков.", "Информация");
                        return;
                    }

                    var rankedPlayers = players.Select((player, index) => new
                    {
                        Rank = index + 1,
                        Name = player.Username,
                        Rating = player.Rating,
                        IsCurrentPlayer = player.PlayerId == GameSettings.PlayerId
                    }).ToList();

                    RankingList.ItemsSource = rankedPlayers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке рейтинга: {ex.Message}", "Ошибка");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var player = context.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
                    if (player != null)
                    {
                        new MainMenuWindow(player).Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Игрок не найден.", "Ошибка");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при возврате в меню: {ex.Message}", "Ошибка");
            }
        }

    }
}