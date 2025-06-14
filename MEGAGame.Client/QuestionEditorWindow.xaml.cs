﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using MahApps.Metro.Controls;
using MEGAGame.Core.Services;
using MEGAGame.Core;
namespace MEGAGame.Client
{
    public partial class QuestionEditorWindow : MetroWindow
    {
        private QuestionPack selectedPack;
        private Question selectedQuestion;

        public QuestionEditorWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;

            if (PackList == null || QuestionList == null || ThemeList == null)
            {
                MessageBox.Show("Ошибка инициализации интерфейса. Элементы управления не найдены.", "Ошибка");
                this.Close();
                return;
            }

            LoadPacks();
        }

        private void LoadPacks()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var packs = context.QuestionPacks
                        .Where(p => p.CreatedBy == GameSettings.PlayerId)
                        .ToList();
                    PackList.ItemsSource = packs;
                    PackList.DisplayMemberPath = "Name";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пакетов: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void LoadQuestions()
        {
            if (selectedPack == null) return;

            try
            {
                using (var context = new GameDbContext())
                {
                    // Загружаем вопросы
                    var questionsQuery = context.Questions
                        .Where(q => q.PackId == selectedPack.PackId);

                    // Попробуем загрузить данные поэтапно для отладки
                    var questions = questionsQuery
                        .Select(q => new
                        {
                            q.QuestionId,
                            q.Text,
                            q.Option1,
                            q.Option2,
                            q.Option3,
                            q.Option4,
                            q.Answer,
                            q.Answer2,
                            q.Answer3,
                            q.ThemeId,
                            q.PackId
                        })
                        .ToList();

                    // Если ошибка не возникла, продолжаем
                    var themes = context.Themes
                        .Where(t => t.PackId == selectedPack.PackId)
                        .ToDictionary(t => t.ThemeId, t => t.Name ?? "Без темы");

                    var groupedQuestions = questions.Select(q => new
                    {
                        QuestionId = q.QuestionId,
                        ThemeName = themes.ContainsKey(q.ThemeId) ? themes[q.ThemeId] : "Тема не найдена",
                        QuestionText = q.Text ?? "Без текста",
                        DisplayText = $"{(themes.ContainsKey(q.ThemeId) ? themes[q.ThemeId] : "Тема не найдена")}: {q.Text ?? "Без текста"}"
                    }).ToList();

                    QuestionList.ItemsSource = groupedQuestions;
                    QuestionList.DisplayMemberPath = "DisplayText";
                    QuestionList.SelectedValuePath = "QuestionId";

                    LoadThemes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке вопросов: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void LoadThemes()
        {
            if (selectedPack == null) return;

            try
            {
                using (var context = new GameDbContext())
                {
                    var themes = context.Themes
                        .Where(t => t.PackId == selectedPack.PackId)
                        .Select(t => new
                        {
                            ThemeId = t.ThemeId,
                            DisplayText = $"{t.Name ?? "Без названия"} (Раунд {t.Round})"
                        })
                        .ToList();

                    ThemeList.ItemsSource = themes;
                    ThemeList.SelectedValuePath = "ThemeId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке тем: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void PackList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                selectedPack = PackList.SelectedItem as QuestionPack;
                if (selectedPack != null)
                {
                    PackNameLabel.Text = selectedPack.Name ?? "Без названия";
                    LoadQuestions();
                }
                else
                {
                    PackNameLabel.Text = "";
                    QuestionList.ItemsSource = null;
                    ThemeList.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе пакета: {ex.Message}", "Ошибка");
            }
        }

        private void CreatePack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string packName = Interaction.InputBox("Введите название пакета:", "Создание пакета");
                if (string.IsNullOrWhiteSpace(packName)) return;

                using (var context = new GameDbContext())
                {
                    if (context.QuestionPacks.Any(p => p.Name == packName))
                    {
                        MessageBox.Show("Пакет с таким названием уже существует.", "Ошибка");
                        return;
                    }

                    var pack = new QuestionPack
                    {
                        Name = packName,
                        CreatedBy = GameSettings.PlayerId,
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsPublished = false
                    };
                    context.QuestionPacks.Add(pack);
                    context.SaveChanges();
                    LoadPacks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании пакета: {ex.Message}", "Ошибка");
            }
        }

        private void PublishPack_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPack == null)
                {
                    MessageBox.Show("Выберите пакет для публикации.", "Ошибка");
                    return;
                }

                using (var context = new GameDbContext())
                {
                    var packToPublish = context.QuestionPacks
                        .Include(p => p.Themes)
                        .ThenInclude(t => t.Questions)
                        .FirstOrDefault(p => p.PackId == selectedPack.PackId);
                    if (packToPublish == null)
                    {
                        MessageBox.Show("Пакет не найден.", "Ошибка");
                        return;
                    }

                    if (packToPublish.IsPublished)
                    {
                        MessageBox.Show("Этот пакет уже опубликован.", "Информация");
                        return;
                    }

                    var themesByRound = packToPublish.Themes
                        .GroupBy(t => t.Round)
                        .ToDictionary(g => g.Key, g => g.ToList());

                    bool allRoundsHaveThemes = true;
                    string missingRoundsMessage = "";
                    for (int round = 1; round <= 4; round++)
                    {
                        if (!themesByRound.ContainsKey(round) || themesByRound[round].Count == 0)
                        {
                            allRoundsHaveThemes = false;
                            missingRoundsMessage += $"Раунд {round}, ";
                        }
                    }
                    if (!allRoundsHaveThemes)
                    {
                        missingRoundsMessage = missingRoundsMessage.TrimEnd(',', ' ');
                        MessageBox.Show($"Невозможно опубликовать пакет. В каждом раунде (1-4) должна быть минимум одна тема. Отсутствуют темы для: {missingRoundsMessage}.", "Ошибка");
                        return;
                    }

                    bool allThemesHaveQuestions = true;
                    string themesWithoutQuestions = "";
                    foreach (var theme in packToPublish.Themes)
                    {
                        if (theme.Questions == null || theme.Questions.Count == 0)
                        {
                            allThemesHaveQuestions = false;
                            themesWithoutQuestions += $"{theme.Name ?? "Без названия"} (Раунд {theme.Round}), ";
                        }
                    }
                    if (!allThemesHaveQuestions)
                    {
                        themesWithoutQuestions = themesWithoutQuestions.TrimEnd(',', ' ');
                        MessageBox.Show($"Невозможно опубликовать пакет. В каждой теме должен быть минимум один вопрос. Отсутствуют вопросы в темах: {themesWithoutQuestions}.", "Ошибка");
                        return;
                    }

                    var result = MessageBox.Show($"Вы уверены, что хотите опубликовать пакет '{selectedPack.Name ?? "Без названия"}'?",
                        "Подтверждение публикации", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;

                    packToPublish.IsPublished = true;
                    packToPublish.LastUpdated = DateTime.Now;
                    context.SaveChanges();

                    MessageBox.Show($"Пакет '{selectedPack.Name ?? "Без названия"}' успешно опубликован!", "Успех");
                    LoadPacks();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при публикации пакета: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void CreateTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPack == null)
                {
                    MessageBox.Show("Выберите пакет перед созданием темы.", "Ошибка");
                    return;
                }

                var themeWindow = new ThemeCreationWindow();
                themeWindow.ShowDialog();

                if (themeWindow.CreatedTheme != null)
                {
                    using (var context = new GameDbContext())
                    {
                        var existingThemes = context.Themes
                            .Where(t => t.CreatedBy == GameSettings.PlayerId && t.Round == themeWindow.CreatedTheme.Round)
                            .ToList();

                        if ((themeWindow.CreatedTheme.Round >= 1 && themeWindow.CreatedTheme.Round <= 3 && existingThemes.Count >= 5) ||
                            (themeWindow.CreatedTheme.Round == 4 && existingThemes.Count >= 1))
                        {
                            MessageBox.Show("Достигнуто максимальное количество тем для этого раунда.", "Ошибка");
                            return;
                        }

                        if (existingThemes.Any(t => t.Name == themeWindow.CreatedTheme.Name))
                        {
                            MessageBox.Show("Тема с таким названием уже существует.", "Ошибка");
                            return;
                        }

                        themeWindow.CreatedTheme.PackId = selectedPack.PackId;
                        context.Themes.Add(themeWindow.CreatedTheme);
                        context.SaveChanges();
                        LoadQuestions();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании темы: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void AddQuestionsToTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedPack == null)
                {
                    MessageBox.Show("Выберите пакет перед добавлением вопросов.", "Ошибка");
                    return;
                }

                using (var context = new GameDbContext())
                {
                    var themes = context.Themes
                        .Where(t => t.PackId == selectedPack.PackId)
                        .ToList();

                    if (!themes.Any())
                    {
                        MessageBox.Show("Создайте хотя бы одну тему перед добавлением вопросов.", "Ошибка");
                        return;
                    }

                    var questionCreationWindow = new QuestionSelectionWindow(selectedPack, themes);
                    questionCreationWindow.ShowDialog();
                    LoadQuestions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении вопроса: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void EditQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QuestionList.SelectedItem == null)
                {
                    MessageBox.Show("Выберите вопрос для редактирования.", "Ошибка");
                    return;
                }

                dynamic selectedItem = QuestionList.SelectedItem;
                int questionId = selectedItem.QuestionId;

                using (var context = new GameDbContext())
                {
                    selectedQuestion = context.Questions
                        .Include(q => q.Theme)
                        .FirstOrDefault(q => q.QuestionId == questionId);

                    if (selectedQuestion == null)
                    {
                        MessageBox.Show("Вопрос не найден.", "Ошибка");
                        return;
                    }

                    if (selectedQuestion.Round == 1)
                    {
                        var editWindow = new QuestionEditWindowRound1(selectedPack, selectedQuestion);
                        editWindow.ShowDialog();
                    }
                    else
                    {
                        var editWindow = new QuestionEditWindowRound234(selectedPack, selectedQuestion);
                        editWindow.ShowDialog();
                    }

                    LoadQuestions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании вопроса: {ex.Message}", "Ошибка");
            }
        }

        private void DeleteQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QuestionList.SelectedItem == null)
                {
                    MessageBox.Show("Выберите вопрос для удаления.", "Ошибка");
                    return;
                }

                dynamic selectedItem = QuestionList.SelectedItem;
                int questionId = selectedItem.QuestionId;

                using (var context = new GameDbContext())
                {
                    var question = context.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                    if (question == null)
                    {
                        MessageBox.Show("Вопрос не найден.", "Ошибка");
                        return;
                    }

                    var result = MessageBox.Show($"Вы уверены, что хотите удалить вопрос: {question.Text ?? "Без текста"}?", "Подтверждение удаления", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;

                    context.Questions.Remove(question);
                    context.SaveChanges();
                    LoadQuestions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении вопроса: {ex.Message}", "Ошибка");
            }
        }

        private void DeleteTheme_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ThemeList.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тему для удаления.", "Ошибка");
                    return;
                }

                dynamic selectedItem = ThemeList.SelectedItem;
                int themeId = selectedItem.ThemeId;

                using (var context = new GameDbContext())
                {
                    var theme = context.Themes
                        .Include(t => t.Questions)
                        .FirstOrDefault(t => t.ThemeId == themeId);
                    if (theme == null)
                    {
                        MessageBox.Show("Тема не найдена.", "Ошибка");
                        return;
                    }

                    var result = MessageBox.Show($"Вы уверены, что хотите удалить тему '{theme.Name ?? "Без названия"}'? Все связанные вопросы также будут удалены.",
                        "Подтверждение удаления", MessageBoxButton.YesNo);
                    if (result != MessageBoxResult.Yes) return;

                    if (theme.Questions != null && theme.Questions.Any())
                    {
                        context.Questions.RemoveRange(theme.Questions);
                    }

                    context.Themes.Remove(theme);
                    context.SaveChanges();
                    LoadQuestions();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении темы: {ex.Message}", "Ошибка");
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var player = context.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
                    if (player != null)
                    {
                        var mainMenu = new MainMenuWindow(player);
                        mainMenu.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Не удалось найти игрока. Пожалуйста, войдите снова.", "Ошибка");
                        var loginWindow = new LoginWindow();
                        loginWindow.Show();
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при возврате в главное меню: {ex.Message}", "Ошибка");
            }
        }

        private void QuestionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Пустой обработчик
        }
    }
}