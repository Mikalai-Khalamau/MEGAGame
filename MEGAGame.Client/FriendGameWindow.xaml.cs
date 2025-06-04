using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace MEGAGame.Client
{
    public enum GameState
    {
        SelectingQuestion,
        WaitingForBuzzer,
        Player1Answering,
        Player2Answering,
        ShowingResult,
        RoundEnded,
        WaitingForBotAnswer
    }

    public partial class FriendGameWindow : Window
    {
        private string player1Name = "Игрок 1";
        private string player2Name = "Игрок 2";
        private int player1Score;
        private int player2Score;

        private GameState currentState = GameState.SelectingQuestion;

        private DispatcherTimer answerTimer = new DispatcherTimer();
        private int timeLeft;

        private int currentPlayer;
        private bool isSecondChance;

        private bool player1AnsweredCorrectly = false;
        private bool player2AnsweredCorrectly = false;

        private int penaltyP1, penaltyP2;

        private List<Theme> currentThemes;
        private List<Question> currentQuestions;
        private Question currentQuestion;

        private bool hasPlayer1Buzzed, hasPlayer2Buzzed;

        private int player1CorrectStreak = 0; // Streak for Player 1
        private int player2CorrectStreak = 0; // Streak for Player 2

        public FriendGameWindow()
        {
            InitializeComponent();
            ShowNicknameInput();
            ResetQuestionsPlayedState();
            StartRound();
            UpdatePlayerScores();
            KeyDown += FriendGameWindow_KeyDown;
            answerTimer.Interval = TimeSpan.FromSeconds(1);
            AchievementService.InitializeAchievements(); // Initialize achievements
        }

        #region ввод ников и сброс

        private void ShowNicknameInput()
        {
            var w = new Window
            {
                Width = 300,
                Height = 200,
                Title = "Введите ники",
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var p = new StackPanel { Margin = new Thickness(10) };
            var t1 = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var t2 = new TextBox { Margin = new Thickness(0, 0, 0, 10) };

            p.Children.Add(new TextBlock { Text = "Ник первого игрока:" });
            p.Children.Add(t1);
            p.Children.Add(new TextBlock { Text = "Ник второго игрока:" });
            p.Children.Add(t2);

            var btn = new Button { Content = "Начать игру" };
            btn.Click += (_, __) =>
            {
                player1Name = string.IsNullOrWhiteSpace(t1.Text) ? "Игрок 1" : t1.Text.Trim();
                player2Name = string.IsNullOrWhiteSpace(t2.Text) ? "Игрок 2" : t2.Text.Trim();
                w.Close();
            };
            p.Children.Add(btn);

            w.Content = p;
            w.ShowDialog();
        }

        private void ResetQuestionsPlayedState()
        {
            using var ctx = new GameDbContext();
            var qs = ctx.Questions.Where(q => q.PackId == GameSettings.SelectedPackId).ToList();
            qs.ForEach(q => q.IsPlayed = false);
            ctx.SaveChanges();
        }

        #endregion

        #region запуск раунда

        private void StartRound()
        {
            QuestionGrid.Visibility = Visibility.Visible;
            RightPanel.Visibility = Visibility.Visible;
            FinalScorePanel.Visibility = Visibility.Collapsed;

            using var ctx = new GameDbContext();
            currentQuestions = ctx.Questions
                                  .Include(q => q.Theme)
                                  .Where(q => q.PackId == GameSettings.SelectedPackId &&
                                              q.Round == GameSettings.CurrentRound)
                                  .ToList();
            currentThemes = currentQuestions.Select(q => q.Theme).Distinct().ToList();

            if (!currentQuestions.Any())
            {
                MessageBox.Show($"Нет вопросов для раунда {GameSettings.CurrentRound}");
                EndGame();
                return;
            }

            CreateQuestionGrid();
            RoundInfo.Text = $"Раунд {GameSettings.CurrentRound}: {GetRoundName(GameSettings.CurrentRound)}";

            QuestionText.Visibility = Visibility.Collapsed;
            OptionsPanel.Visibility = Visibility.Collapsed;
            TextAnswerPanel.Visibility = Visibility.Collapsed;
            ResultPanel.Visibility = Visibility.Collapsed;

            NextRoundButton.Visibility = Visibility.Collapsed;
            ShowResultsButton.Visibility = Visibility.Collapsed;

            ResetTimer();
            currentState = GameState.SelectingQuestion;
            NavigationMessage.Text = "Выберите вопрос.";
            player1AnsweredCorrectly = false;
            player2AnsweredCorrectly = false;
        }

        private static string GetRoundName(int r) => r switch
        {
            1 => "Викторина",
            2 => "Своя игра",
            3 => "Что? Где? Когда?",
            _ => "—"
        };

        private void CreateQuestionGrid()
        {
            QuestionGrid.Children.Clear();
            QuestionGrid.RowDefinitions.Clear();
            QuestionGrid.ColumnDefinitions.Clear();

            int themeCnt = 5;
            int questionCnt = GameSettings.CurrentRound == 3 ? 2 : 5;

            for (int i = 0; i < themeCnt; i++)
                QuestionGrid.RowDefinitions.Add(new() { Height = new GridLength(1, GridUnitType.Star) });
            for (int i = 0; i < questionCnt; i++)
                QuestionGrid.ColumnDefinitions.Add(new() { Width = new GridLength(1, GridUnitType.Star) });

            var groups = currentQuestions.GroupBy(q => q.ThemeId)
                                         .ToDictionary(g => g.Key, g => g.OrderBy(q => q.Points).ToList());

            for (int r = 0; r < themeCnt && r < currentThemes.Count; r++)
            {
                var th = currentThemes[r];
                var list = groups.ContainsKey(th.ThemeId) ? groups[th.ThemeId] : new();

                for (int c = 0; c < questionCnt && c < list.Count; c++)
                {
                    var q = list[c];
                    var b = new Button
                    {
                        Tag = q.QuestionId,
                        Background = q.IsPlayed ? Brushes.Red : Brushes.LightGreen,
                        Margin = new Thickness(2),
                        Content = new TextBlock
                        {
                            Text = $"{th.Name} ({q.Points})",
                            TextWrapping = TextWrapping.Wrap,
                            TextAlignment = TextAlignment.Center
                        }
                    };
                    b.Click += QuestionButton_Click;
                    Grid.SetRow(b, r); Grid.SetColumn(b, c);
                    QuestionGrid.Children.Add(b);
                }
            }
        }

        #endregion

        #region выбор вопроса

        private void QuestionButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentState != GameState.SelectingQuestion || sender is not Button b) return;

            int id = (int)b.Tag;
            using var ctx = new GameDbContext();
            currentQuestion = ctx.Questions.First(q => q.QuestionId == id);
            currentQuestion.IsPlayed = true;
            ctx.SaveChanges();

            b.Background = Brushes.Red; b.IsEnabled = false;

            ShowQuestion();
        }

        private void ShowQuestion()
        {
            penaltyP1 = penaltyP2 = 0;
            player1AnsweredCorrectly = false;
            player2AnsweredCorrectly = false;

            QuestionText.Text = currentQuestion.Text;
            QuestionText.Visibility = Visibility.Visible;

            bool quiz = GameSettings.CurrentRound == 1;
            OptionsPanel.Visibility = quiz ? Visibility.Visible : Visibility.Collapsed;
            TextAnswerPanel.Visibility = quiz ? Visibility.Collapsed : Visibility.Visible;

            if (quiz)
            {
                Option1Text.Text = $"1. {currentQuestion.Option1}";
                Option2Text.Text = $"2. {currentQuestion.Option2}";
                Option3Text.Text = $"3. {currentQuestion.Option3}";
                Option4Text.Text = $"4. {currentQuestion.Option4}";
                Option1Radio.IsChecked = Option2Radio.IsChecked =
                Option3Radio.IsChecked = Option4Radio.IsChecked = false;
                ConfirmAnswerButton.IsEnabled = true;
            }
            else
            {
                TextAnswerInput.Text = "";
                TextAnswerInput.IsEnabled = false;
            }

            ResultPanel.Visibility = Visibility.Collapsed;

            currentState = GameState.WaitingForBuzzer;
            hasPlayer1Buzzed = hasPlayer2Buzzed = false;
            isSecondChance = false;

            NavigationMessage.Text = $"{player1Name} – A, {player2Name} – L";
            StartTimer(GetInitialTime());
        }

        #endregion

        #region таймер

        private void ResetTimer()
        {
            answerTimer.Stop();
            answerTimer.Tick -= TimerTick;
            answerTimer.Tick += TimerTick;
            timeLeft = 0;
            TimerText.Text = "Время: -";
        }

        private void StartTimer(int sec)
        {
            answerTimer.Stop();
            timeLeft = sec;
            TimerText.Text = $"Время: {timeLeft}";
            answerTimer.Start();
        }

        private void TimerTick(object? _, EventArgs __)
        {
            if (--timeLeft >= 0)
            {
                TimerText.Text = $"Время: {timeLeft}";
                return;
            }

            answerTimer.Stop();

            if (currentState == GameState.WaitingForBuzzer)
            {
                if (!isSecondChance)
                {
                    isSecondChance = true;
                    currentState = GameState.WaitingForBuzzer;
                    NavigationMessage.Text = $"Второй шанс! {player1Name} – A, {player2Name} – L";
                    StartTimer(GetSecondChanceTime());
                }
                else
                {
                    FinishQuestion("Время вышло!", false, true, true);
                }
                return;
            }

            FinishQuestion("Время вышло!", false, true, false);
        }

        private int GetInitialTime() => GameSettings.CurrentRound switch
        {
            1 => 10,
            2 => 20,
            3 => 30,
            _ => 10
        };

        private int GetSecondChanceTime() => GameSettings.CurrentRound switch
        {
            1 => 5,
            2 => 10,
            3 => 15,
            _ => 5
        };

        #endregion

        #region нажатия A/L

        private void FriendGameWindow_KeyDown(object _, KeyEventArgs e)
        {
            if (currentState != GameState.WaitingForBuzzer) return;

            if (e.Key == Key.A && !hasPlayer1Buzzed)
                BeginAnswer(1);
            else if (e.Key == Key.L && !hasPlayer2Buzzed)
                BeginAnswer(2);
        }

        private async void BeginAnswer(int player)
        {
            currentPlayer = player;
            hasPlayer1Buzzed = player == 1 || hasPlayer1Buzzed;
            hasPlayer2Buzzed = player == 2 || hasPlayer2Buzzed;

            currentState = player == 1 ? GameState.Player1Answering : GameState.Player2Answering;

            ResetTimer();
            int answerTime = isSecondChance ? GetSecondChanceTime() : GetInitialTime();
            StartTimer(answerTime);

            if (GameSettings.CurrentRound != 1)
            {
                TextAnswerInput.IsEnabled = true;
                TextAnswerInput.Focus();
                await Task.Delay(50);
                TextAnswerInput.Text = "";
            }
            else
            {
                ConfirmAnswerButton.IsEnabled = true;
            }

            NavigationMessage.Text = $"У {(player == 1 ? player1Name : player2Name)} {timeLeft} сек.";
        }

        #endregion

        #region подтверждение – раунд 1

        private void ConfirmAnswer_Click(object? sender, RoutedEventArgs e)
        {
            if (currentState is not (GameState.Player1Answering or GameState.Player2Answering)) return;
            answerTimer.Stop();

            int sel = Option1Radio.IsChecked == true ? 1 :
                      Option2Radio.IsChecked == true ? 2 :
                      Option3Radio.IsChecked == true ? 3 :
                      Option4Radio.IsChecked == true ? 4 : 0;

            if (sel == 0)
            {
                NavigationMessage.Text = "Нужно выбрать вариант!";
                StartTimer(timeLeft);
                return;
            }

            bool correct = sel == currentQuestion.CorrectOption;
            if (currentPlayer == 1)
            {
                player1AnsweredCorrectly = correct;
                if (correct)
                {
                    player1CorrectStreak++;
                    CheckStreakAchievements(1);
                }
                else
                {
                    player1CorrectStreak = 0;
                }
            }
            else
            {
                player2AnsweredCorrectly = correct;
                if (correct)
                {
                    player2CorrectStreak++;
                    CheckStreakAchievements(2);
                }
                else
                {
                    player2CorrectStreak = 0;
                }
            }
            FinishQuestion(correct ? "Правильно!" : "Неправильно!", correct, false, false);
        }

        #endregion

        #region ввод текста – раунды 2-3

        private void SubmitTextAnswer_Click(object? sender, RoutedEventArgs e)
        {
            if (currentState is not (GameState.Player1Answering or GameState.Player2Answering)) return;
            answerTimer.Stop();

            string ans = TextAnswerInput.Text.Trim();
            bool empty = string.IsNullOrEmpty(ans);

            bool correct = !empty &&
                           ans.Equals(currentQuestion.Answer?.Trim(),
                                      StringComparison.OrdinalIgnoreCase);

            if (currentPlayer == 1)
            {
                player1AnsweredCorrectly = correct;
                if (correct)
                {
                    player1CorrectStreak++;
                    CheckStreakAchievements(1);
                }
                else
                {
                    player1CorrectStreak = 0;
                }
            }
            else
            {
                player2AnsweredCorrectly = correct;
                if (correct)
                {
                    player2CorrectStreak++;
                    CheckStreakAchievements(2);
                }
                else
                {
                    player2CorrectStreak = 0;
                }
            }
            FinishQuestion(correct ? "Правильно!" : (empty ? "Время вышло!" : "Неправильно!"),
                           correct, empty, false);
        }

        #endregion

        #region расчёт очков и завершение вопроса

        private void FinishQuestion(string message, bool isCorrect, bool isEmptyInput, bool noPlayersActed)
        {
            int pts = currentQuestion.Points;

            penaltyP1 = penaltyP2 = 0;

            if (GameSettings.CurrentRound == 2 && !noPlayersActed)
            {
                if (currentPlayer == 1)
                {
                    player1AnsweredCorrectly = isCorrect;
                    if (!isCorrect)
                        penaltyP1 = -pts;
                }
                else if (currentPlayer == 2)
                {
                    player2AnsweredCorrectly = isCorrect;
                    if (!isCorrect)
                        penaltyP2 = -pts;
                }

                if (isSecondChance)
                {
                    if (!player1AnsweredCorrectly && !player2AnsweredCorrectly)
                    {
                        penaltyP1 = -pts;
                        penaltyP2 = -pts;
                    }
                    else if (player1AnsweredCorrectly && !player2AnsweredCorrectly)
                    {
                        penaltyP2 = -pts;
                    }
                    else if (!player1AnsweredCorrectly && player2AnsweredCorrectly)
                    {
                        penaltyP1 = -pts;
                    }
                }
            }

            if (!isCorrect && !isSecondChance && !noPlayersActed)
            {
                isSecondChance = true;
                hasPlayer1Buzzed = currentPlayer == 1;
                hasPlayer2Buzzed = currentPlayer == 2;
                currentState = GameState.WaitingForBuzzer;
                NavigationMessage.Text = $"Второй шанс! {(currentPlayer == 1 ? player2Name : player1Name)} – {(currentPlayer == 1 ? "L" : "A")}";
                StartTimer(GetSecondChanceTime());
                return;
            }

            int bonusP1 = 0, bonusP2 = 0;
            if (isCorrect)
            {
                if (currentPlayer == 1)
                    bonusP1 = pts;
                else if (currentPlayer == 2)
                    bonusP2 = pts;
            }

            int deltaP1 = bonusP1 + penaltyP1;
            int deltaP2 = bonusP2 + penaltyP2;
            ApplyAnswerResult(deltaP1, deltaP2, message);

            if (GameSettings.CurrentRound != 1)
                OptionsPanel.Visibility = Visibility.Collapsed;

            CheckMasterPackageAchievement();
            CheckRoundCompletion();
        }

        private void ApplyAnswerResult(int deltaP1, int deltaP2, string message)
        {
            player1Score += deltaP1;
            player2Score += deltaP2;
            UpdatePlayerScores();

            ResultText.Text = message;
            ResultText.Foreground = message == "Правильно!" ? Brushes.Green :
                                   message == "Неправильно!" ? Brushes.Red :
                                   Brushes.Orange;

            QuestionText.Visibility = Visibility.Visible;

            CorrectAnswerText.Text = GameSettings.CurrentRound == 1
                                     ? $"Правильный вариант: {currentQuestion.CorrectOption}"
                                     : $"Правильный ответ: {currentQuestion.Answer}";
            CorrectAnswerText.Visibility = Visibility.Visible;

            string sign(int d) => d switch { > 0 => $"+{d}", < 0 => d.ToString(), _ => "0" };
            ScoreChangeText.Text = $"{player1Name}: {sign(deltaP1)}   {player2Name}: {sign(deltaP2)}";
            ScoreChangeText.Visibility = Visibility.Visible;

            ResultPanel.Visibility = Visibility.Visible;
            currentState = GameState.SelectingQuestion;
            NavigationMessage.Text = "Выберите следующий вопрос.";
            player1AnsweredCorrectly = false;
            player2AnsweredCorrectly = false;
        }

        private void CheckStreakAchievements(int player)
        {
            int streak = player == 1 ? player1CorrectStreak : player2CorrectStreak;
            if (streak >= 3)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 2); // "Тройной удар"
            if (streak >= 5)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 3); // "Пятерка"
            if (streak >= 10)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 4); // "Десятка"
        }

        private void CheckMasterPackageAchievement()
        {
            using var ctx = new GameDbContext();
            bool allCorrect = !ctx.Questions
                .Where(q => q.PackId == GameSettings.SelectedPackId)
                .Any(q => q.IsPlayed && (!player1AnsweredCorrectly && !player2AnsweredCorrectly));
            if (allCorrect)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 1); // "Мастер пакета"
        }

        #endregion

        #region завершение раунда / игры

        private void UpdatePlayerScores()
        {
            Player1ScoreText.Text = $"{player1Name}: {player1Score}";
            Player2ScoreText.Text = $"{player2Name}: {player2Score}";
        }

        private void CheckRoundCompletion()
        {
            bool anyLeft;
            using (var ctx = new GameDbContext())
            {
                anyLeft = ctx.Questions
                             .Any(q => q.PackId == GameSettings.SelectedPackId &&
                                       q.Round == GameSettings.CurrentRound &&
                                       !q.IsPlayed);
            }

            if (anyLeft) return;

            currentState = GameState.RoundEnded;
            NavigationMessage.Text = "Раунд завершён!";

            if (GameSettings.CurrentRound < 3)
                NextRoundButton.Visibility = Visibility.Visible;
            else
                ShowResultsButton.Visibility = Visibility.Visible;
        }

        private void NextRound_Click(object? _, RoutedEventArgs e)
        {
            GameSettings.CurrentRound++;
            StartRound();
        }

        private void ShowResults_Click(object? _, RoutedEventArgs e) => EndGame();
        private void EndGame_Click(object? _, RoutedEventArgs e) => EndGame();

        private void EndGame()
        {
            QuestionGrid.Visibility = Visibility.Collapsed;
            RightPanel.Visibility = Visibility.Collapsed;
            ResetTimer();

            string winner = player1Score > player2Score ? player1Name :
                            player2Score > player1Score ? player2Name : "Ничья";

            FinalScoreText.Text = $"{player1Name}: {player1Score}\n" +
                                  $"{player2Name}: {player2Score}\n\n" +
                                  $"Победитель: {winner}";
            FinalScorePanel.Visibility = Visibility.Visible;

            using var ctx = new GameDbContext();
            ctx.PlayedPacks.Add(new PlayedPack
            {
                PlayerId = GameSettings.PlayerId,
                PackId = GameSettings.SelectedPackId,
                PlayedDate = DateTime.Now
            });
            ctx.SaveChanges();
        }

        private void GoToMainMenu_Click(object? _, RoutedEventArgs e)
        {
            using var ctx = new GameDbContext();
            var pl = ctx.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
            if (pl == null) return;
            new MainMenuWindow(pl).Show();
            Close();
        }

        #endregion
    }
}