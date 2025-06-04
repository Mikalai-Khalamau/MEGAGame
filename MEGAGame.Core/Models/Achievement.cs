using System.ComponentModel.DataAnnotations;

namespace MEGAGame.Core.Models
{
    public class Achievement
    {
        [Key]
        public int AchievementId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}