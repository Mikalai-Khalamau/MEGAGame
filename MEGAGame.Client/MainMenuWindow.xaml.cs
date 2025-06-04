using System;
using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class MainMenuWindow : Window
    {
        private readonly Player _currentPlayer;
        private static bool _isFirstLaunch = true;

        public MainMenuWindow(Player player)
        {
            InitializeComponent();
            _currentPlayer = player ?? throw new ArgumentNullException(nameof(player));
            UpdatePlayerInfo();
            InitializeMusicSelection();

            if (_isFirstLaunch)
            {
                MusicPlayer.PlayMusic();
                _isFirstLaunch = false;
            }
        }

        private void UpdatePlayerInfo()
        {
            if (_currentPlayer != null)
            {
                PlayerUsernameTextBlock.Text = _currentPlayer.Username;
                PlayerRatingTextBlock.Text = $"Рейтинг: {_currentPlayer.Rating}";
            }
        }

        private void InitializeMusicSelection()
        {
            MusicSelectionComboBox.SelectedIndex = GameSettings.SelectedMusicTrackIndex;
        }

        private void MusicSelectionComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int newTrackIndex = MusicSelectionComboBox.SelectedIndex;
            GameSettings.SelectedMusicTrackIndex = newTrackIndex;
            MusicPlayer.ChangeTrack(newTrackIndex);
        }

        private void SinglePlayer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks.Where(p => p.IsPublished).ToList();
                    if (!packs.Any())
                    {
                        MessageBox.Show("Нет опубликованных пакетов вопросов! Создайте и опубликуйте пакет в редакторе вопросов.");
                        return;
                    }
                    GameSettings.GameMode = GameSettings.GameModeType.SinglePlayer;
                    var packSelectionWindow = new SinglePlayerPackSelectionWindow(_currentPlayer, false, false);
                    packSelectionWindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии выбора пакетов: {ex.Message}", "Ошибка");
            }
        }

        private void PlayWithFriend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks.Where(p => p.IsPublished).ToList();
                    if (!packs.Any())
                    {
                        MessageBox.Show("Нет опубликованных пакетов вопросов! Создайте и опубликуйте пакет в редакторе вопросов.");
                        return;
                    }
                    GameSettings.GameMode = GameSettings.GameModeType.Friend;
                    var packSelectionWindow = new SinglePlayerPackSelectionWindow(_currentPlayer, true, false);
                    packSelectionWindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии выбора пакетов: {ex.Message}", "Ошибка");
            }
        }

        private void PlayWithBot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks.Where(p => p.IsPublished).ToList();
                    if (!packs.Any())
                    {
                        MessageBox.Show("Нет опубликованных пакетов вопросов! Создайте и опубликуйте пакет в редакторе вопросов.");
                        return;
                    }
                    GameSettings.GameMode = GameSettings.GameModeType.Bot;
                    var packSelectionWindow = new SinglePlayerPackSelectionWindow(_currentPlayer, false, true);
                    packSelectionWindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии выбора пакетов: {ex.Message}", "Ошибка");
            }
        }

        private void PlayOnline_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks.Where(p => p.IsPublished).ToList();
                    if (!packs.Any())
                    {
                        MessageBox.Show("Нет опубликованных пакетов вопросов! Создайте и опубликуйте пакет в редакторе вопросов.");
                        return;
                    }

                    GameSettings.GameMode = GameSettings.GameModeType.Online;
                    Console.WriteLine($"GameMode set to: {GameSettings.GameMode}");

                    var packSelectionWindow = new SinglePlayerPackSelectionWindow(_currentPlayer, false, false);
                    packSelectionWindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии выбора пакетов: {ex.Message}", "Ошибка");
            }
        }

        private void ShowRating_Click(object sender, RoutedEventArgs e)
        {
            new RankingWindow().Show();
            Close();
        }

        private void ShowAchievements_Click(object sender, RoutedEventArgs e)
        {
            var achievementsWindow = new AchievementsWindow(_currentPlayer);
            achievementsWindow.Show();
            Close();
        }

        private void QuestionEditor_Click(object sender, RoutedEventArgs e)
        {
            new QuestionEditorWindow().Show();
            Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MusicPlayer.StopMusic();
            Application.Current.Shutdown();
        }
    }
}