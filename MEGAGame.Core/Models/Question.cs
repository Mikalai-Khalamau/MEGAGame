namespace MEGAGame.Core.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }
        public int CorrectOption { get; set; } // 1, 2, 3 или 4
        public int ThemeId { get; set; }
        public Theme Theme { get; set; }
        public int Points { get; set; } = 100; // Очки за правильный ответ
    }
}