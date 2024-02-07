using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Parse.Domain
{
    internal class PostgreDbProvider : IDbProvider
    {
        private readonly string connectionString;

        public PostgreDbProvider(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<string> GetAnotherUrlsCount()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT count(*) FROM anotherurls";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            reader.Read();

            return reader[0].ToString();

        }

        public async Task InsertUrlEntity(URLEntity urlEntity)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = "INSERT INTO urlandhtml (url, html, text,links, date) VALUES (@url, @html, @text, @links, @date)";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("url", urlEntity.URL);
            cmd.Parameters.AddWithValue("html", urlEntity.Title);
            cmd.Parameters.AddWithValue("text", urlEntity.Text);
            cmd.Parameters.AddWithValue("links", urlEntity.Links);
            cmd.Parameters.AddWithValue("date", DateTime.Now);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                // Console.WriteLine("Вставил новую запаршенную ссылку " + urlEntity.URL + " в " + DateTime.Now);

            }
            catch
            {
                // Console.WriteLine("Небольшая заминочка на ссылке: " + urlEntity.URL);
            }
        }

        public async Task InsertAnotherLink(List<string> urls)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();

            foreach (string url in urls)
            {
                string sql = "INSERT INTO anotherurls (url) VALUES (@url) ON CONFLICT DO NOTHING ";
                using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
                cmd.Parameters.AddWithValue("url", url);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAnotherUrl(string url)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = "DELETE FROM anotherurls WHERE url=@url";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("url", url);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {

            }
        }

        public async Task<List<string>> GetAnotherUrls()
        {
            using NpgsqlConnection connection = new NpgsqlConnection(connectionString);
            connection.Open();

            string sql = "SELECT url FROM anotherurls";
            NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
            using NpgsqlDataReader reader = cmd.ExecuteReader();
            List<string> AnotherLinks = new List<string>();

            while (reader.Read())
            {
                AnotherLinks.Add(reader[0].ToString());
            }

            // await ClearAnotherUrls();
            return AnotherLinks;

        }

        public async Task ClearAnotherUrls()
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();
            string sql = "DELETE FROM anotherurls";
            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> ContainsUrlandhtml(string url)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = "SELECT url FROM urlandhtml WHERE url=@url";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("url", url);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return reader[0].ToString() == url;
            }
            else return false;
        }

        public async Task InsertUnaccessedUrl(string url)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();
            string sql = "INSERT INTO unaccessedurl (url) VALUES (@url) ON CONFLICT DO NOTHING ";
            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("url", url);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<bool> ContainsUnaccessedUrll(string url)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = "SELECT url FROM unaccessedurl WHERE url=@url";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("url", url);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (reader.Read())
            {
                return reader[0].ToString() == url;
            }
            else return false;
        }

        public async Task<List<Robots>> InsertRobots(Robots robots)
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = "INSERT INTO robots (host, file) VALUES (@host, @file)";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("host", robots.Host);
            cmd.Parameters.AddWithValue("file", robots.Content);

            try
            {
                await cmd.ExecuteNonQueryAsync();
                return await GetRobots();
            }
            catch
            {
                return await GetRobots();
            }
        }

        public async Task<List<Robots>> GetRobots()
        {
            using NpgsqlConnection con = new NpgsqlConnection(connectionString);
            var result = new List<Robots>();
            con.Open();

            string sql = "SELECT * FROM robots";

            using NpgsqlCommand cmd = new NpgsqlCommand(sql, con);

            using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (reader.Read())
            {
                result.Add(new Robots(reader[0].ToString(), reader[1].ToString()));
            }

            return result;
        }
    }
}
