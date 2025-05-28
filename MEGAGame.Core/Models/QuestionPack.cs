using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MEGAGame.Core.Models
{
    public class QuestionPack
    {
        [Key]
        public int PackId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int CreatedBy { get; set; }

        [Required]
        public DateTime CreatedDate { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }

        [Required]
        public bool IsPublished { get; set; }

        public List<Theme> Themes { get; set; }

        public List<Question> Questions { get; set; } // Добавлено
    }
}