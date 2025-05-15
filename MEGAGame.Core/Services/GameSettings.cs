namespace MEGAGame.Core.Services
{
    public static class GameSettings
    {
        public static string PlayerName { get; set; }
        public static string PlayerFirstName { get; set; }
        public static string PlayerLastName { get; set; }
        public static string PlayerCity { get; set; }
        public static string PlayerEmail { get; set; }
        public static int PlayerScore { get; set; } = 0;
        public static bool IsFirstRun { get; set; } = true;
    }
}