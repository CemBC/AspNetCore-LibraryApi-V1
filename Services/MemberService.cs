using LibraryApi.Models;
using Microsoft.Data.SqlClient;

namespace LibraryApi.Services
{
    public class MemberService
    {
        private readonly string _connectionString;

        public MemberService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("LibraryDb")!;
        }

        public List<Member> GetAll()
        {
            List<Member> members = new();

            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                "SELECT Id, FullName, Email FROM Members";

            using SqlCommand command =
                new SqlCommand(query, connection);

            using SqlDataReader reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                Member member = new Member
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2)
                };

                members.Add(member);
            }

            return members;
        }

        public Member? GetById(int id)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                "SELECT Id, FullName, Email FROM Members WHERE Id = @Id";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            using SqlDataReader reader =
                command.ExecuteReader();

            if (reader.Read())
            {
                return new Member
                {
                    Id = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2)
                };
            }

            return null;
        }

        public void AddMember(Member member)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                @"INSERT INTO Members (FullName, Email)
                  OUTPUT INSERTED.Id
                  VALUES (@FullName, @Email)";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@FullName", member.FullName);
            command.Parameters.AddWithValue("@Email", member.Email);

            member.Id = Convert.ToInt32(command.ExecuteScalar());
        }

        public bool Update(int id, Member updatedMember)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                @"UPDATE Members
                  SET FullName = @FullName,
                      Email = @Email
                  WHERE Id = @Id";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@FullName", updatedMember.FullName);
            command.Parameters.AddWithValue("@Email", updatedMember.Email);
            command.Parameters.AddWithValue("@Id", id);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows > 0;
        }

        public bool Delete(int id)
        {
            using SqlConnection connection =
                new SqlConnection(_connectionString);

            connection.Open();

            string query =
                "DELETE FROM Members WHERE Id = @Id";

            using SqlCommand command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@Id", id);

            int affectedRows = command.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }
}