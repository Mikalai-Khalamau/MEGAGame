using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Добавляем для [Key]

namespace MEGAGame.Core.Models
{
    public class GameSession
    {
        [Key] // Явно указываем первичный ключ
        public int SessionId { get; set; }
        public int PackId { get; set; }
        public string? Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public int HostId { get; set; }
        public virtual Player? Host { get; set; }
        public virtual ICollection<SessionPlayer>? SessionPlayers { get; set; }
    }
}