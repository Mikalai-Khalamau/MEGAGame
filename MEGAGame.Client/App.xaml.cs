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
            using (var context = new GameDbContext())
            {
                // Убедимся, что контекст создаёт схему базы данных
                context.Database.EnsureCreated();

                // Проверяем, есть ли данные в таблице Themes
                if (!context.Themes.Any())
                {
                    var historyTheme = new Theme { Name = "История" };
                    context.Themes.Add(historyTheme);
                    context.SaveChanges();

                    context.Questions.AddRange(
                        new Question
                        {
                            Text = "В каком году произошла Октябрьская революция?",
                            Option1 = "1914",
                            Option2 = "1917",
                            Option3 = "1921",
                            Option4 = "1905",
                            CorrectOption = 2,
                            ThemeId = historyTheme.Id,
                            Points = 100
                        },
                        new Question
                        {
                            Text = "Кто был первым президентом США?",
                            Option1 = "Авраам Линкольн",
                            Option2 = "Джордж Вашингтон",
                            Option3 = "Томас Джефферсон",
                            Option4 = "Джон Адамс",
                            CorrectOption = 2,
                            ThemeId = historyTheme.Id,
                            Points = 100
                        }
                    );

                    var scienceTheme = new Theme { Name = "Наука" };
                    context.Themes.Add(scienceTheme);
                    context.SaveChanges();

                    context.Questions.AddRange(
                        new Question
                        {
                            Text = "Как называется химический элемент с символом O?",
                            Option1 = "Золото",
                            Option2 = "Кислород",
                            Option3 = "Серебро",
                            Option4 = "Водород",
                            CorrectOption = 2,
                            ThemeId = scienceTheme.Id,
                            Points = 100
                        },
                        new Question
                        {
                            Text = "Какова скорость света в вакууме?",
                            Option1 = "300 000 км/с",
                            Option2 = "150 000 км/с",
                            Option3 = "450 000 км/с",
                            Option4 = "600 000 км/с",
                            CorrectOption = 1,
                            ThemeId = scienceTheme.Id,
                            Points = 100
                        }
                    );

                    context.SaveChanges();
                }
            }
        }
    }
}