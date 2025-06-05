using System;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;
using MEGAGame.Core.Services;
using MEGAGame.Core;

namespace MEGAGame.Client
{
    public partial class ThemeCreationWindow : MetroWindow
    {
        public Theme CreatedTheme { get; private set; }

        public ThemeCreationWindow()
        {
            InitializeComponent();
        }

        private void CreateTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ThemeNameTextBox.Text))
                {
                    MessageBox.Show("Введите название темы.", "Ошибка");
                    return;
                }

                if (RoundComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите раунд.", "Ошибка");
                    return;
                }

                int round = int.Parse((RoundComboBox.SelectedItem as ComboBoxItem).Tag.ToString());

                CreatedTheme = new Theme
                {
                    Name = ThemeNameTextBox.Text,
                    Round = round,
                    CreatedBy = GameSettings.PlayerId,
                    CreatedDate = DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании темы: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CreatedTheme = null;
            this.Close();
        }
    }
}