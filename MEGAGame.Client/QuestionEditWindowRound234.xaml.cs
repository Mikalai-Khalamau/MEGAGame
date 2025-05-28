using System;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class QuestionEditWindowRound234 : MetroWindow
    {
        private readonly QuestionPack selectedPack;
        private readonly Theme selectedTheme;
        private Question question;

        public QuestionEditWindowRound234(QuestionPack pack, Theme theme)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            selectedPack = pack;
            selectedTheme = theme;
            ThemeTextBlock.Text = $"Тема: {selectedTheme.Name}";
            RoundTextBlock.Text = $"Раунд: {GetRoundName(selectedTheme.Round)}";
        }

        public QuestionEditWindowRound234(QuestionPack pack, Question existingQuestion)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            selectedPack = pack;
            question = existingQuestion;
            ThemeTextBlock.Text = $"Тема: {question.Theme.Name}";
            RoundTextBlock.Text = $"Раунд: {GetRoundName(question.Round)}";
            QuestionTextBox.Text = question.Text;
            PointsTextBox.Text = question.Points.ToString();
            AnswerTextBox.Text = string.IsNullOrEmpty(question.Answer) || question.Answer == "N/A" ? "" : question.Answer;
        }

        private string GetRoundName(int round)
        {
            return round switch
            {
                2 => "Своя игра",
                3 => "Что? Где? Когда?",
                4 => "Ставки",
                _ => "Неизвестный раунд"
            };
        }

        private void SaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(QuestionTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PointsTextBox.Text) ||
                    string.IsNullOrWhiteSpace(AnswerTextBox.Text))
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка");
                    return;
                }

                if (!int.TryParse(PointsTextBox.Text, out int points) || points <= 0)
                {
                    MessageBox.Show("Номинал должен быть положительным числом.", "Ошибка");
                    return;
                }

                using (var context = new GameDbContext())
                {
                    var newQuestion = question ?? new Question
                    {
                        PackId = selectedPack.PackId,
                        ThemeId = selectedTheme?.ThemeId ?? question.ThemeId,
                        CreatedBy = GameSettings.PlayerId,
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsActive = true,
                        IsPlayed = false
                    };

                    newQuestion.Text = QuestionTextBox.Text;
                    newQuestion.Points = points;
                    newQuestion.Round = selectedTheme?.Round ?? question.Round;
                    newQuestion.Option1 = "N/A";
                    newQuestion.Option2 = "N/A";
                    newQuestion.Option3 = "N/A";
                    newQuestion.Option4 = "N/A";
                    newQuestion.CorrectOption = null;
                    newQuestion.Answer = AnswerTextBox.Text;

                    if (question == null)
                    {
                        context.Questions.Add(newQuestion);
                    }
                    context.SaveChanges();
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении вопроса: {ex.Message}\nПодробности: {ex.StackTrace}", "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}