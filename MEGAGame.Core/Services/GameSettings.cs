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
        public static int CurrentRound { get; set; } = 1; // Должно быть 1 при старте
        public static bool IsFirstRun { get; set; } = true;

        // Добавим метод сброса настроек при старте игры
        public static void ResetForNewGame()
        {
            PlayerScore = 0;
            CurrentRound = 1;
            IsFirstRun = true;
        }
    }
}