using System;
using System.Windows;
using System.Windows.Controls;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class QuestionEditWindowRound1 : MetroWindow
    {
        private readonly QuestionPack selectedPack;
        private readonly Theme selectedTheme;
        private Question question;

        public QuestionEditWindowRound1(QuestionPack pack, Theme theme)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            selectedPack = pack;
            selectedTheme = theme;
            ThemeTextBlock.Text = $"Тема: {selectedTheme.Name}";
            RoundTextBlock.Text = $"Раунд: Викторина";
        }

        public QuestionEditWindowRound1(QuestionPack pack, Question existingQuestion)
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.ResizeMode = ResizeMode.NoResize;
            selectedPack = pack;
            question = existingQuestion;
            ThemeTextBlock.Text = $"Тема: {question.Theme.Name}";
            RoundTextBlock.Text = $"Раунд: Викторина";
            QuestionTextBox.Text = question.Text;
            PointsTextBox.Text = question.Points.ToString();
            Option1TextBox.Text = string.IsNullOrEmpty(question.Option1) || question.Option1 == "N/A" ? "" : question.Option1;
            Option2TextBox.Text = string.IsNullOrEmpty(question.Option2) || question.Option2 == "N/A" ? "" : question.Option2;
            Option3TextBox.Text = string.IsNullOrEmpty(question.Option3) || question.Option3 == "N/A" ? "" : question.Option3;
            Option4TextBox.Text = string.IsNullOrEmpty(question.Option4) || question.Option4 == "N/A" ? "" : question.Option4;
            if (question.CorrectOption.HasValue)
            {
                CorrectOptionComboBox.SelectedIndex = question.CorrectOption.Value - 1;
            }
        }

        private void SaveQuestion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(QuestionTextBox.Text) ||
                    string.IsNullOrWhiteSpace(PointsTextBox.Text) ||
                    string.IsNullOrWhiteSpace(Option1TextBox.Text) ||
                    string.IsNullOrWhiteSpace(Option2TextBox.Text) ||
                    string.IsNullOrWhiteSpace(Option3TextBox.Text) ||
                    string.IsNullOrWhiteSpace(Option4TextBox.Text) ||
                    CorrectOptionComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Все поля должны быть заполнены.", "Ошибка");
                    return;
                }

                if (!int.TryParse(PointsTextBox.Text, out int points) || points <= 0)
                {
                    MessageBox.Show("Номинал должен быть положительным числом.", "Ошибка");
                    return;
                }

                var selectedItem = CorrectOptionComboBox.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Выберите правильный вариант.", "Ошибка");
                    return;
                }
                int correctOption = int.Parse(selectedItem.Tag.ToString());

                using (var context = new GameDbContext())
                {
                    int themeId = selectedTheme != null ? selectedTheme.ThemeId : (question != null ? question.ThemeId : 0);
                    if (themeId == 0 || context.Themes.Find(themeId) == null)
                    {
                        MessageBox.Show("Выбранная тема не существует.", "Ошибка");
                        return;
                    }

                    if (context.QuestionPacks.Find(selectedPack.PackId) == null)
                    {
                        MessageBox.Show("Выбранный пакет не существует.", "Ошибка");
                        return;
                    }

                    if (context.Players.Find(GameSettings.PlayerId) == null)
                    {
                        MessageBox.Show("Игрок не найден.", "Ошибка");
                        return;
                    }

                    var existingQuestion = context.Questions
                        .FirstOrDefault(q => q.Text == QuestionTextBox.Text && q.ThemeId == themeId && q.QuestionId != (question != null ? question.QuestionId : 0));
                    if (existingQuestion != null)
                    {
                        MessageBox.Show("Вопрос с таким текстом уже существует в этой теме.", "Ошибка");
                        return;
                    }

                    var newQuestion = question ?? new Question
                    {
                        PackId = selectedPack.PackId,
                        ThemeId = themeId,
                        CreatedBy = GameSettings.PlayerId,
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now,
                        IsActive = true,
                        IsPlayed = false
                    };

                    newQuestion.Text = QuestionTextBox.Text;
                    newQuestion.Points = points;
                    newQuestion.Round = 1;
                    newQuestion.Option1 = Option1TextBox.Text;
                    newQuestion.Option2 = Option2TextBox.Text;
                    newQuestion.Option3 = Option3TextBox.Text;
                    newQuestion.Option4 = Option4TextBox.Text;
                    newQuestion.CorrectOption = correctOption;
                    newQuestion.Answer = "N/A";

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
                MessageBox.Show($"Ошибка при сохранении вопроса: {ex.Message}\nВнутреннее исключение: {ex.InnerException?.Message}", "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}