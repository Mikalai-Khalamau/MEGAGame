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
        private readonly Player currentPlayer;
        private List<PackDisplayItem> packItems;

        public SinglePlayerPackSelectionWindow(Player player)
        {
            InitializeComponent();
            currentPlayer = player;
            LoadPacks();
        }

        private void LoadPacks()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    // Получаем все опубликованные пакеты
                    var packs = context.QuestionPacks
                        .Where(p => p.IsPublished)
                        .ToList();

                    // Получаем список сыгранных пакетов для текущего игрока
                    var playedPacks = context.PlayedPacks
                        .Where(pp => pp.PlayerId == currentPlayer.PlayerId)
                        .Select(pp => pp.PackId)
                        .ToList();

                    // Формируем отображаемые элементы
                    packItems = new List<PackDisplayItem>();
                    foreach (var pack in packs)
                    {
                        var creator = context.Players.FirstOrDefault(p => p.PlayerId == pack.CreatedBy);
                        string creatorName = creator?.Username ?? "Неизвестный";
                        bool isPlayed = playedPacks.Contains(pack.PackId);

                        packItems.Add(new PackDisplayItem
                        {
                            PackId = pack.PackId,
                            DisplayName = $"{pack.Name} (by {creatorName})",
                            IsPlayed = isPlayed
                        });
                    }

                    PackListBox.ItemsSource = packItems;
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

                if (selectedPackItem.IsPlayed)
                {
                    MessageBox.Show("Вы уже играли этот пакет. Выберите другой.", "Ошибка");
                    return;
                }

                // Устанавливаем выбранный пакет и запускаем игру
                GameSettings.SelectedPackId = selectedPackItem.PackId;
                GameSettings.CurrentRound = 1;
                GameSettings.PlayerScore = 0;

                var gameWindow = new MainWindow();
                gameWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}", "Ошибка");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var mainMenu = new MainMenuWindow(currentPlayer);
            mainMenu.Show();
            this.Close();
        }
    }
}