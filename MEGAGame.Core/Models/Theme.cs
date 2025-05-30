    using System.ComponentModel.DataAnnotations;
    namespace MEGAGame.Core.Models
    {
        public class Theme
        {
            [Key]
            public int ThemeId { get; set; }
            public string Name { get; set; }
            public int Round { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime LastUpdated { get; set; }

            // Связь с пакетом
            public int PackId { get; set; }
            public virtual QuestionPack Pack { get; set; }

            // Связь с вопросами
            public virtual ICollection<Question> Questions { get; set; }
        }
    }