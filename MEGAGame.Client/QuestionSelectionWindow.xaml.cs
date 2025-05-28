using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;

namespace MEGAGame.Client
{
    public partial class QuestionSelectionWindow : MetroWindow
    {
        private readonly QuestionPack selectedPack;
        private readonly List<Theme> themes;

        public QuestionSelectionWindow(QuestionPack pack, List<Theme> themes)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            this.selectedPack = pack;
            this.themes = themes;

            ThemeComboBox.ItemsSource = themes;
            ThemeComboBox.DisplayMemberPath = "Name";
            ThemeComboBox.SelectedValuePath = "ThemeId";
            if (themes.Count > 0)
            {
                ThemeComboBox.SelectedIndex = 0;
            }
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ThemeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тему.", "Ошибка");
                    return;
                }

                var selectedTheme = ThemeComboBox.SelectedItem as Theme;
                if (selectedTheme == null)
                {
                    MessageBox.Show("Ошибка выбора темы.", "Ошибка");
                    return;
                }

                if (selectedTheme.Round == 1)
                {
                    var editWindow = new QuestionEditWindowRound1(selectedPack, selectedTheme);
                    editWindow.ShowDialog();
                }
                else
                {
                    var editWindow = new QuestionEditWindowRound234(selectedPack, selectedTheme);
                    editWindow.ShowDialog();
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении вопроса: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}