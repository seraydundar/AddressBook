using Npgsql;

namespace AddressBook.Data
{
    public static class Db
    {
        // TODO: Burayı kendi bilgilerine göre düzenle
        private const string ConnString =
            "Host=localhost;Port=5432;Database=address_book;Username=postgres;Password=1963";

        public static NpgsqlConnection CreateConnection()
            => new NpgsqlConnection(ConnString);
    }
}
