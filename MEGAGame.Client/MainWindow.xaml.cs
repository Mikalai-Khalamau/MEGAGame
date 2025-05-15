using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class MainWindow : Window
    {
        private List<Theme> themes;
        private List<Question> currentQuestions;
        private Question currentQuestion;
        private int selectedOption;

        public MainWindow()
        {
            InitializeComponent();
            LoadThemes();
            UpdatePlayerInfo();
        }

        private void LoadThemes()
        {
            using (var context = new GameDbContext())
            {
                themes = context.Themes.ToList();
                ThemeSelector.ItemsSource = themes;
                ThemeSelector.DisplayMemberPath = "Name";
            }
        }

        private void ThemeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeSelector.SelectedItem is Theme selectedTheme)
            {
                using (var context = new GameDbContext())
                {
                    currentQuestions = context.Questions
                        .Where(q => q.ThemeId == selectedTheme.Id)
                        .ToList();
                }

                if (currentQuestions.Any())
                {
                    currentQuestion = currentQuestions.First();
                    DisplayQuestion();
                }
            }
        }

        private void DisplayQuestion()
        {
            if (currentQuestion != null)
            {
                QuestionPanel.Visibility = Visibility.Visible;
                QuestionText.Text = currentQuestion.Text;
                Option1.Content = currentQuestion.Option1;
                Option2.Content = currentQuestion.Option2;
                Option3.Content = currentQuestion.Option3;
                Option4.Content = currentQuestion.Option4;
                Option1.IsChecked = false;
                Option2.IsChecked = false;
                Option3.IsChecked = false;
                Option4.IsChecked = false;
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

            if (selectedOption == currentQuestion.CorrectOption)
            {
                MessageBox.Show($"Правильно! Вы заработали {currentQuestion.Points} очков!");
                GameSettings.PlayerScore += currentQuestion.Points;

                using (var context = new GameDbContext())
                {
                    var player = context.Players.FirstOrDefault(p => p.Name == GameSettings.PlayerName);
                    if (player != null)
                    {
                        player.Score += currentQuestion.Points;
                        player.Rating += 10; // Увеличиваем рейтинг за правильный ответ
                        context.SaveChanges();
                    }
                }
            }
            else
            {
                MessageBox.Show("Неправильно! Попробуйте следующий вопрос.");
            }

            UpdatePlayerInfo();
            NextQuestion_Click(sender, e);
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (currentQuestions == null || !currentQuestions.Any())
            {
                MessageBox.Show("Вопросы закончились!");
                return;
            }

            var currentIndex = currentQuestions.IndexOf(currentQuestion);
            if (currentIndex + 1 < currentQuestions.Count)
            {
                currentQuestion = currentQuestions[currentIndex + 1];
                DisplayQuestion();
            }
            else
            {
                MessageBox.Show("Вы ответили на все вопросы в этой теме!");
                QuestionPanel.Visibility = Visibility.Collapsed;
                ThemeSelector.SelectedIndex = -1;
                currentQuestions = null;
                currentQuestion = null;
            }

            selectedOption = 0;
        }

        private void UpdatePlayerInfo()
        {
            PlayerInfo.Text = $"Игрок: {GameSettings.PlayerName}, Очки: {GameSettings.PlayerScore}";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}