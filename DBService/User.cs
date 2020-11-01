namespace DBService
{
    [System.Serializable]
    public class User
    {
        long ID { get; }
        string Name { get; set; }
        string Login { get; set; }

        int Coins { get; set; }

        public User(long id, string name, string login, int coins)
        {
            ID = id;
            Name = name;
            Login = login;
            Coins = coins;
        }
    }
}
