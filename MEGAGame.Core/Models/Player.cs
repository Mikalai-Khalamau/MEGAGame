namespace MEGAGame.Core.Models
{
    public class Player
    {
        public int Id { get; set; } // Добавляем Id как первичный ключ
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; } = 1000; // Начальный рейтинг
        public int Score { get; set; }
    }
}