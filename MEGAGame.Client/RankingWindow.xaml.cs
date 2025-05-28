using System.Linq;
using System.Windows;
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
            using (var context = new GameDbContext())
            {
                var players = context.Players
                    .OrderByDescending(p => p.Rating)
                    .ToList();

                var rankedPlayers = players.Select((player, index) => new
                {
                    Rank = index + 1,
                    Name = player.Username,
                    Email = player.Email,
                    player.Rating
                }).ToList();

                RankingList.ItemsSource = rankedPlayers;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new GameDbContext())
            {
                var player = context.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
                if (player != null)
                {
                    new MainMenuWindow(player).Show();
                    this.Close();
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}