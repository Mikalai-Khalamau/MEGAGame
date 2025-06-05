#nullable enable                
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEGAGame.Core.Models
{
    public class Question
    {
        [Key] public int QuestionId { get; set; }

        [Required] public string Text { get; set; } = null!;

        [Required] public int Points { get; set; }
        [Required] public int Round { get; set; }

        [Required] public string Option1 { get; set; } = null!;
        [Required] public string Option2 { get; set; } = null!;
        [Required] public string Option3 { get; set; } = null!;
        [Required] public string Option4 { get; set; } = null!;

        public int? CorrectOption { get; set; }

        [Required] public string Answer { get; set; } = null!;
        public string? Answer2 { get; set; }
        public string? Answer3 { get; set; }

        [Required] public int ThemeId { get; set; }
        [ForeignKey(nameof(ThemeId))] public Theme Theme { get; set; } = null!;

        [Required] public int PackId { get; set; }
        [ForeignKey(nameof(PackId))] public QuestionPack Pack { get; set; } = null!;

        [Required] public int CreatedBy { get; set; }
        [ForeignKey(nameof(CreatedBy))] public Player CreatedByPlayer { get; set; } = null!;

        [Required] public DateTime CreatedDate { get; set; }
        [Required] public DateTime LastUpdated { get; set; }
        [Required] public bool IsActive { get; set; }
        [Required] public bool IsPlayed { get; set; }
    }
}