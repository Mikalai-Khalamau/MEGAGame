using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;
using Microsoft.VisualBasic;

namespace MEGAGame.Client
{
    public partial class MainMenuWindow : Window
    {
        private Player _currentPlayer;

        public MainMenuWindow(Player player)
        {
            InitializeComponent();
            _currentPlayer = player;
            UpdatePlayerInfo();
        }

        private void UpdatePlayerInfo()
        {
            if (_currentPlayer != null)
            {
                PlayerUsernameTextBlock.Text = _currentPlayer.Username;
                PlayerRatingTextBlock.Text = $"Рейтинг: {_currentPlayer.Rating}";
            }
        }

        private void SinglePlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks
                        .Where(p => p.IsPublished)
                        .ToList();

                    if (!packs.Any())
                    {
                        MessageBox.Show("Нет опубликованных пакетов вопросов! Создайте и опубликуйте пакет в редакторе вопросов.");
                        return;
                    }

                    var packSelectionWindow = new SinglePlayerPackSelectionWindow(_currentPlayer);
                    packSelectionWindow.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии выбора пакетов: {ex.Message}", "Ошибка");
            }
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
            new RankingWindow().Show();
            this.Close();
        }

        private void QuestionEditor_Click(object sender, RoutedEventArgs e)
        {
            new QuestionEditorWindow().Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}