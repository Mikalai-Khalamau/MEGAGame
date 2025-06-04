using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

                    _packItems = new List<PackDisplayItem>();
                    foreach (var pack in packs)
                    {
                        var creator = context.Players.FirstOrDefault(p => p.PlayerId == pack.CreatedBy);
                        string creatorName = creator?.Username ?? "Неизвестный";
                        bool isPlayed = playedPacks.Contains(pack.PackId);

                        _packItems.Add(new PackDisplayItem
                        {
                            PackId = pack.PackId,
                            DisplayName = $"{pack.Name} (by {creatorName})",
                            IsPlayed = isPlayed
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

        private void StartGame_Click(object sender, RoutedEventArgs e)
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

                if (selectedPackItem.IsPlayed && GameSettings.GameMode != GameSettings.GameModeType.Online)
                {
                    MessageBox.Show("Вы уже играли этот пакет. Выберите другой.", "Ошибка");
                    return;
                }

                GameSettings.SelectedPackId = selectedPackItem.PackId;
                GameSettings.CurrentRound = 1;
                GameSettings.PlayerScore = 0;

                if (GameSettings.GameMode == GameSettings.GameModeType.Online)
                {
                    var onlineGameWindow = new OnlineGameWindow(_currentPlayer);
                    onlineGameWindow.Show();
                }
                else if (_isBotMode)
                {
                    var botDifficultyWindow = new BotDifficultySelectionWindow();
                    botDifficultyWindow.Show();
                }
                else if (_isFriendMode)
                {
                    var friendGameWindow = new FriendGameWindow();
                    friendGameWindow.Show();
                }
                else
                {
                    var gameWindow = new MainWindow();
                    gameWindow.Show();
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}", "Ошибка");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainMenu = new MainMenuWindow(_currentPlayer);
            mainMenu.Show();
            Close();
        }
    }
}