namespace MEGAGame.Core.Models
{
    public class SessionPlayer
    {
        public int SessionPlayerId { get; set; }
        public string SessionId { get; set; }
        public string PlayerId { get; set; }
        public int Score { get; set; }
        public DateTime LastUpdated { get; set; }
        public GameSession Session { get; set; }
    }
}