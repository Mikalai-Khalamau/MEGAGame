using System.Collections.Generic;

namespace MEGAGame.Core.Models
{
    public class Theme
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Question> Questions { get; set; } = new List<Question>();
    }
}