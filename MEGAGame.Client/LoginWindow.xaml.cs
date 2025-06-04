using System;
using System.Windows;
using System.Windows.Threading;
using MEGAGame.Core.Models;
using MEGAGame.Core.Services;

namespace MEGAGame.Client
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            if (!GameSettings.IsFirstRun)
            {
                UsernameBox.Text = GameSettings.PlayerUsername;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageTextBlock.Text = "Пожалуйста, введите никнейм!";
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageTextBlock.Text = "Пожалуйста, введите email!";
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageTextBlock.Text = "Пожалуйста, введите пароль!";
                return;
            }

            // Валидация длины
            if (username.Length > 16)
            {
                MessageTextBlock.Text = "Максимальная длина ника — 16 символов.";
                return;
            }

            var player = PlayerService.GetPlayerByUsername(username);
            if (player == null || !BCrypt.Net.BCrypt.Verify(password, player.Password))
            {
                MessageTextBlock.Text = "Такого пользователя не существует или неверный пароль.";
                return;
            }

            GameSettings.PlayerId = player.PlayerId;
            GameSettings.PlayerUsername = player.Username;
            GameSettings.IsFirstRun = false;

            player.LastLogin = DateTime.Now;
            PlayerService.UpdatePlayer(player);

            OpenMainMenu(player);
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string email = EmailBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageTextBlock.Text = "Пожалуйста, введите никнейм!";
                return;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageTextBlock.Text = "Пожалуйста, введите email!";
                return;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                MessageTextBlock.Text = "Пожалуйста, введите пароль!";
                return;
            }

            // Валидация длины
            if (username.Length > 16)
            {
                MessageTextBlock.Text = "Максимальная длина ника — 16 символов.";
                return;
            }
            if (email.Length > 30)
            {
                MessageTextBlock.Text = "Максимальная длина почты — 30 символов.";
                return;
            }

            var existingPlayerByUsername = PlayerService.GetPlayerByUsername(username);
            var existingPlayerByEmail = PlayerService.GetPlayerByEmail(email);

            if (existingPlayerByUsername != null)
            {
                MessageTextBlock.Text = "Пользователь с таким никнеймом уже существует. Поменяйте данные.";
                return;
            }
            if (existingPlayerByEmail != null)
            {
                MessageTextBlock.Text = "Пользователь с таким email уже существует. Поменяйте данные.";
                return;
            }

            var player = new Player
            {
                Username = username,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password),
                Score = 0,
                Rating = 1500,
                RegistrationDate = DateTime.Now,
                LastLogin = DateTime.Now
            };

            PlayerService.SavePlayer(player);
            GameSettings.PlayerId = player.PlayerId;
            GameSettings.PlayerUsername = player.Username;
            GameSettings.IsFirstRun = false;

            //MessageTextBlock.Text = "Регистрация успешна! Вы можете войти.";
            OpenMainMenu(player);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenMainMenu(Player player)
        {
            var mainMenu = new MainMenuWindow(player);
            mainMenu.Show();
            this.Close();
        }
    }
}