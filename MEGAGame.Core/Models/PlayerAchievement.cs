using System;
using System.ComponentModel.DataAnnotations;

namespace MEGAGame.Core.Models
{
    public class PlayerAchievement
    {
        [Key]
        public int PlayerAchievementId { get; set; }
        public int PlayerId { get; set; }
        public int AchievementId { get; set; }
        public DateTime AchievedDate { get; set; }
        public virtual Player Player { get; set; }
        public virtual Achievement Achievement { get; set; }
    }
}