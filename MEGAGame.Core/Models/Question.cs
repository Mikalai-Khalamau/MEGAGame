using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEGAGame.Core.Models
{
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public string Text { get; set; }

        [Required]
        public int Points { get; set; }

        [Required]
        public int Round { get; set; }

        public string Option1 { get; set; }
        public string Option2 { get; set; }
        public string Option3 { get; set; }
        public string Option4 { get; set; }

        public int? CorrectOption { get; set; }

        public string Answer { get; set; }

        [Required]
        public int ThemeId { get; set; }

        [ForeignKey("ThemeId")]
        public Theme Theme { get; set; }

        [Required]
        public int PackId { get; set; }

        [ForeignKey("PackId")]
        public QuestionPack Pack { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [ForeignKey("CreatedBy")]
        public Player CreatedByPlayer { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [Required]
        public bool IsPlayed { get; set; }
    }
}