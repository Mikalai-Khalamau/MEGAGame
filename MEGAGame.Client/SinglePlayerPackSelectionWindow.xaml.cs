using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public class PackDisplayItem
    {
        public int PackId { get; set; }
        public string DisplayName { get; set; }
        public bool IsPlayed { get; set; }
        public int QuestionCount { get; set; }
    }

    [ValueConversion(typeof(bool), typeof(System.Windows.Media.Color))]
    public class PlayedToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isPlayed = (bool)value;
            return isPlayed ? System.Windows.Media.Colors.Gray : System.Windows.Media.Colors.Green;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class SinglePlayerPackSelectionWindow : Window
    {
        private readonly Player _currentPlayer;
        private List<PackDisplayItem> _packItems;
        private readonly bool _isFriendMode;
        private readonly bool _isBotMode;

        public SinglePlayerPackSelectionWindow(Player player, bool friendMode = false, bool isBotMode = false)
        {
            InitializeComponent();
            _currentPlayer = player ?? throw new ArgumentNullException(nameof(player));
            _isFriendMode = friendMode;
            _isBotMode = isBotMode;
            LoadPacks();
        }

        private void LoadPacks()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks
                        .Where(p => p.IsPublished)
                        .ToList();

                    var playedPacks = context.PlayedPacks
                        .Where(pp => pp.PlayerId == _currentPlayer.PlayerId)
                        .Select(pp => pp.PackId)
                        .ToList();

                    var questionCounts = context.Questions
                        .GroupBy(q => q.PackId)
                        .Select(g => new { PackId = g.Key, Count = g.Count() })
                        .ToDictionary(g => g.PackId, g => g.Count);

                    _packItems = new List<PackDisplayItem>();
                    foreach (var pack in packs)
                    {
                        var creator = context.Players.FirstOrDefault(p => p.PlayerId == pack.CreatedBy);
                        string creatorName = creator?.Username ?? "Неизвестный";
                        bool isPlayed = playedPacks.Contains(pack.PackId);
                        int questionCount = questionCounts.ContainsKey(pack.PackId) ? questionCounts[pack.PackId] : 0;

                        _packItems.Add(new PackDisplayItem
                        {
                            PackId = pack.PackId,
                            DisplayName = $"{pack.Name} (by {creatorName})",
                            IsPlayed = isPlayed,
                            QuestionCount = questionCount
                        });
                    }

                    PackListBox.ItemsSource = _packItems;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пакетов: {ex.Message}", "Ошибка");
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PackListBox.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите пакет вопросов.", "Ошибка");
                    return;
                }

                var selectedPackItem = PackListBox.SelectedItem as PackDisplayItem;
                if (selectedPackItem == null)
                {
                    MessageBox.Show("Ошибка выбора пакета.", "Ошибка");
                    return;
                }

                if (selectedPackItem.IsPlayed)
                {
                    MessageBox.Show("Вы уже играли этот пакет. Выберите другой.", "Ошибка");
                    return;
                }

                GameSettings.SelectedPackId = selectedPackItem.PackId;
                GameSettings.CurrentRound = 1;
                GameSettings.PlayerScore = 0;

                Window nextWindow = null;
                if (_isBotMode)
                {
                    nextWindow = new BotDifficultySelectionWindow();
                }
                else if (_isFriendMode)
                {
                    nextWindow = new FriendGameWindow();
                }
                else
                {
                    nextWindow = new MainWindow();
                }

                if (nextWindow != null)
                {
                    nextWindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}", "Ошибка");
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var mainMenu = new MainMenuWindow(_currentPlayer);
                mainMenu.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при возврате в меню: {ex.Message}", "Ошибка");
            }
        }
    }
}