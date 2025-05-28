using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;
using Microsoft.VisualBasic;

namespace MEGAGame.Client
{
    public partial class MainWindow : Window
    {
        private List<Theme> currentThemes;
        private List<Question> currentQuestions;
        private Question currentQuestion;
        private int selectedOption;
        private int playerBet;
        private bool isQuestionActive;

        public MainWindow()
        {
            InitializeComponent();
            StartRound();
            UpdatePlayerInfo();
            isQuestionActive = false;
        }

        private void StartRound()
        {
            using (var context = new GameDbContext())
            {
                currentQuestions = context.Questions
                    .Where(q => q.PackId == GameSettings.SelectedPackId && q.Round == GameSettings.CurrentRound)
                    .ToList();

                currentThemes = currentQuestions
                    .Select(q => q.Theme)
                    .Distinct()
                    .ToList();

                if (!currentQuestions.Any())
                {
                    MessageBox.Show($"Вопросов для раунда {GameSettings.CurrentRound} в выбранном пакете не найдены!");
                    EndGame();
                    return;
                }

                CreateQuestionGrid();
                RoundInfo.Text = $"Раунд {GameSettings.CurrentRound}: {GetRoundName(GameSettings.CurrentRound)}";

                if (GameSettings.CurrentRound == 4)
                {
                    QuestionGridPanel.Visibility = Visibility.Collapsed;
                    BetPanel.Visibility = Visibility.Visible;
                }
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

        private void CreateQuestionGrid()
        {
            QuestionGrid.Children.Clear();
            QuestionGrid.RowDefinitions.Clear();
            QuestionGrid.ColumnDefinitions.Clear();

            const int themeCount = 5;
            const int questionCount = 5;

            for (int i = 0; i < questionCount; i++) QuestionGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int i = 0; i < themeCount; i++) QuestionGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var themeQuestions = currentQuestions
                .GroupBy(q => q.ThemeId)
                .ToDictionary(g => g.Key, g => g.OrderBy(q => q.Points).ToList());

            for (int i = 0; i < themeCount; i++)
            {
                if (i < currentThemes.Count)
                {
                    var theme = currentThemes[i];
                    var questions = themeQuestions.ContainsKey(theme.ThemeId) ? themeQuestions[theme.ThemeId] : new List<Question>();

                    for (int j = 0; j < questionCount; j++)
                    {
                        if (j < questions.Count)
                        {
                            var question = questions[j];
                            var button = new Button
                            {
                                Content = $"{theme.Name} ({question.Points} очков)",
                                Tag = question.QuestionId,
                                Background = question.IsPlayed ? Brushes.Red : Brushes.LightGreen,
                                IsEnabled = !question.IsPlayed && !isQuestionActive,
                                Margin = new Thickness(5),
                                Padding = new Thickness(10),
                                FontSize = 16
                            };
                            button.Click += QuestionButton_Click;
                            Grid.SetRow(button, j);
                            Grid.SetColumn(button, i);
                            QuestionGrid.Children.Add(button);
                        }
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
                            button.IsEnabled = !question.IsPlayed && !isQuestionActive;
                        }
                    }
                }
            }
        }

        private void SubmitBet_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(BetInput.Text, out playerBet) || playerBet <= 0 || playerBet > GameSettings.PlayerScore)
            {
                MessageBox.Show($"Пожалуйста, введите корректную ставку (от 1 до {GameSettings.PlayerScore})!");
                return;
            }

            BetPanel.Visibility = Visibility.Collapsed;
            QuestionDisplayPanel.Visibility = Visibility.Visible;

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
                QuestionDisplayPanel.Visibility = Visibility.Visible;
                QuestionText.Text = "";
                ResultPanel.Visibility = Visibility.Collapsed;
                QuestionText.Text = $"{currentQuestion.Text} ({currentQuestion.Points} очков)";

                if (GameSettings.CurrentRound == 1)
                {
                    OptionsPanel.Visibility = Visibility.Visible;
                    TextAnswerPanel.Visibility = Visibility.Collapsed;
                    SubmitOptionsButton.Visibility = Visibility.Visible;

                    Option1.Content = currentQuestion.Option1;
                    Option2.Content = currentQuestion.Option2;
                    Option3.Content = currentQuestion.Option3;
                    Option4.Content = currentQuestion.Option4;
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
                MessageBox.Show("Пожалуйста, выберите вариант ответа!");
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
            }
            else
            {
                ResultText.Text = "Неправильно!";
                ResultText.Foreground = Brushes.Red;
                ScoreChangeText.Text = "0 очков";
            }
            CorrectAnswerText.Text = $"Правильный ответ: {currentQuestion.Option1}";
            ResultPanel.Visibility = Visibility.Visible;
            SubmitOptionsButton.Visibility = Visibility.Collapsed;
            UpdatePlayerInfo();
            isQuestionActive = false;
            UpdateQuestionButtons();
            CheckRoundCompletion();
        }

        private void SubmitTextAnswer_Click(object sender, RoutedEventArgs e)
        {
            string userAnswer = TextAnswerInput.Text.Trim().ToLower();

            if (GameSettings.CurrentRound == 2 && string.IsNullOrWhiteSpace(userAnswer))
            {
                ResultText.Text = "Ответ пропущен";
                ResultText.Foreground = Brushes.Orange;
                ScoreChangeText.Text = "0 очков";
                CorrectAnswerText.Text = $"Правильный ответ: {currentQuestion.Answer}";
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
                CheckRoundCompletion();
                return;
            }

            if (string.IsNullOrWhiteSpace(userAnswer) && GameSettings.CurrentRound != 2)
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

            string correctAnswer = currentQuestion.Answer?.Trim().ToLower();
            if (userAnswer == correctAnswer)
            {
                GameSettings.PlayerScore += currentQuestion.Points;
                ResultText.Text = "Правильно!";
                ResultText.Foreground = Brushes.Green;
                ScoreChangeText.Text = $"+{currentQuestion.Points} очков";
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
            }
            CorrectAnswerText.Text = $"Правильный ответ: {currentQuestion.Answer}";
            ResultPanel.Visibility = Visibility.Visible;
            TextAnswerSubmitButton.Visibility = Visibility.Collapsed;
            UpdatePlayerInfo();
            isQuestionActive = false;
            UpdateQuestionButtons();
            CheckRoundCompletion();
        }

        private void CheckRoundCompletion()
        {
            using (var context = new GameDbContext())
            {
                var remainingQuestions = context.Questions
                    .Where(q => q.PackId == GameSettings.SelectedPackId && q.Round == GameSettings.CurrentRound && !q.IsPlayed)
                    .ToList();

                if (!remainingQuestions.Any())
                {
                    if (GameSettings.CurrentRound == 4)
                    {
                        EndGame();
                    }
                    else
                    {
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
                }
                else
                {
                    QuestionGridPanel.Visibility = Visibility.Visible;
                    CreateQuestionGrid();
                }
            }
        }

        private void EndGame()
        {
            GameArea.Visibility = Visibility.Collapsed;
            QuestionDisplayPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Collapsed;
            FinalScorePanel.Visibility = Visibility.Visible;
            FinalScoreText.Text = $"Ваш итоговый счёт: {GameSettings.PlayerScore}";

            using (var context = new GameDbContext())
            {
                var player = context.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
                if (player != null)
                {
                    player.Score = GameSettings.PlayerScore;
                    player.Rating += GameSettings.PlayerScore / 10;
                    context.SaveChanges();
                }

                var session = new GameSession
                {
                    HostId = GameSettings.PlayerId,
                    StartTime = DateTime.Now,
                    Status = "completed",
                    LastUpdated = DateTime.Now
                };
                context.GameSessions.Add(session);
                context.SaveChanges();

                var sessionPlayer = new SessionPlayer
                {
                    SessionId = session.SessionId,
                    PlayerId = GameSettings.PlayerId,
                    Score = GameSettings.PlayerScore,
                    LastUpdated = DateTime.Now
                };
                context.SessionPlayers.Add(sessionPlayer);
                context.SaveChanges();
            }

            using (var context = new GameDbContext())
            {
                var questions = context.Questions
                    .Where(q => q.PackId == GameSettings.SelectedPackId)
                    .ToList();
                questions.ForEach(q => q.IsPlayed = false);
                context.SaveChanges();
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

        private void ExitToMainMenu_Click(object sender, RoutedEventArgs e)
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
    }
}