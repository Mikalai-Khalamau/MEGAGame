using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;
using Microsoft.Extensions.Logging;

namespace MEGAGame.Client
{
    public partial class OnlineGameWindow : Window
    {
        private HubConnection _hubConnection;
        private readonly Player _currentPlayer;
        private string _sessionId = "";
        private readonly DispatcherTimer _timer;
        private int _timeLeft;
        private bool _isHost;

        public OnlineGameWindow(Player player)
        {
            InitializeComponent();
            _currentPlayer = player ?? throw new ArgumentNullException(nameof(player));
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;
            KeyDown += OnlineGameWindow_KeyDown;
            LobbyPanel.Visibility = Visibility.Visible;
            SetupSignalR();
        }

        private async void SetupSignalR()
        {
            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5065/gamehub")
                    .ConfigureLogging(logging =>
                    {
                        logging.AddConsole();
                        logging.SetMinimumLevel(LogLevel.Debug);
                    })
                    .Build();

                Console.WriteLine("SignalR connection built");

                _hubConnection.On<dynamic>("ReceiveSessionInfo", (info) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        _sessionId = info.SessionId;
                        RoomInfo.Text = $"Комната: {_sessionId} {(info.JoinKey != null ? $"Ключ: {info.JoinKey}" : "")}";
                        _isHost = true;
                        StartGameButton.Visibility = Visibility.Visible;
                        Console.WriteLine($"SessionId received: {_sessionId}, JoinKey: {info.JoinKey}");
                    });
                });

                _hubConnection.On<List<SessionPlayer>>("UpdatePlayers", (players) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine($"Received {players.Count} players");
                        PlayersList.ItemsSource = players;
                    });
                });

                _hubConnection.On<string>("JoinRoomFailed", (message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Ошибка при присоединении: {message}", "Ошибка");
                    });
                });

                _hubConnection.On<string>("StartGameFailed", (message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show($"Ошибка при запуске игры: {message}", "Ошибка");
                    });
                });

                _hubConnection.On<string>("GameStarted", (sessionId) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Console.WriteLine($"Game started received for session: {sessionId}");
                        LobbyPanel.Visibility = Visibility.Collapsed;
                        GamePanel.Visibility = Visibility.Visible;
                        BuzzButton.Visibility = Visibility.Visible;
                    });
                });

                _hubConnection.On<Question, int>("UpdateQuestion", (question, time) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        QuestionText.Text = question.Text;
                        _timeLeft = time;
                        TimerText.Text = $"Время: {_timeLeft}";
                        _timer.Start();
                        BuzzButton.IsEnabled = true;
                        AnswerInput.Visibility = question.Round == 1 ? Visibility.Collapsed : Visibility.Visible;
                        SubmitAnswerButton.Visibility = Visibility.Collapsed;
                    });
                });

                _hubConnection.On<string>("PlayerBuzzed", (playerId) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (playerId == _currentPlayer.PlayerId.ToString())
                        {
                            BuzzButton.IsEnabled = false;
                            AnswerInput.IsEnabled = true;
                            SubmitAnswerButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            BuzzButton.IsEnabled = false;
                        }
                    });
                });

                _hubConnection.On<string, string, bool, int>("AnswerSubmitted", (playerId, answer, isCorrect, score) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ResultText.Text += $"\nИгрок {playerId} ответил: {answer} ({(isCorrect ? "Верно" : "Неверно")}), Очки: {score}";
                        if (!isCorrect && _timeLeft > 0)
                        {
                            BuzzButton.IsEnabled = true;
                            _timeLeft /= 2;
                            _timer.Start();
                        }
                        else
                        {
                            _timer.Stop();
                        }
                    });
                });

                await _hubConnection.StartAsync();
                Console.WriteLine("SignalR connection started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SignalR error: {ex.Message}");
                MessageBox.Show($"Ошибка подключения к серверу: {ex.Message}", "Ошибка");
                new MainMenuWindow(_currentPlayer).Show();
                Close();
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (--_timeLeft >= 0)
            {
                TimerText.Text = $"Время: {_timeLeft}";
            }
            else
            {
                _timer.Stop();
                BuzzButton.IsEnabled = false;
                AnswerInput.IsEnabled = false;
                SubmitAnswerButton.Visibility = Visibility.Collapsed;
            }
        }

        private async void CreatePublicRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentPlayer?.Username) || GameSettings.SelectedPackId == 0)
                {
                    MessageBox.Show("Выберите игрока и пакет вопросов перед созданием комнаты.", "Ошибка");
                    return;
                }
                Console.WriteLine($"Creating public room. Username: {_currentPlayer.Username}, PackId: {GameSettings.SelectedPackId}");
                await _hubConnection.InvokeAsync("CreateRoom", _currentPlayer.Username, GameSettings.SelectedPackId, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating public room: {ex.Message}");
                MessageBox.Show($"Ошибка при создании публичной комнаты: {ex.Message}", "Ошибка");
            }
        }

        private async void CreatePrivateRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_currentPlayer?.Username) || GameSettings.SelectedPackId == 0)
                {
                    MessageBox.Show("Выберите игрока и пакет вопросов перед созданием комнаты.", "Ошибка");
                    return;
                }
                Console.WriteLine($"Creating private room. Username: {_currentPlayer.Username}, PackId: {GameSettings.SelectedPackId}");
                await _hubConnection.InvokeAsync("CreateRoom", _currentPlayer.Username, GameSettings.SelectedPackId, false);
                JoinKey.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating private room: {ex.Message}");
                MessageBox.Show($"Ошибка при создании приватной комнаты: {ex.Message}", "Ошибка");
            }
        }

        private async void JoinRoom_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine($"Attempting to join session: {JoinSessionId.Text}, Player: {_currentPlayer.Username}, Key: {JoinKey.Text}");
                await _hubConnection.InvokeAsync("JoinRoom", JoinSessionId.Text, _currentPlayer.Username, JoinKey.Text);
                _sessionId = JoinSessionId.Text;
                RoomInfo.Text = $"Комната: {_sessionId}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JoinRoom error: {ex.Message}");
                MessageBox.Show($"Ошибка при присоединении к комнате: {ex.Message}", "Ошибка");
            }
        }

        private async void StartGame_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine($"Starting game for session: {_sessionId}");
                await _hubConnection.InvokeAsync("StartGame", _sessionId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"StartGame error: {ex.Message}");
                MessageBox.Show($"Ошибка при запуске игры: {ex.Message}", "Ошибка");
            }
        }

        private async void Buzz_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _hubConnection.InvokeAsync("Buzz", _sessionId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при нажатии на Buzz: {ex.Message}", "Ошибка");
            }
        }

        private void OnlineGameWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space && BuzzButton.IsEnabled)
            {
                Buzz_Click(null, null);
            }
        }

        private async void SubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _hubConnection.InvokeAsync("SubmitAnswer", _sessionId, AnswerInput.Text);
                AnswerInput.Text = "";
                AnswerInput.IsEnabled = false;
                SubmitAnswerButton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке ответа: {ex.Message}", "Ошибка");
            }
        }

        private async void Exit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_hubConnection != null)
                {
                    await _hubConnection.StopAsync();
                }
                new MainMenuWindow(_currentPlayer).Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе: {ex.Message}", "Ошибка");
            }
        }

        protected override async void OnClosed(EventArgs e)
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
            base.OnClosed(e);
        }
    }
}