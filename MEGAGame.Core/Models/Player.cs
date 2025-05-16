namespace MEGAGame.Core.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string City { get; set; }
        public string Email { get; set; }
        public int Rating { get; set; } = 1000;
        public int Score { get; set; }
    }
}