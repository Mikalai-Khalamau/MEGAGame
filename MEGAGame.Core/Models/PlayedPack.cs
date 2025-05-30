using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MEGAGame.Core.Models
{
    public class PlayedPack
    {
        [Key]
        public int PlayedPackId { get; set; }

        [Required]
        public int PlayerId { get; set; }

        [ForeignKey("PlayerId")]
        public Player Player { get; set; }

        [Required]
        public int PackId { get; set; }

        [ForeignKey("PackId")]
        public QuestionPack Pack { get; set; }

        [Required]
        public DateTime PlayedDate { get; set; }
    }
}