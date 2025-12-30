using AddressBook.Models;
using Npgsql;
using System.Collections.Generic;

namespace AddressBook.Data
{
    public class PersonRepository
    {
        public List<Person> GetAll()
        {
            var list = new List<Person>();

            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"SELECT id, first_name, last_name, phone
                  FROM persons
                  ORDER BY id DESC;", conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Person
                {
                    Id = reader.GetInt32(0),
                    FirstName = reader.GetString(1),
                    LastName = reader.GetString(2),
                    Phone = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }

            return list;
        }

        public int Add(Person p)
        {
            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"INSERT INTO persons (first_name, last_name, phone)
                  VALUES (@fn, @ln, @ph)
                  RETURNING id;", conn);

            cmd.Parameters.AddWithValue("@fn", p.FirstName);
            cmd.Parameters.AddWithValue("@ln", p.LastName);
            cmd.Parameters.AddWithValue("@ph", (object?)p.Phone ?? DBNull.Value);

            return (int)cmd.ExecuteScalar()!;
        }

        public void Delete(int id)
        {
            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"DELETE FROM persons WHERE id=@id;", conn);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void Update(Person p)
        {
            using var conn = Db.CreateConnection();
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"UPDATE persons
          SET first_name=@fn, last_name=@ln, phone=@ph
          WHERE id=@id;", conn);

            cmd.Parameters.AddWithValue("@fn", p.FirstName);
            cmd.Parameters.AddWithValue("@ln", p.LastName);
            cmd.Parameters.AddWithValue("@ph", (object?)p.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", p.Id);

            cmd.ExecuteNonQuery();
        }

    }
}
