using System.Windows;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class MainMenuWindow : Window
    {
        public MainMenuWindow()
        {
            InitializeComponent();
        }

        private void SinglePlayer_Click(object sender, RoutedEventArgs e)
        {
            GameSettings.CurrentRound = 1;
            GameSettings.PlayerScore = 0;
            new MainWindow().Show();
            this.Close();
        }

        private void PlayWithFriend_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Режим 'Игра с другом' пока не реализован.");
        }

        private void PlayOnline_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Режим 'Игра по сети' пока не реализован.");
        }

        private void ShowRating_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Рейтинг пока не реализован.");
        }

        private void QuestionEditor_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Редактор вопросов пока не реализован.");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}