using System;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;

namespace MEGAGame.Client
{
    public partial class QuestionEditWindowRound1 : MetroWindow
    {
        private readonly QuestionPack selectedPack;
        private readonly Question question;

        public QuestionEditWindowRound1(QuestionPack pack, Question questionToEdit)
        {
            InitializeComponent();
            selectedPack = pack;
            question = questionToEdit;

            // Заполняем поля текущими значениями вопроса
            QuestionTextBox.Text = question.Text;
            Option1TextBox.Text = question.Option1;
            Option2TextBox.Text = question.Option2;
            Option3TextBox.Text = question.Option3;
            Option4TextBox.Text = question.Option4;
            CorrectOptionTextBox.Text = question.CorrectOption.ToString();
            PointsTextBox.Text = question.Points.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string questionText = QuestionTextBox.Text.Trim();
                string option1 = Option1TextBox.Text.Trim();
                string option2 = Option2TextBox.Text.Trim();
                string option3 = Option3TextBox.Text.Trim();
                string option4 = Option4TextBox.Text.Trim();
                string correctOptionStr = CorrectOptionTextBox.Text.Trim();
                string pointsStr = PointsTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(option1) ||
                    string.IsNullOrWhiteSpace(option2) || string.IsNullOrWhiteSpace(option3) ||
                    string.IsNullOrWhiteSpace(option4) || string.IsNullOrWhiteSpace(correctOptionStr) ||
                    string.IsNullOrWhiteSpace(pointsStr))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка");
                    return;
                }

                if (!int.TryParse(correctOptionStr, out int correctOption) || correctOption < 1 || correctOption > 4)
                {
                    MessageBox.Show("Некорректный номер правильного ответа. Укажите число от 1 до 4.", "Ошибка");
                    return;
                }

                if (!int.TryParse(pointsStr, out int points) || points <= 0)
                {
                    MessageBox.Show("Некорректное количество очков. Укажите положительное число.", "Ошибка");
                    return;
                }

                using (var context = new GameDbContext())
                {
                    // Если вопрос новый (QuestionId == 0), добавляем его
                    if (question.QuestionId == 0)
                    {
                        question.Text = questionText;
                        question.Option1 = option1;
                        question.Option2 = option2;
                        question.Option3 = option3;
                        question.Option4 = option4;
                        question.CorrectOption = correctOption;
                        question.Points = points;
                        question.LastUpdated = DateTime.Now;

                        context.Questions.Add(question);
                    }
                    else
                    {
                        // Если вопрос уже существует, обновляем его
                        var questionToUpdate = context.Questions.FirstOrDefault(q => q.QuestionId == question.QuestionId);
                        if (questionToUpdate == null)
                        {
                            MessageBox.Show("Вопрос не найден.", "Ошибка");
                            return;
                        }

                        questionToUpdate.Text = questionText;
                        questionToUpdate.Option1 = option1;
                        questionToUpdate.Option2 = option2;
                        questionToUpdate.Option3 = option3;
                        questionToUpdate.Option4 = option4;
                        questionToUpdate.CorrectOption = correctOption;
                        questionToUpdate.Points = points;
                        questionToUpdate.LastUpdated = DateTime.Now;
                    }

                    context.SaveChanges();
                    MessageBox.Show("Вопрос успешно сохранён!", "Успех");
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении вопроса: {ex.Message}", "Ошибка");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}