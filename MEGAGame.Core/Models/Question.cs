namespace MEGAGame.Core.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public string? Option1 { get; set; } // Сделали nullable
        public string? Option2 { get; set; }
        public string? Option3 { get; set; }
        public string? Option4 { get; set; }
        public int? CorrectOption { get; set; } // Сделали nullable
        public string? Answer { get; set; }
        public int Points { get; set; }
        public bool IsPlayed { get; set; }
        public int Round { get; set; }
        public int ThemeId { get; set; }
        public Theme Theme { get; set; }
    }
}