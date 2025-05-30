using System;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using MahApps.Metro.Controls;

namespace MEGAGame.Client
{
    public partial class QuestionEditWindowRound234 : MetroWindow
    {
        private readonly QuestionPack selectedPack;
        private readonly Question question;

        public QuestionEditWindowRound234(QuestionPack pack, Question questionToEdit)
        {
            InitializeComponent();
            selectedPack = pack;
            question = questionToEdit;

            // Заполняем поля текущими значениями вопроса
            QuestionTextBox.Text = question.Text;
            AnswerTextBox.Text = question.Answer;
            PointsTextBox.Text = question.Points.ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string questionText = QuestionTextBox.Text.Trim();
                string answer = AnswerTextBox.Text.Trim();
                string pointsStr = PointsTextBox.Text.Trim();

                if (string.IsNullOrWhiteSpace(questionText) || string.IsNullOrWhiteSpace(answer) || string.IsNullOrWhiteSpace(pointsStr))
                {
                    MessageBox.Show("Заполните все поля!", "Ошибка");
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
                        question.Answer = answer;
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
                        questionToUpdate.Answer = answer;
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