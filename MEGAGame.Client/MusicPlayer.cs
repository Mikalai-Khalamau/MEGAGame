using System;
using System.Windows.Media;
using System.Windows.Threading;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public static class MusicPlayer
    {
        private static MediaPlayer mediaPlayer = new MediaPlayer();
        private static DispatcherTimer timer = new DispatcherTimer();
        private static int currentTrackIndex = -1;
        private static bool IsPlaying = false; // Флаг для отслеживания состояния воспроизведения

        static MusicPlayer()
        {
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
        }

        public static void PlayMusic()
        {
            // Если музыка уже играет, не перезапускаем её
            if (IsPlaying)
                return;

            if (GameSettings.SelectedMusicTrackIndex == 0) // Без музыки
            {
                StopMusic();
                return;
            }

            currentTrackIndex = GameSettings.SelectedMusicTrackIndex - 1; // Смещение, т.к. 0 - это "No Music"
            PlayCurrentTrack();
            timer.Start();
            IsPlaying = true;
        }

        private static void PlayCurrentTrack()
        {
            if (currentTrackIndex < 0 || currentTrackIndex >= GameSettings.MusicTracks.Length - 1)
                return;

            try
            {
                mediaPlayer.Stop();
                mediaPlayer.Open(new Uri(GameSettings.MusicTracks[currentTrackIndex + 1], UriKind.RelativeOrAbsolute)); // ✅
                mediaPlayer.Play();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка воспроизведения трека: {ex.Message}");
            }
        }

        private static void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            NextTrack();
        }

        private static void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.Source == null || !mediaPlayer.NaturalDuration.HasTimeSpan)
                return;

            if (mediaPlayer.Position >= mediaPlayer.NaturalDuration.TimeSpan)
            {
                NextTrack();
            }
        }

        private static void NextTrack()
        {
            currentTrackIndex++;
            if (currentTrackIndex >= GameSettings.MusicTracks.Length - 1) // -1, т.к. 0 - "No Music"
                currentTrackIndex = 0; // Начинаем с первого трека (индекс 1 в MusicTracks)

            PlayCurrentTrack();
        }

        public static void StopMusic()
        {
            mediaPlayer.Stop();
            mediaPlayer.Close();
            timer.Stop();
            IsPlaying = false;
        }

        // Метод для изменения текущего трека без остановки воспроизведения
        public static void ChangeTrack(int newTrackIndex)
        {
            GameSettings.SelectedMusicTrackIndex = newTrackIndex;
            if (GameSettings.SelectedMusicTrackIndex == 0) // Без музыки
            {
                StopMusic();
                return;
            }

            currentTrackIndex = GameSettings.SelectedMusicTrackIndex - 1;
            PlayCurrentTrack();
            if (!IsPlaying)
            {
                timer.Start();
                IsPlaying = true;
            }
        }
    }
}