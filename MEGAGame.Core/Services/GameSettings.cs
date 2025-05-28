namespace MEGAGame.Core.Services
{
    public static class GameSettings
    {
        public static int PlayerId { get; set; } = 1;
        public static string PlayerUsername { get; set; }
        public static int PlayerScore { get; set; } = 0;
        public static int CurrentRound { get; set; } = 1;
        public static bool IsFirstRun { get; set; } = true;
        public static int SelectedPackId { get; set; }

        public static void ResetForNewGame()
        {
            PlayerScore = 0;
            CurrentRound = 1;
        }
    }
}