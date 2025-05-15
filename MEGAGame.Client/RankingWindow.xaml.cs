using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;

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
                    player.Name,
                    player.City,
                    player.Rating
                }).ToList();

                RankingList.ItemsSource = rankedPlayers;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new MainMenuWindow().Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}