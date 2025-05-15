using System.Windows;
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
                NicknameBox.Text = GameSettings.PlayerName;
                FirstNameBox.Text = GameSettings.PlayerFirstName;
                LastNameBox.Text = GameSettings.PlayerLastName;
                CityBox.Text = GameSettings.PlayerCity;
            }
        }

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NicknameBox.Text) ||
                string.IsNullOrWhiteSpace(FirstNameBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameBox.Text) ||
                string.IsNullOrWhiteSpace(CityBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены!");
                return;
            }

            GameSettings.PlayerName = NicknameBox.Text;
            GameSettings.PlayerFirstName = FirstNameBox.Text;
            GameSettings.PlayerLastName = LastNameBox.Text;
            GameSettings.PlayerCity = CityBox.Text;
            GameSettings.IsFirstRun = false;

            var player = new Player
            {
                Name = GameSettings.PlayerName,
                FirstName = GameSettings.PlayerFirstName,
                LastName = GameSettings.PlayerLastName,
                City = GameSettings.PlayerCity,
                Score = 0
            };
            MEGAGame.Core.Services.PlayerService.SavePlayer(player);

            new MainMenuWindow().Show();
            this.Close();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}