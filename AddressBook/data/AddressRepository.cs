using AddressBook.Models;
using Npgsql;
using System.Collections.Generic;

namespace AddressBook.Data
{
    public class AddressRepository
    {
        public List<Address> GetByPersonId(int personId)
        {
            var list = new List<Address>();

            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"SELECT id, person_id, title, city, district, address_line
                  FROM addresses
                  WHERE person_id = @pid
                  ORDER BY id DESC;", conn);

            cmd.Parameters.AddWithValue("@pid", personId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Address
                {
                    Id = reader.GetInt32(0),
                    PersonId = reader.GetInt32(1),
                    Title = reader.IsDBNull(2) ? null : reader.GetString(2),
                    City = reader.IsDBNull(3) ? null : reader.GetString(3),
                    District = reader.IsDBNull(4) ? null : reader.GetString(4),
                    AddressLine = reader.GetString(5)
                });
            }

            return list;
        }

        public int Add(Address a)
        {
            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"INSERT INTO addresses (person_id, title, city, district, address_line)
                  VALUES (@pid, @t, @c, @d, @line)
                  RETURNING id;", conn);

            cmd.Parameters.AddWithValue("@pid", a.PersonId);
            cmd.Parameters.AddWithValue("@t", (object?)a.Title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@c", (object?)a.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@d", (object?)a.District ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@line", a.AddressLine);

            return (int)cmd.ExecuteScalar()!;
        }

        public void Delete(int id)
        {
            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"DELETE FROM addresses WHERE id=@id;", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Update(Address a)
        {
            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"UPDATE addresses
          SET title=@t, city=@c, district=@d, address_line=@line
          WHERE id=@id;", conn);

            cmd.Parameters.AddWithValue("@t", (object?)a.Title ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@c", (object?)a.City ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@d", (object?)a.District ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@line", a.AddressLine);
            cmd.Parameters.AddWithValue("@id", a.Id);

            cmd.ExecuteNonQuery();
        }

    }
}
