using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;
using MEGAGame.Core.Services;
using MEGAGame.Core;
namespace MEGAGame.Client
{
    public partial class QuestionSelectionWindow : MetroWindow
    {
        private readonly QuestionPack selectedPack;
        private readonly List<Theme> themes;
        private Theme selectedTheme;

        public QuestionSelectionWindow(QuestionPack pack, List<Theme> availableThemes)
        {
            InitializeComponent();
            selectedPack = pack;

            // Загружаем темы с обработкой NULL
            using (var context = new GameDbContext())
            {
                themes = context.Themes
                    .Where(t => t.PackId == selectedPack.PackId)
                    .Select(t => new Theme
                    {
                        ThemeId = t.ThemeId,
                        Name = t.Name ?? "Без названия", // Обработка NULL для Name
                        PackId = t.PackId,
                        Round = t.Round
                    })
                    .ToList();

                if (!themes.Any())
                {
                    MessageBox.Show("Темы для выбранного пакета не найдены.", "Ошибка");
                    Close();
                    return;
                }

                ThemeComboBox.ItemsSource = themes;
                ThemeComboBox.DisplayMemberPath = "Name";
                ThemeComboBox.SelectedValuePath = "ThemeId";
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selectedTheme = ThemeComboBox.SelectedItem as Theme;
            if (selectedTheme != null)
            {
                ThemeNameLabel.Text = selectedTheme.Name ?? "Без названия";
                RoundLabel.Text = $"Раунд: {selectedTheme.Round}";
            }
            else
            {
                ThemeNameLabel.Text = "";
                RoundLabel.Text = "";
            }
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedTheme == null)
                {
                    MessageBox.Show("Выберите тему перед добавлением вопроса.", "Ошибка");
                    return;
                }

                // Проверяем текущее количество вопросов в теме
                using (var context = new GameDbContext())
                {
                    var existingQuestions = context.Questions
                        .Where(q => q.ThemeId == selectedTheme.ThemeId)
                        .ToList();

                    // Ограничения по количеству вопросов в зависимости от раунда
                    int maxQuestions = selectedTheme.Round switch
                    {
                        1 or 2 => 5,  // Раунды 1-2: максимум 5 вопросов
                        3 => 2,       // Раунд 3: максимум 2 вопроса
                        4 => 1,       // Раунд 4: максимум 1 вопрос
                        _ => 0
                    };

                    if (existingQuestions.Count >= maxQuestions)
                    {
                        MessageBox.Show($"Достигнуто максимальное количество вопросов для этой темы (максимум {maxQuestions}).", "Ошибка");
                        return;
                    }

                    // Создаём новый вопрос с пустыми полями
                    var newQuestion = new Question
                    {
                        Text = "",
                        Option1 = "",
                        Option2 = "",
                        Option3 = "",
                        Option4 = "",
                        CorrectOption = 1,
                        Answer = "",
                        Points = 0,
                        Round = selectedTheme.Round,
                        ThemeId = selectedTheme.ThemeId,
                        PackId = selectedPack.PackId,
                        CreatedBy = GameSettings.PlayerId,
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsPlayed = false
                    };

                    // Открываем окно создания вопроса для всех раундов
                    var creationWindow = new QuestionCreationWindow(selectedPack, selectedTheme);
                    creationWindow.ShowDialog();

                    // Если вопрос был сохранён, он уже добавлен в базу в QuestionCreationWindow
                    if (newQuestion.Text != "")
                    {
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении вопроса: {ex.Message}", "Ошибка");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}