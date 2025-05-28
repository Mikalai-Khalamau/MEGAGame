using System;
using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class QuestionCreationWindow : Window
    {
        private readonly QuestionPack selectedPack;
        private readonly Theme selectedTheme;

        public QuestionCreationWindow(QuestionPack pack, Theme theme)
        {
            InitializeComponent();
            selectedPack = pack;
            selectedTheme = theme;

            // Устанавливаем предустановленные значения
            RoundComboBox.SelectedValue = selectedTheme.Round;
            ThemeComboBox.ItemsSource = new[] { selectedTheme };
            ThemeComboBox.SelectedItem = selectedTheme;
            ThemeComboBox.IsEnabled = false; // Блокируем редактирование темы
            RoundComboBox.IsEnabled = false; // Блокируем редактирование раунда
            PointsBox.Text = "100";
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PointsBox.Text, out int points) || points <= 0)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text))
            {
                return;
            }

            using (var context = new GameDbContext())
            {
                var questionsInTheme = context.Questions
                    .Where(q => q.ThemeId == selectedTheme.ThemeId && q.Round == selectedTheme.Round && q.PackId == selectedPack.PackId)
                    .ToList();

                if (selectedTheme.Round == 1 || selectedTheme.Round == 2)
                {
                    if (questionsInTheme.Count >= 5)
                    {
                        return;
                    }
                }
                else if (selectedTheme.Round == 3)
                {
                    if (questionsInTheme.Count >= 2)
                    {
                        return;
                    }
                }
                else if (selectedTheme.Round == 4)
                {
                    if (questionsInTheme.Count >= 1)
                    {
                        return;
                    }
                }

                var question = new Question
                {
                    Text = QuestionTextBox.Text,
                    Points = points,
                    Round = selectedTheme.Round,
                    ThemeId = selectedTheme.ThemeId,
                    PackId = selectedPack.PackId,
                    CreatedBy = GameSettings.PlayerId,
                    CreatedDate = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    IsActive = true,
                    IsPlayed = false
                };

                if (question.Round == 1)
                {
                    question.Option1 = string.IsNullOrWhiteSpace(Option1Box.Text) ? "N/A" : Option1Box.Text;
                    question.Option2 = string.IsNullOrWhiteSpace(Option2Box.Text) ? "N/A" : Option2Box.Text;
                    question.Option3 = string.IsNullOrWhiteSpace(Option3Box.Text) ? "N/A" : Option3Box.Text;
                    question.Option4 = string.IsNullOrWhiteSpace(Option4Box.Text) ? "N/A" : Option4Box.Text;
                    question.CorrectOption = (int)CorrectOptionComboBox.SelectedValue;
                    question.Answer = "N/A";

                    if (question.Option1 == "N/A" || question.Option2 == "N/A" ||
                        question.Option3 == "N/A" || question.Option4 == "N/A")
                    {
                        return;
                    }
                }
                else
                {
                    question.Option1 = "N/A";
                    question.Option2 = "N/A";
                    question.Option3 = "N/A";
                    question.Option4 = "N/A";
                    question.CorrectOption = null;
                    question.Answer = string.IsNullOrWhiteSpace(AnswerBox.Text) ? "N/A" : AnswerBox.Text;

                    if (question.Answer == "N/A")
                    {
                        return;
                    }
                }

                context.Questions.Add(question);
                context.SaveChanges();

                QuestionTextBox.Text = "";
                PointsBox.Text = "100";
                Option1Box.Text = "";
                Option2Box.Text = "";
                Option3Box.Text = "";
                Option4Box.Text = "";
                AnswerBox.Text = "";
                CorrectOptionComboBox.SelectedIndex = 0;
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}