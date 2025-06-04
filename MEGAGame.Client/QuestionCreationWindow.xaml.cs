using System;
using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;
using Microsoft.EntityFrameworkCore;
using MEGAGame.Core;
namespace MEGAGame.Client
{
    public partial class QuestionCreationWindow : Window
    {
        private readonly int selectedPackId;
        private readonly int selectedThemeId;
        private readonly int round;
        private readonly string themeName;

        public QuestionCreationWindow(QuestionPack pack, Theme theme)
        {
            InitializeComponent();
            selectedPackId = pack.PackId;
            selectedThemeId = theme.ThemeId;
            round = theme.Round;
            themeName = theme.Name ?? "Без названия";

            RoundComboBox.SelectedValue = round;
            ThemeComboBox.ItemsSource = new[] { new { ThemeId = theme.ThemeId, Name = theme.Name ?? "Без названия" } };
            ThemeComboBox.DisplayMemberPath = "Name";
            ThemeComboBox.SelectedIndex = 0;
            ThemeComboBox.IsEnabled = false;
            RoundComboBox.IsEnabled = false;
            PointsBox.Text = "100";
        }

        private void AddQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(PointsBox.Text, out int points) || points <= 0)
            {
                MessageBox.Show("Введите корректное количество очков (положительное число).");
                return;
            }

            if (string.IsNullOrWhiteSpace(QuestionTextBox.Text))
            {
                MessageBox.Show("Введите текст вопроса.");
                return;
            }

            using (var context = new GameDbContext())
            {
                var questionsInTheme = context.Questions
                    .AsNoTracking()
                    .Where(q => q.ThemeId == selectedThemeId && q.Round == round && q.PackId == selectedPackId)
                    .ToList();

                if (round == 1 || round == 2)
                {
                    if (questionsInTheme.Count >= 5)
                    {
                        MessageBox.Show("Достигнуто максимальное количество вопросов для этого раунда (5).");
                        return;
                    }
                }
                else if (round == 3)
                {
                    if (questionsInTheme.Count >= 2)
                    {
                        MessageBox.Show("Достигнуто максимальное количество вопросов для этого раунда (2).");
                        return;
                    }
                }
                else if (round == 4)
                {
                    if (questionsInTheme.Count >= 1)
                    {
                        MessageBox.Show("Достигнуто максимальное количество вопросов для этого раунда (1).");
                        return;
                    }
                }

                var question = new Question
                {
                    Text = QuestionTextBox.Text.Trim(),
                    Points = points,
                    Round = round,
                    ThemeId = selectedThemeId,
                    PackId = selectedPackId,
                    CreatedBy = GameSettings.PlayerId,
                    CreatedDate = DateTime.Now,
                    LastUpdated = DateTime.Now,
                    IsActive = true,
                    IsPlayed = false
                };

                if (question.Round == 1)
                {
                    question.Option1 = string.IsNullOrWhiteSpace(Option1Box.Text) ? null : Option1Box.Text.Trim();
                    question.Option2 = string.IsNullOrWhiteSpace(Option2Box.Text) ? null : Option2Box.Text.Trim();
                    question.Option3 = string.IsNullOrWhiteSpace(Option3Box.Text) ? null : Option3Box.Text.Trim();
                    question.Option4 = string.IsNullOrWhiteSpace(Option4Box.Text) ? null : Option4Box.Text.Trim();
                    question.CorrectOption = CorrectOptionComboBox.SelectedIndex >= 0 ? CorrectOptionComboBox.SelectedIndex + 1 : (int?)null;
                    question.Answer = "N/A";
                    question.Answer2 = null;
                    question.Answer3 = null;

                    if (string.IsNullOrWhiteSpace(question.Option1) ||
                        string.IsNullOrWhiteSpace(question.Option2) ||
                        string.IsNullOrWhiteSpace(question.Option3) ||
                        string.IsNullOrWhiteSpace(question.Option4) ||
                        question.CorrectOption == null)
                    {
                        MessageBox.Show("Заполните все варианты ответа и выберите правильный вариант.");
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

                    string answerText = AnswerBox.Text == "Основной ответ (обязательно)" ? null : AnswerBox.Text.Trim();
                    string answer2Text = Answer2Box.Text == "Дополнительный ответ 2 (опционально)" ? null : Answer2Box.Text.Trim();
                    string answer3Text = Answer3Box.Text == "Дополнительный ответ 3 (опционально)" ? null : Answer3Box.Text.Trim();

                    question.Answer = string.IsNullOrWhiteSpace(answerText) ? null : answerText;
                    question.Answer2 = string.IsNullOrWhiteSpace(answer2Text) ? null : answer2Text;
                    question.Answer3 = string.IsNullOrWhiteSpace(answer3Text) ? null : answer3Text;

                    if (string.IsNullOrWhiteSpace(question.Answer))
                    {
                        MessageBox.Show("Укажите хотя бы один правильный ответ.");
                        return;
                    }

                    // Убедимся, что Option1-Option4 не пустые
                    if (string.IsNullOrWhiteSpace(question.Option1) ||
                        string.IsNullOrWhiteSpace(question.Option2) ||
                        string.IsNullOrWhiteSpace(question.Option3) ||
                        string.IsNullOrWhiteSpace(question.Option4))
                    {
                        MessageBox.Show("Все поля вариантов ответа должны быть заполнены.");
                        return;
                    }
                }

                try
                {
                    context.Questions.Add(question);
                    context.SaveChanges();
                    MessageBox.Show("Вопрос успешно добавлен!");
                    ClearFields();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении вопроса: {ex.Message}\nПодробности: {ex.StackTrace}");
                }
            }
        }

        private void ClearFields()
        {
            QuestionTextBox.Text = "";
            PointsBox.Text = "100";
            Option1Box.Text = "";
            Option2Box.Text = "";
            Option3Box.Text = "";
            Option4Box.Text = "";
            AnswerBox.Text = "Основной ответ (обязательно)";
            Answer2Box.Text = "Дополнительный ответ 2 (опционально)";
            Answer3Box.Text = "Дополнительный ответ 3 (опционально)";
            CorrectOptionComboBox.SelectedIndex = -1;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}