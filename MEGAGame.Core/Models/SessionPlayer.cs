using System;
using System.ComponentModel.DataAnnotations;
namespace MEGAGame.Core.Models
{
    public class SessionPlayer
    {
        
        public int SessionPlayerId { get; set; }
        public int SessionId { get; set; }
        public int PlayerId { get; set; }
        public int Score { get; set; }
        public DateTime LastUpdated { get; set; } // Добавлено

        public virtual GameSession? Session { get; set; }
        public virtual Player? Player { get; set; }
    }
}