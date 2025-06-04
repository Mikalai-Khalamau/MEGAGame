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
    public partial class BotGameWindow : Window
    {
        private string player1Name = "Вы";
        private string player2Name = "Бот";
        private int player1Score = 0;
        private int player2Score = 0;

        private GameState currentState = GameState.SelectingQuestion;

        private DispatcherTimer answerTimer = new DispatcherTimer();
        private DispatcherTimer botTimer = new DispatcherTimer();
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

        private int questionNumber = 0;
        private bool botWillAnswerCorrectly;
        private bool botWillAnswer;

        private int playerBet;
        private int botBet;

        private int correctStreak = 0; // Added for achievement tracking

        public BotGameWindow()
        {
            InitializeComponent();
            ResetQuestionsPlayedState();
            StartRound();
            UpdatePlayerScores();
            KeyDown += BotGameWindow_KeyDown;
            answerTimer.Interval = TimeSpan.FromSeconds(1);
            AchievementService.InitializeAchievements(); // Initialize achievements
        }

        private void ResetQuestionsPlayedState()
        {
            using var ctx = new GameDbContext();
            var qs = ctx.Questions.Where(q => q.PackId == GameSettings.SelectedPackId).ToList();
            qs.ForEach(q => q.IsPlayed = false);
            ctx.SaveChanges();
        }

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
            BetPanel.Visibility = GameSettings.CurrentRound == 4 ? Visibility.Visible : Visibility.Collapsed;

            NextRoundButton.Visibility = Visibility.Collapsed;
            ShowResultsButton.Visibility = Visibility.Collapsed;

            ResetTimer();
            currentState = GameState.SelectingQuestion;
            NavigationMessage.Text = "Выберите вопрос.";
            player1AnsweredCorrectly = false;
            player2AnsweredCorrectly = false;

            if (GameSettings.CurrentRound == 4)
            {
                BetInput.Text = "";
            }
        }

        private static string GetRoundName(int r) => r switch
        {
            1 => "Викторина",
            2 => "Своя игра",
            3 => "Что? Где? Когда?",
            4 => "Ставки",
            _ => "—"
        };

        private void CreateQuestionGrid()
        {
            QuestionGrid.Children.Clear();
            QuestionGrid.RowDefinitions.Clear();
            QuestionGrid.ColumnDefinitions.Clear();

            int themeCnt = 5;
            int questionCnt = GameSettings.CurrentRound == 3 ? 2 : GameSettings.CurrentRound == 4 ? 1 : 5;

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
                            Text = GameSettings.CurrentRound == 4 ? $"Тема: {th.Name}" : $"{th.Name} ({q.Points})",
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
            questionNumber++;
            penaltyP1 = penaltyP2 = 0;
            player1AnsweredCorrectly = player2AnsweredCorrectly = false;

            DetermineBotAnswer();

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
            BotAnswerText.Visibility = Visibility.Collapsed;

            currentState = GameState.WaitingForBuzzer;
            hasPlayer1Buzzed = hasPlayer2Buzzed = false;
            isSecondChance = false;

            NavigationMessage.Text = "Нажмите пробел для ответа.";
            StartTimer(GetInitialTime());

            double botDelay = botWillAnswer ? GetBotDelay() : 0;
            if (botWillAnswer)
            {
                botTimer.Interval = TimeSpan.FromSeconds(botDelay);
                botTimer.Tick += BotTimer_Tick;
                botTimer.Start();
            }
        }

        private void DetermineBotAnswer()
        {
            botWillAnswerCorrectly = true;
            botWillAnswer = true;

            if (questionNumber % 6 == 0)
            {
                botWillAnswer = false;
            }
            else if (questionNumber % 5 == 0)
            {
                switch (GameSettings.BotDifficulty)
                {
                    case GameSettings.BotDifficultyLevel.Easy:
                        botWillAnswer = false;
                        break;
                    case GameSettings.BotDifficultyLevel.Medium:
                        botWillAnswerCorrectly = true;
                        break;
                    case GameSettings.BotDifficultyLevel.Hard:
                        botWillAnswerCorrectly = true;
                        break;
                }
            }
            else if (questionNumber % 4 == 0)
            {
                switch (GameSettings.BotDifficulty)
                {
                    case GameSettings.BotDifficultyLevel.Easy:
                        botWillAnswerCorrectly = false;
                        break;
                    case GameSettings.BotDifficultyLevel.Medium:
                        botWillAnswer = false;
                        break;
                    case GameSettings.BotDifficultyLevel.Hard:
                        botWillAnswerCorrectly = true;
                        break;
                }
            }
            else if (questionNumber % 3 == 0)
            {
                botWillAnswerCorrectly = false;
            }
            else if (questionNumber % 2 == 0)
            {
                botWillAnswer = false;
            }
        }

        private double GetBotDelay()
        {
            Random rand = new Random();
            int baseMin = 0, baseMax = 0;

            switch (GameSettings.BotDifficulty)
            {
                case GameSettings.BotDifficultyLevel.Easy:
                    baseMin = 8; baseMax = 10;
                    break;
                case GameSettings.BotDifficultyLevel.Medium:
                    baseMin = 6; baseMax = 8;
                    break;
                case GameSettings.BotDifficultyLevel.Hard:
                    baseMin = 4; baseMax = 6;
                    break;
            }

            int multiplier = GameSettings.CurrentRound switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                4 => 0,
                _ => 1
            };

            if (multiplier == 0) return 0;
            return rand.Next(baseMin * multiplier, baseMax * multiplier + 1);
        }

        private void BotTimer_Tick(object sender, EventArgs e)
        {
            botTimer.Stop();
            if (currentState == GameState.WaitingForBuzzer && !hasPlayer1Buzzed)
            {
                BeginAnswer(2);
            }
        }

        private void ResetTimer()
        {
            answerTimer.Stop();
            botTimer.Stop();
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
                    NavigationMessage.Text = "Второй шанс! Нажмите пробел.";
                    StartTimer(GetSecondChanceTime());
                    double botDelay = botWillAnswer ? GetBotDelay() : 0;
                    if (botWillAnswer)
                    {
                        botTimer.Interval = TimeSpan.FromSeconds(botDelay);
                        botTimer.Tick += BotTimer_Tick;
                        botTimer.Start();
                    }
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
            4 => 20,
            _ => 10
        };

        private int GetSecondChanceTime() => GameSettings.CurrentRound switch
        {
            1 => 5,
            2 => 10,
            3 => 15,
            4 => 10,
            _ => 5
        };

        private void BotGameWindow_KeyDown(object _, KeyEventArgs e)
        {
            if (currentState != GameState.WaitingForBuzzer) return;

            if (e.Key == Key.Space && !hasPlayer1Buzzed)
            {
                botTimer.Stop();
                BeginAnswer(1);
            }
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

            if (player == 1)
            {
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
                NavigationMessage.Text = $"У вас {timeLeft} сек.";
            }
            else
            {
                if (GameSettings.CurrentRound == 1)
                {
                    if (botWillAnswerCorrectly)
                    {
                        switch (currentQuestion.CorrectOption)
                        {
                            case 1: Option1Radio.IsChecked = true; break;
                            case 2: Option2Radio.IsChecked = true; break;
                            case 3: Option3Radio.IsChecked = true; break;
                            case 4: Option4Radio.IsChecked = true; break;
                        }
                    }
                    ConfirmAnswer_Click(null, null);
                }
                else
                {
                    TextAnswerInput.Text = botWillAnswerCorrectly ? currentQuestion.Answer : "";
                    SubmitTextAnswer_Click(null, null);
                }
            }
        }

        private void ConfirmAnswer_Click(object? sender, RoutedEventArgs e)
        {
            if (currentState is not (GameState.Player1Answering or GameState.Player2Answering)) return;
            answerTimer.Stop();

            int sel = Option1Radio.IsChecked == true ? 1 :
                      Option2Radio.IsChecked == true ? 2 :
                      Option3Radio.IsChecked == true ? 3 :
                      Option4Radio.IsChecked == true ? 4 : 0;

            if (sel == 0 && currentPlayer == 1)
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
                    correctStreak++;
                    CheckStreakAchievements();
                }
                else
                {
                    correctStreak = 0;
                }
            }
            else
                player2AnsweredCorrectly = correct;
            FinishQuestion(correct ? "Правильно!" : "Неправильно!", correct, false, false);
        }

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
                    correctStreak++;
                    CheckStreakAchievements();
                }
                else
                {
                    correctStreak = 0;
                }
            }
            else
                player2AnsweredCorrectly = correct;

            if (GameSettings.CurrentRound == 4)
            {
                HandleBettingRound(correct, empty);
            }
            else
            {
                FinishQuestion(correct ? "Правильно!" : (empty ? "Время вышло!" : "Неправильно!"),
                               correct, empty, false);
            }
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

        private void SubmitBet_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(BetInput.Text, out playerBet) || playerBet <= 0 || playerBet > player1Score)
            {
                MessageBox.Show($"Введите ставку от 1 до {player1Score}!");
                return;
            }

            Random rand = new Random();
            botBet = rand.Next(1, player2Score + 1);

            BetPanel.Visibility = Visibility.Collapsed;
            ShowQuestion();
        }

        private void HandleBettingRound(bool playerCorrect, bool playerEmpty)
        {
            bool botCorrect = GameSettings.BotDifficulty == GameSettings.BotDifficultyLevel.Hard;
            string botAnswer = botCorrect ? currentQuestion.Answer : "";

            int playerDelta = playerCorrect ? playerBet : -playerBet;
            int botDelta = botCorrect ? botBet : -botBet;

            player1Score += playerDelta;
            player2Score += botDelta;

            ResultText.Text = playerCorrect ? "Правильно!" : (playerEmpty ? "Время вышло!" : "Неправильно!");
            ResultText.Foreground = playerCorrect ? Brushes.Green : Brushes.Red;
            string sign(int d) => d switch { > 0 => $"+{d}", < 0 => d.ToString(), _ => "0" };
            ScoreChangeText.Text = $"{player1Name}: {sign(playerDelta)}   {player2Name}: {sign(botDelta)}";
            CorrectAnswerText.Text = $"Правильный ответ: {currentQuestion.Answer}";
            BotAnswerText.Text = botCorrect ? "Бот ответил верно" : "Бот ответил неверно";
            BotAnswerText.Visibility = Visibility.Visible;

            ResultPanel.Visibility = Visibility.Visible;
            ShowResultsButton.Visibility = Visibility.Visible;

            UpdatePlayerScores();
            currentState = GameState.RoundEnded;
            NavigationMessage.Text = "Раунд завершён!";
        }

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

            if (!isCorrect && currentPlayer == 1 && !isSecondChance && !noPlayersActed)
            {
                isSecondChance = true;
                hasPlayer1Buzzed = true;
                if (botWillAnswer)
                {
                    currentState = GameState.WaitingForBotAnswer;
                    NavigationMessage.Text = "Ожидание ответа бота...";
                    double botDelay = GameSettings.CurrentRound switch
                    {
                        1 => 3,
                        2 => 6,
                        3 => 9,
                        4 => 0,
                        _ => 3
                    };
                    if (botDelay > 0)
                    {
                        botTimer.Interval = TimeSpan.FromSeconds(botDelay);
                        botTimer.Tick += BotAnswerTimer_Tick;
                        botTimer.Start();
                    }
                    else
                    {
                        BotAnswerTimer_Tick(null, null);
                    }
                }
                else
                {
                    currentState = GameState.WaitingForBuzzer;
                    NavigationMessage.Text = "Бот не отвечает. Ваш второй шанс! Нажмите пробел.";
                    StartTimer(GetSecondChanceTime());
                }
                return;
            }

            if (!isCorrect && currentPlayer == 2 && !isSecondChance && !noPlayersActed)
            {
                isSecondChance = true;
                hasPlayer2Buzzed = true;
                currentState = GameState.WaitingForBuzzer;
                NavigationMessage.Text = "Второй шанс! Нажмите пробел.";
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

            CheckRoundCompletion();
        }

        private void BotAnswerTimer_Tick(object sender, EventArgs e)
        {
            botTimer.Stop();
            BeginAnswer(2);
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

            BotAnswerText.Text = currentPlayer == 2 ? (player2AnsweredCorrectly ? "Бот ответил верно" : "Бот ответил неверно") : "";
            BotAnswerText.Visibility = currentPlayer == 2 ? Visibility.Visible : Visibility.Collapsed;

            ResultPanel.Visibility = Visibility.Visible;
            currentState = GameState.SelectingQuestion;
            NavigationMessage.Text = "Выберите следующий вопрос.";
            player1AnsweredCorrectly = player2AnsweredCorrectly = false;

            // Check for "Мастер пакета" after each question
            CheckMasterPackageAchievement();
        }

        private void CheckMasterPackageAchievement()
        {
            using var ctx = new GameDbContext();
            bool allCorrect = !ctx.Questions
                .Where(q => q.PackId == GameSettings.SelectedPackId)
                .Any(q => q.IsPlayed && !player1AnsweredCorrectly);
            if (allCorrect)
                AchievementService.AwardAchievement(GameSettings.PlayerId, 1); // "Мастер пакета"
        }

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

            if (anyLeft || GameSettings.CurrentRound == 4) return;

            currentState = GameState.RoundEnded;
            NavigationMessage.Text = "Раунд завершён!";
            NextRoundButton.Visibility = GameSettings.CurrentRound < 4 ? Visibility.Visible : Visibility.Collapsed;
            ShowResultsButton.Visibility = GameSettings.CurrentRound == 4 ? Visibility.Visible : Visibility.Collapsed;
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

            double ratingChange = 0;
            if (GameSettings.GameMode == GameSettings.GameModeType.Bot)
            {
                double botDifficultyMultiplier = GameSettings.BotDifficulty switch
                {
                    GameSettings.BotDifficultyLevel.Easy => 1.0,
                    GameSettings.BotDifficultyLevel.Medium => 1.5,
                    GameSettings.BotDifficultyLevel.Hard => 2.0,
                    _ => 1.0
                };
                GameSettings.PlayerScore = player1Score;
                ratingChange = (GameSettings.PlayerScore / 10.0) * botDifficultyMultiplier;
            }

            string ratingChangeText = ratingChange >= 0 ? $"+{ratingChange:F1}" : $"{ratingChange:F1}";
            RatingChangeText.Text = $"Изменение рейтинга: {ratingChangeText}";

            using var ctx = new GameDbContext();
            var player = ctx.Players.FirstOrDefault(p => p.PlayerId == GameSettings.PlayerId);
            if (player != null)
            {
                player.Score = GameSettings.PlayerScore;
                player.Rating += ratingChange;
                ctx.SaveChanges();
                Console.WriteLine($"Player {GameSettings.PlayerId} updated: Score = {player.Score}, Rating = {player.Rating}");

                // Check bot victory achievements
                if (player1Score > player2Score)
                {
                    switch (GameSettings.BotDifficulty)
                    {
                        case GameSettings.BotDifficultyLevel.Easy:
                            AchievementService.AwardAchievement(GameSettings.PlayerId, 6); // "Победитель простого бота"
                            break;
                        case GameSettings.BotDifficultyLevel.Medium:
                            AchievementService.AwardAchievement(GameSettings.PlayerId, 7); // "Победитель среднего бота"
                            break;
                        case GameSettings.BotDifficultyLevel.Hard:
                            AchievementService.AwardAchievement(GameSettings.PlayerId, 5); // "Победитель сложного бота"
                            break;
                    }
                }

                // Check rating achievements
                if (player.Rating > 2000)
                    AchievementService.AwardAchievement(GameSettings.PlayerId, 8); // "Рейтинг 2000+"
                if (player.Rating > 3000)
                    AchievementService.AwardAchievement(GameSettings.PlayerId, 9); // "Рейтинг 3000+"
                if (player.Rating > 5000)
                    AchievementService.AwardAchievement(GameSettings.PlayerId, 10); // "Рейтинг 5000+"
            }

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
    }
}