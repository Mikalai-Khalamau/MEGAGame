using System.Windows;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class BotDifficultySelectionWindow : Window
    {
        public BotDifficultySelectionWindow()
        {
            InitializeComponent();
        }

        private void EasyBot_Click(object sender, RoutedEventArgs e)
        {
            GameSettings.BotDifficulty = GameSettings.BotDifficultyLevel.Easy;
            OpenBotGameWindow();
        }

        private void MediumBot_Click(object sender, RoutedEventArgs e)
        {
            GameSettings.BotDifficulty = GameSettings.BotDifficultyLevel.Medium;
            OpenBotGameWindow();
        }

        private void HardBot_Click(object sender, RoutedEventArgs e)
        {
            GameSettings.BotDifficulty = GameSettings.BotDifficultyLevel.Hard;
            OpenBotGameWindow();
        }

        private void OpenBotGameWindow()
        {
            var botGameWindow = new BotGameWindow();
            botGameWindow.Show();
            Close();
        }
    }
}