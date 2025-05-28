using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace MEGAGame.Core.Models
{
    public class Player
    {
        [Key]
        public int PlayerId { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public DateTime RegistrationDate { get; set; } // Заменили CreatedDate
        public DateTime LastLogin { get; set; }
        public int Score { get; set; } // Добавлено
        public double Rating { get; set; } // Добавлено

        public virtual ICollection<SessionPlayer>? SessionPlayers { get; set; }
    }
}