using System.Windows;

namespace MEGAGame.Client
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Answer_Click(object sender, RoutedEventArgs e)
        {
            // Логика ответа на вопрос (будет реализована позже)
            MessageBox.Show("Функция ответа будет реализована позже!");
        }

        private void NextQuestion_Click(object sender, RoutedEventArgs e)
        {
            // Логика перехода к следующему вопросу (будет реализована позже)
            MessageBox.Show("Функция следующего вопроса будет реализована позже!");
        }
    }
}