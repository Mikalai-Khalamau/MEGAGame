using System;
using System.Linq;
using System.Windows;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;

namespace MEGAGame.Client
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            try
            {
                using (var context = new GameDbContext())
                {
                    context.Database.EnsureCreated();
                    Console.WriteLine("База данных инициализируется...");

                    if (!context.Themes.Any())
                    {
                        Console.WriteLine("Начало заполнения тем и вопросов...");

                        // Раунд 1: Викторина (оставим без изменений, так как он работает)
                        int[] pointsRound1 = { 100, 100, 100, 100, 100 };
                        var historyTheme1 = new Theme { Name = "История (Викторина)" };
                        context.Themes.Add(historyTheme1);
                        context.SaveChanges();
                        context.Questions.AddRange(
                            new Question { Text = "В каком году произошла Октябрьская революция?", Option1 = "1914", Option2 = "1917", Option3 = "1921", Option4 = "1905", CorrectOption = 2, Points = pointsRound1[0], ThemeId = historyTheme1.Id, Round = 1 },
                            new Question { Text = "Кто был первым президентом США?", Option1 = "Авраам Линкольн", Option2 = "Джордж Вашингтон", Option3 = "Томас Джефферсон", Option4 = "Джон Адамс", CorrectOption = 2, Points = pointsRound1[1], ThemeId = historyTheme1.Id, Round = 1 }
                        );
                        context.SaveChanges();

                        var geographyTheme1 = new Theme { Name = "География (Викторина)" };
                        context.Themes.Add(geographyTheme1);
                        context.SaveChanges();
                        context.Questions.AddRange(
                            new Question { Text = "Какая река самая длинная в мире?", Option1 = "Амазонка", Option2 = "Нил", Option3 = "Янцзы", Option4 = "Миссисипи", CorrectOption = 1, Points = pointsRound1[0], ThemeId = geographyTheme1.Id, Round = 1 },
                            new Question { Text = "Какой континент самый маленький по площади?", Option1 = "Африка", Option2 = "Азия", Option3 = "Австралия", Option4 = "Европа", CorrectOption = 3, Points = pointsRound1[1], ThemeId = geographyTheme1.Id, Round = 1 }
                        );
                        context.SaveChanges();

                        // Раунд 2: Своя игра (оставим без изменений)
                        int[] pointsRound2 = { 100, 200, 300, 400, 500 };
                        var historyTheme2 = new Theme { Name = "История (Своя игра)" };
                        context.Themes.Add(historyTheme2);
                        context.SaveChanges();
                        context.Questions.AddRange(
                            new Question { Text = "Как звали последнего царя Российской империи?", Answer = "Николай II", Points = pointsRound2[0], ThemeId = historyTheme2.Id, Round = 2 },
                            new Question { Text = "Как называется битва 1815 года, где Наполеон потерпел окончательное поражение?", Answer = "Ватерлоо", Points = pointsRound2[1], ThemeId = historyTheme2.Id, Round = 2 }
                        );
                        context.SaveChanges();

                        var geographyTheme2 = new Theme { Name = "География (Своя игра)" };
                        context.Themes.Add(geographyTheme2);
                        context.SaveChanges();
                        context.Questions.AddRange(
                            new Question { Text = "Как называется столица Бразилии?", Answer = "Бразилиа", Points = pointsRound2[0], ThemeId = geographyTheme2.Id, Round = 2 },
                            new Question { Text = "Какой остров является самым большим в Средиземном море?", Answer = "Сицилия", Points = pointsRound2[1], ThemeId = geographyTheme2.Id, Round = 2 }
                        );
                        context.SaveChanges();

                        // Раунд 3: Что? Где? Когда? (оставим без изменений, предполагается, что он есть)
                        int[] pointsRound3 = { 300, 300, 300, 300, 300 };
                        var historyTheme3 = new Theme { Name = "История (Что? Где? Когда?)" };
                        context.Themes.Add(historyTheme3);
                        context.SaveChanges();
                        context.Questions.AddRange(
                            new Question { Text = "Какой город был основан в 753 году до нашей эры?", Answer = "Рим", Points = pointsRound3[0], ThemeId = historyTheme3.Id, Round = 3 },
                            new Question { Text = "Как звали первого человека, ступившего на Луну?", Answer = "Нил Армстронг", Points = pointsRound3[1], ThemeId = historyTheme3.Id, Round = 3 }
                        );
                        context.SaveChanges();

                        // Раунд 4: Ставки (добавляем одну тему и один вопрос)
                        var finalTheme = new Theme { Name = "Финальный вопрос (Ставки)" };
                        context.Themes.Add(finalTheme);
                        context.SaveChanges();
                        Console.WriteLine($"Добавлена тема: {finalTheme.Name}, Id: {finalTheme.Id}");

                        context.Questions.AddRange(
                            new Question
                            {
                                Text = "Как называется крупнейший океан на Земле?",
                                Answer = "Тихий океан",
                                Points = 0, // Стоимость будет установлена в зависимости от ставки
                                ThemeId = finalTheme.Id,
                                Round = 4
                            }
                        );
                        context.SaveChanges();
                        Console.WriteLine("Добавлен 1 вопрос для Финального раунда (Ставки)");

                        context.SaveChanges();
                        Console.WriteLine("Инициализация завершена!");
                    }
                    else
                    {
                        Console.WriteLine("База данных уже содержит данные.");
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Ошибка при инициализации: {ex.Message}\nВнутренняя ошибка: {ex.InnerException?.Message}";
                Console.WriteLine(errorMessage);
                MessageBox.Show(errorMessage);
            }
        }
    }
}