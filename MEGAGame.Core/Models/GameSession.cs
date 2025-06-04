namespace MEGAGame.Core.Models
{
    public class GameSession
    {
        public string SessionId { get; set; }
        public int PackId { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastUpdated { get; set; }
        public string HostId { get; set; }
        public bool IsPublic { get; set; }
        public string JoinKey { get; set; }
        public List<SessionPlayer> Players { get; set; } = new List<SessionPlayer>();
    }
}