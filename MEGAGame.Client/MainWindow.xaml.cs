using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using MEGAGame.Core;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace MEGAGame.Client
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private List<Theme> currentThemes;
        private List<Question> currentQuestions;
        private Question currentQuestion;
        private int selectedOption;
        private int playerBet;
        private bool isQuestionActive;
        private bool isBetPlaced;
        private bool isMouseOverButton;

        private bool isBetInputReadOnly;
        private int correctStreak = 0; // Added for streak-based achievements
        private bool answeredCorrectlyThisQuestion = false; // Track if the current question was answered correctly

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsBetInputReadOnly
        {
            get => isBetInputReadOnly;
            set
            {
                isBetInputReadOnly = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBetInputReadOnly)));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ResetQuestionsPlayedState();
            StartRound();
            UpdatePlayerInfo();
            isQuestionActive = false;
            isMouseOverButton = false;
            AchievementService.InitializeAchievements(); // Initialize achievements
        }

        private void ResetQuestionsPlayedState()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    var questions = context.Questions
                        .Where(q => q.PackId == GameSettings.SelectedPackId)
                        .ToList();
                    questions.ForEach(q => q.IsPlayed = false);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сбросе состояния вопросов: {ex.Message}", "Ошибка");
            }
        }

        private void StartRound()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    currentQuestions = context.Questions
                        .Include(q => q.Theme)
                        .Where(q => q.PackId == GameSettings.SelectedPackId && q.Round == GameSettings.CurrentRound)
                        .ToList();

                    currentThemes = currentQuestions
                        .Select(q => q.Theme)
                        .Distinct()
                        .ToList();

                    if (!currentQuestions.Any() || !currentThemes.Any())
                    {
                        MessageBox.Show($"Вопросов или тем для раунда {GameSettings.CurrentRound} в выбранном пакете не найдено!");
                        EndGame();
                        return;
                    }

                    if (GameSettings.CurrentRound == 4)
                    {
                        if (GameSettings.PlayerScore <= 0)
                        {
                            BetInput.Text = "1";
                            IsBetInputReadOnly = true;
                        }
                        else
                        {
                            BetInput.Text = "";
                            IsBetInputReadOnly = false;
                        }
                        BetPanel.Visibility = Visibility.Visible;
                        isBetPlaced = false;
                    }
                    else
                    {
                        isBetPlaced = true;
                    }

                    CreateQuestionGrid();
                    RoundInfo.Text = $"Раунд {GameSettings.CurrentRound}: {GetRoundName(GameSettings.CurrentRound)}";
                    UpdateRoundInfoPopup();

                    QuestionText.Visibility = Visibility.Collapsed;
                    OptionsPanel.Visibility = Visibility.Collapsed;
                    TextAnswerPanel.Visibility = Visibility.Collapsed;
                    ResultPanel.Visibility = Visibility.Collapsed;
                    GoToResultsButton.Visibility = Visibility.Collapsed;

                    if (GameSettings.CurrentRound == 4)
                    {
                        BetPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        BetPanel.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске раунда: {ex.Message}", "Ошибка");
                EndGame();
            }
        }

        private string GetRoundName(int round)
        {
            return round switch
            {
                1 => "Викторина",
                2 => "Своя игра",
                3 => "Что? Где? Когда?",
                4 => "Ставки",
                _ => "Неизвестный раунд"
            };
        }

        private void UpdateRoundInfoPopup()
        {
            string popupText = GameSettings.CurrentRound switch
            {
                1 => "В РАУНДЕ ВИКТОРИНА ВАМ ДАНЫ 1-5 ТЕМ ВОПРОСОВ, В КАЖДОЙ ТЕМЕ ВОПРОСЫ И ОТВЕТЫ СВЯЗАНЫ С НАЗВАНИЕМ ТЕМЫ, С 1-5 ВОПРОСАМИ В ТЕМЕ. В КАЖДОМ ВОПРОСЕ ЕСТЬ 4 ВАРИАНТА ОТВЕТА И ТОЛЬКО ОДИН ВЕРНЫЙ. ОТВЕТ ОБЯЗАТЕЛЬНЫЙ, ЗА НЕПРАВИЛЬНЫЙ ОТВЕТ ОЧКИ НЕ ОТНИМАЮТСЯ.",
                2 => "В РАУНДЕ СВОЯ ИГРА ВАМ ДАНЫ 1-5 ТЕМ ВОПРОСОВ, В КАЖДОЙ ТЕМЕ ВОПРОСЫ И ОТВЕТЫ СВЯЗАНЫ С НАЗВАНИЕМ ТЕМЫ, С 1-5 ВОПРОСАМИ В ТЕМЕ. ВЫ ДОЛЖНЫ ВВЕСТИ ОТВЕТ В ТЕКСТОВОЕ ПОЛЕ. ЕСЛИ ОТВЕТ ОТЛИЧАЕТСЯ ОТ ПРАВИЛЬНОГО НЕ БОЛЕЕ 2 СИМВОЛАМИ, ТО ОН БУДЕТ ЗАСЧИТАН КАК ВЕРНЫЙ. ЗА НЕПРАВИЛЬНЫЙ ОТВЕТ ВЫ ТЕРЯЕТЕ ОЧКИ. МОЖНО НЕ ОТВЕЧАТЬ НА ВОПРОС И ПРОСТО ОСТАВИТЬ ПУСТОЕ ПОЛЕ ДЛЯ ОТВЕТА И ТОГДА КОЛИЧЕСТВО ВАШИХ ОЧКОВ НЕ ИЗМЕНИТСЯ.",
                3 => "В РАУНДЕ ЧТО? ГДЕ? КОГДА? ВАМ ДАНЫ 1-5 ТЕМ ВОПРОСОВ, В КАЖДОЙ ТЕМЕ ВОПРОСЫ И ОТВЕТЫ СВЯЗАНЫ С НАЗВАНИЕМ ТЕМЫ, С 1-2 ВОПРОСАМИ В ТЕМЕ. ВЫ ДОЛЖНЫ ВВЕСТИ ОТВЕТ В ТЕКСТОВОЕ ПОЛЕ. ЕСЛИ ОТВЕТ ОТЛИЧАЕТСЯ ОТ ПРАВИЛЬНОГО НЕ БОЛЕЕ 2 СИМВОЛАМИ, ТО ОН БУДЕТ ЗАСЧИТAN КАК ВЕРНЫЙ. ЗА НЕВЕРНЫЙ ОТВЕТ ОЧКИ НЕ ОТНИМАЮТСЯ.",
                4 => "В РАУНДЕ СТАВКИ ВАМ ДАН ВОПРОС НА ОПРЕДЕЛЕННУЮ ТЕМУ И ВЫ ДЕЛАЕТЕ СТАВКУ ОТ 1 ДО КОЛИЧЕСТВА СВОИХ ОЧКОВ (ЕСЛИ У ВАС НЕПОЛОЖИТЕЛЬНОЕ ЧИСЛО ОЧКОВ, ТО СТАВКА АВТОМАТИЧЕСКИ РАВНА 1). ЕСЛИ ОТВЕТ ВЕРНЫЙ, ТО ВЫ ПОЛУЧАЕТЕ СТОЛЬКО ОЧКОВ, СКОЛЬКО ПОСТАВИЛИ. ЕСЛИ ОТВЕТ НЕВЕРНЫЙ, ТО ВЫ ТЕРЯЕТЕ СТОЛЬКО ОЧКОВ, СКОЛЬКО ПОСТАВИЛИ.",
                _ => "Информация о раунде недоступна."
            };
            RoundInfoText.Text = popupText;
        }

        private void InfoButton_MouseEnter(object sender, MouseEventArgs e)
        {
            isMouseOverButton = true;
            UpdateRoundInfoPopup();
            InfoPopup.IsOpen = true;
        }

        private void InfoButton_MouseLeave(object sender, MouseEventArgs e)
        {
            isMouseOverButton = false;
            if (!IsMouseOverPopup())
            {
                InfoPopup.IsOpen = false;
            }
        }

        private void Popup_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!isMouseOverButton && !IsMouseOverPopup())
            {
                InfoPopup.IsOpen = false;
            }
        }

        private bool IsMouseOverPopup()
        {
            return InfoPopup.IsMouseOver;
        }

        private void CreateQuestionGrid()
        {
            QuestionGrid.Children.Clear();
            QuestionGrid.RowDefinitions.Clear();
            QuestionGrid.ColumnDefinitions.Clear();

            int themeCount;
            int questionCount;
            double buttonWidth = double.PositiveInfinity;
            double buttonHeight = double.PositiveInfinity;

            if (GameSettings.CurrentRound == 1 || GameSettings.CurrentRound == 2)
            {
                themeCount = 5;
                questionCount = 5;
            }
            else if (GameSettings.CurrentRound == 3)
            {
                themeCount = 5;
                questionCount = 2;
            }
            else if (GameSettings.CurrentRound == 4)
            {
                themeCount = 1;
                questionCount = 1;
            }
            else
            {
                return;
            }

            for (int i = 0; i < themeCount; i++)
            {
                QuestionGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < questionCount; i++)
            {
                QuestionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }

            var themeQuestions = currentQuestions
                .GroupBy(q => q.ThemeId)
                .ToDictionary(g => g.Key, g => g.OrderBy(q => q.Points).ToList());

            for (int row = 0; row < themeCount; row++)
            {
                if (row < currentThemes.Count)
                {
                    var theme = currentThemes[row];
                    var questions = themeQuestions.ContainsKey(theme.ThemeId) ? themeQuestions[theme.ThemeId] : new List<Question>();

                    for (int col = 0; col < questionCount; col++)
                    {
                        if (col < questions.Count)
                        {
                            var question = questions[col];
                            var button = new Button
                            {
                                Tag = question.QuestionId,
                                Background = question.IsPlayed ? Brushes.Red : Brushes.LightGreen,
                                IsEnabled = !question.IsPlayed && !isQuestionActive && isBetPlaced,
                                Margin = new Thickness(2, 0, 2, 0),
                                Padding = new Thickness(10),
                                FontSize = 16,
                                HorizontalContentAlignment = HorizontalAlignment.Center,
                                VerticalContentAlignment = VerticalAlignment.Center,
                                MaxWidth = buttonWidth,
                                MaxHeight = buttonHeight
                            };

                            var textBlock = new TextBlock
                            {
                                Text = GameSettings.CurrentRound == 4 ? $"Тема: {theme.Name ?? "Без названия"}" : $"{theme.Name ?? "Без названия"} ({question.Points} очков)",
                                TextWrapping = TextWrapping.Wrap,
                                TextAlignment = TextAlignment.Center
                            };
                            button.Content = textBlock;

                            button.Click += QuestionButton_Click;
                            Grid.SetRow(button, row);
                            Grid.SetColumn(button, col);
                            QuestionGrid.Children.Add(button);
                        }
                        else
                        {
                            var emptyBlock = new TextBlock
                            {
                                Background = Brushes.Gray,
                                Margin = new Thickness(2, 0, 2, 0),
                            };
                            Grid.SetRow(emptyBlock, row);
                            Grid.SetColumn(emptyBlock, col);
                            QuestionGrid.Children.Add(emptyBlock);
                        }
                    }
                }
                else
                {
                    for (int col = 0; col < questionCount; col++)
                    {
                        var emptyBlock = new TextBlock
                        {
                            Background = Brushes.Gray,
                            Margin = new Thickness(2, 0, 2, 0),
                        };
                        Grid.SetRow(emptyBlock, row);
                        Grid.SetColumn(emptyBlock, col);
                        QuestionGrid.Children.Add(emptyBlock);
                    }
                }
            }
        }

        private void QuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                int questionId = (int)button.Tag;
                using (var context = new GameDbContext())
                {
                    currentQuestion = context.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                    if (currentQuestion != null)
                    {
                        button.Background = Brushes.Red;
                        button.IsEnabled = false;
                        isQuestionActive = true;
                        UpdateQuestionButtons();
                        DisplayQuestion();
                    }
                }
            }
        }

        private void UpdateQuestionButtons()
        {
            foreach (var child in QuestionGrid.Children)
            {
                if (child is Button button)
                {
                    var questionId = (int)button.Tag;
                    using (var context = new GameDbContext())
                    {
                        var question = context.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                        if (question != null)
                        {
                            button.IsEnabled = !question.IsPlayed && !isQuestionActive && isBetPlaced;
                        }
                    }
                }
            }
        }

        private void SubmitBet_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(BetInput.Text, out playerBet) || playerBet <= 0 || (playerBet > GameSettings.PlayerScore && GameSettings.PlayerScore > 0))
            {
                MessageBox.Show($"Пожалуйста, введите корректную ставку (от 1 до {GameSettings.PlayerScore})!");
                return;
            }

            BetPanel.Visibility = Visibility.Collapsed;
            isBetPlaced = true;
            UpdateQuestionButtons();

            using (var context = new GameDbContext())
            {
                currentQuestions = context.Questions
                    .Where(q => q.PackId == GameSettings.SelectedPackId && q.Round == GameSettings.CurrentRound && !q.IsPlayed)
                    .ToList();

                if (currentQuestions.Any())
                {
                    currentQuestion = currentQuestions.First();
                    currentQuestion.Points = playerBet;
                    context.SaveChanges();
                    DisplayQuestion();
                }
                else
                {
                    MessageBox.Show("Вопросы для Раунда 4 не найдены!");
                    EndGame();
                }
            }
        }

        private void DisplayQuestion()
        {
            if (currentQuestion != null)
            {
                QuestionText.Text = $"{currentQuestion.Text ?? "Без текста"} ({currentQuestion.Points} очков)";
                QuestionText.Visibility = Visibility.Visible;
                ResultPanel.Visibility = Visibility.Collapsed;
                GoToResultsButton.Visibility = Visibility.Collapsed;

                if (GameSettings.CurrentRound == 1)
                {
                    OptionsPanel.Visibility = Visibility.Visible;
                    TextAnswerPanel.Visibility = Visibility.Collapsed;
                    SubmitOptionsButton.Visibility = Visibility.Visible;

                    Option1.Content = currentQuestion.Option1 ?? "N/A";
                    Option2.Content = currentQuestion.Option2 ?? "N/A";
                    Option3.Content = currentQuestion.Option3 ?? "N/A";
                    Option4.Content = currentQuestion.Option4 ?? "N/A";
                    Option1.IsChecked = false;
                    Option2.IsChecked = false;
                    Option3.IsChecked = false;
                    Option4.IsChecked = false;
                }
                else
                {
                    OptionsPanel.Visibility = Visibility.Collapsed;
                    TextAnswerPanel.Visibility = Visibility.Visible;
                    TextAnswerSubmitButton.Visibility = Visibility.Visible;
                    TextAnswerInput.Text = "";
                }
                answeredCorrectlyThisQuestion = false; // Reset for the new question
            }
        }

        private void Option_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == Option1) selectedOption = 1;
            else if (sender == Option2) selectedOption = 2;
            else if (sender == Option3) selectedOption = 3;
            else if (sender == Option4) selectedOption = 4;
        }

        private void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (selectedOption == 0)
            {
                MessageBox.Show("Вы должны выбрать один из вариантов ответа в 1-м раунде!", "Ошибка");
                return;
            }

            using (var context = new GameDbContext())
            {
                var question = context.Questions.FirstOrDefault(q => q.QuestionId == currentQuestion.QuestionId);
                if (question != null)
                {
                    question.IsPlayed = true;
                    context.SaveChanges();
                }
            }

            if (selectedOption == currentQuestion.CorrectOption)
            {
                GameSettings.PlayerScore += currentQuestion.Points;
                ResultText.Text = "Правильно!";
                ResultText.Foreground = Brushes.Green;
                ScoreChangeText.Text = $"+{currentQuestion.Points} очков";
                answeredCorrectlyThisQuestion = true;
                correctStreak++;
                CheckStreakAchievements();
            }
            else
            {
                ResultText.Text = "Неправильно!";
                ResultText.Foreground = Brushes.Red;
                ScoreChangeText.Text = "0 очков";
                correctStreak = 0;
            }

            string correctAnswer = "";
            switch (currentQuestion.CorrectOption)
            {
                case 1: correctAnswer = currentQuestion.Option1 ?? "N/A"; break;
                case 2: correctAnswer = currentQuestion.Option2 ?? "N/A"; break;
                case 3: correctAnswer = currentQuestion.Option3 ?? "N/A"; break;
                case 4: correctAnswer = currentQuestion.Option4 ?? "N/A"; break;
            }
            CorrectAnswerText.Text = $"Правильный ответ: {correctAnswer}";

            ResultPanel.Visibility = Visibility.Visible;
            SubmitOptionsButton.Visibility = Visibility.Collapsed;
            UpdatePlayerInfo();
            isQuestionActive = false;
            UpdateQuestionButtons();
            CheckMasterPackageAchievement();
            CheckRoundCompletion();
        }

        private void SubmitTextAnswer_Click(object sender, RoutedEventArgs e)
        {
            string userAnswer = TextAnswerInput.Text.Trim();
            bool empty = string.IsNullOrWhiteSpace(userAnswer);

            if (GameSettings.CurrentRound == 2 && empty)
            {
                ResultText.Text = "Ответ пропущен";
                ResultText.Foreground = Brushes.Orange;
                ScoreChangeText.Text = "0 очков";
                CorrectAnswerText.Text = $"Правильный ответ: {currentQuestion.Answer ?? "N/A"}";
                ResultPanel.Visibility = Visibility.Visible;

                using (var context = new GameDbContext())
                {
                    var question = context.Questions.FirstOrDefault(q => q.QuestionId == currentQuestion.QuestionId);
                    if (question != null)
                    {
                        question.IsPlayed = true;
                        context.SaveChanges();
                    }
                }

                TextAnswerSubmitButton.Visibility = Visibility.Collapsed;
                isQuestionActive = false;
                UpdateQuestionButtons();
                correctStreak = 0;
                CheckMasterPackageAchievement();
                CheckRoundCompletion();
                return;
            }

            if (empty && GameSettings.CurrentRound != 2)
            {
                MessageBox.Show("Пожалуйста, введите ответ!");
                return;
            }

            using (var context = new GameDbContext())
            {
                var question = context.Questions.FirstOrDefault(q => q.QuestionId == currentQuestion.QuestionId);
                if (question != null)
                {
                    question.IsPlayed = true;
                    context.SaveChanges();
                }
            }

            bool correct = false;
            string[] correctAnswers = new[] { currentQuestion.Answer ?? "N/A", currentQuestion.Answer2 ?? "", currentQuestion.Answer3 ?? "" }
                .Where(a => !string.IsNullOrWhiteSpace(a) && a != "N/A")
                .ToArray();

            if (!empty)
            {
                foreach (string correctAnswer in correctAnswers)
                {
                    int distance = StringUtils.LevenshteinDistance(userAnswer.ToLower(), correctAnswer.ToLower());
                    if (distance <= 2)
                    {
                        correct = true;
                        break;
                    }
                }
            }

            if (correct)
            {
                GameSettings.PlayerScore += currentQuestion.Points;
                ResultText.Text = "Правильно!";
                ResultText.Foreground = Brushes.Green;
                ScoreChangeText.Text = $"+{currentQuestion.Points} очков";
                answeredCorrectlyThisQuestion = true;
                correctStreak++;
                CheckStreakAchievements();
            }
            else
            {
                if (GameSettings.CurrentRound == 2 || GameSettings.CurrentRound == 4)
                {
                    GameSettings.PlayerScore -= currentQuestion.Points;
                    ResultText.Text = "Неправильно!";
                    ResultText.Foreground = Brushes.Red;
                    ScoreChangeText.Text = $"-{currentQuestion.Points} очков";
                }
                else
                {
                    ResultText.Text = "Неправильно!";
                    ResultText.Foreground = Brushes.Red;
                    ScoreChangeText.Text = "0 очков";
                }
                correctStreak = 0;
            }
            CorrectAnswerText.Text = $"Правильный ответ: {currentQuestion.Answer ?? "N/A"}";
            ResultPanel.Visibility = Visibility.Visible;
            TextAnswerSubmitButton.Visibility = Visibility.Collapsed;

            if (GameSettings.CurrentRound == 4)
            {
                GoToResultsButton.Visibility = Visibility.Visible;
            }

            UpdatePlayerInfo();
            isQuestionActive = false;
            UpdateQuestionButtons();
            CheckMasterPackageAchievement();
            CheckRoundCompletion();
        }

        private void CheckStreakAchievements()
        {
            if (correctStreak >= 3)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 2); // "Тройной удар"
            if (correctStreak >= 5)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 3); // "Пятерка"
            if (correctStreak >= 10)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 4); // "Десятка"
        }

        private void CheckMasterPackageAchievement()
        {
            using var ctx = new GameDbContext();
            bool allCorrect = ctx.Questions
                .Where(q => q.PackId == GameSettings.SelectedPackId && q.IsPlayed)
                .All(q => answeredCorrectlyThisQuestion); // Simplified check; assumes sequential answering
            if (allCorrect)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 1); // "Мастер пакета"
        }

        private void CheckRoundCompletion()
        {
            using (var context = new GameDbContext())
            {
                var remainingQuestions = context.Questions
                    .Where(q => q.PackId == GameSettings.SelectedPackId && q.Round == GameSettings.CurrentRound && !q.IsPlayed)
                    .ToList();

                if (!remainingQuestions.Any() && GameSettings.CurrentRound != 4)
                {
                    NextRoundButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void GoToResults_Click(object sender, RoutedEventArgs e)
        {
            EndGame();
        }

        private void NextRound_Click(object sender, RoutedEventArgs e)
        {
            NextRoundButton.Visibility = Visibility.Collapsed;
            GameSettings.CurrentRound++;
            if (GameSettings.CurrentRound <= 4)
            {
                StartRound();
            }
            else
            {
                EndGame();
            }
        }

        private void EndGame()
        {
            try
            {
                FinalScorePanel.Visibility = Visibility.Visible;
                QuestionGrid.Visibility = Visibility.Collapsed;
                RightPanel.Visibility = Visibility.Collapsed;

                double ratingChange = 0;
                if (GameSettings.GameMode == GameSettings.GameModeType.SinglePlayer)
                {
                    ratingChange = GameSettings.PlayerScore / 10.0; // Одиночная игра
                }

                string ratingChangeText = ratingChange >= 0 ? $"+{ratingChange:F1}" : $"{ratingChange:F1}";
                FinalScoreText.Text = $"Ваш итоговый счёт: {GameSettings.PlayerScore}";
                RatingChangeText.Text = $"Изменение рейтинга: {ratingChangeText}";

                using (var context = new GameDbContext())
                {
                    var player = context.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
                    if (player != null)
                    {
                        player.Score = GameSettings.PlayerScore;
                        player.Rating += ratingChange;
                        context.SaveChanges();

                        // Check rating achievements
                        if (player.Rating > 2000)
                            AchievementService.AwardAchievement(GameSettings.PlayerId, 8); // "Рейтинг 2000+"
                        if (player.Rating > 3000)
                            AchievementService.AwardAchievement(GameSettings.PlayerId, 9); // "Рейтинг 3000+"
                        if (player.Rating > 5000)
                            AchievementService.AwardAchievement(GameSettings.PlayerId, 10); // "Рейтинг 5000+"
                    }

                    var playedPack = new PlayedPack
                    {
                        PlayerId = GameSettings.PlayerId,
                        PackId = GameSettings.SelectedPackId,
                        PlayedDate = DateTime.Now
                    };
                    context.PlayedPacks.Add(playedPack);

                    var session = new GameSession
                    {
                        HostId = GameSettings.PlayerId.ToString(),
                        StartTime = DateTime.Now,
                        Status = "completed",
                        LastUpdated = DateTime.Now,
                        PackId = GameSettings.SelectedPackId
                    };
                    context.GameSessions.Add(session);
                    context.SaveChanges();

                    var sessionPlayer = new SessionPlayer
                    {
                        SessionId = session.SessionId,
                        PlayerId = GameSettings.PlayerId.ToString(),
                        Score = GameSettings.PlayerScore,
                        LastUpdated = DateTime.Now
                    };
                    context.SessionPlayers.Add(sessionPlayer);
                    context.SaveChanges();

                    var questions = context.Questions
                        .Where(q => q.PackId == GameSettings.SelectedPackId)
                        .ToList();
                    questions.ForEach(q => q.IsPlayed = false);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при завершении игры: {ex.Message}", "Ошибка");
            }
        }

        private void UpdatePlayerInfo()
        {
            PlayerInfo.Text = $"Игрок: {GameSettings.PlayerUsername}, Очки: {GameSettings.PlayerScore}";
        }

        private void GoToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new GameDbContext())
            {
                var player = context.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
                if (player != null)
                {
                    new MainMenuWindow(player).Show();
                    this.Close();
                }
            }
        }

        private void EndGame_Click(object sender, RoutedEventArgs e)
        {
            EndGame();
        }
    }
}