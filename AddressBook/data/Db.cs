using Npgsql;

namespace AddressBook.Data
{
    public static class Db
    {
        // TODO: Burayı kendi bilgilerine göre düzenle
        private const string ConnString =
            "Host=localhost;Port=5433;Database=address_book;Username=postgres;Password=rndMdgl1980";

        public static NpgsqlConnection CreateConnection()
            => new NpgsqlConnection(ConnString);
    }
}
