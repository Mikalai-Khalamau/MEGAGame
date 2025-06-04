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

        public enum BotDifficultyLevel { Easy, Medium, Hard }
        public static BotDifficultyLevel BotDifficulty { get; set; }

        public enum GameModeType { SinglePlayer, Bot, Friend, Online } // Добавлено Online
        public static GameModeType GameMode { get; set; }

        // Музыкальные треки
        public static string[] MusicTracks { get; } = new string[]
        {
            "No Music", // Первый вариант - без музыки
            @"C:\Users\khala\Downloads\tam-gde-klen-shumit-nad-rechnoy-volnoy.mp3",
            @"C:\Users\khala\Downloads\Тату - Нас Не Догонят.mp3",
            @"C:\Users\khala\Downloads\baby-shark.mp3",
            @"C:\Users\khala\Downloads\Михаил Круг - Владимирский Централ.mp3",
            "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3"
        };

        public static int SelectedMusicTrackIndex { get; set; } = 0; // По умолчанию без музыки

        public static void ResetForNewGame()
        {
            PlayerScore = 0;
            CurrentRound = 1;
        }
    }
}