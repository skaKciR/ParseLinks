using System.Text.RegularExpressions;
using System.Xml;
using Npgsql;
using Parse;
using System.Text.Encodings;
using System.Text.Encodings.Web;
using System.Text;
using System.Reflection.Metadata;
using NpgsqlTypes;
using System.Runtime.CompilerServices;
using System.Linq.Expressions;

string connectionString = "Server=localhost; port=5432; user id=postgres; password=star20021812; database=JanuaryTaskDB;";
Console.WriteLine("1. First parse");
Console.WriteLine("2. Another links parse");
Console.WriteLine("3. Get last another links index");
string choose = Console.ReadLine();
if (choose == "1")
{
    List<string> Urls = new List<string>() { "https://www.interfax.ru/business/", "https://www.interfax.ru/culture/", "https://www.sport-interfax.ru/", "https://www.interfax.ru/russia/", "https://www.interfax.ru/story/", "https://www.interfax.ru/photo/", "https://kuban.rbc.ru/krasnodar/story/63dcc4ea9a7947317518627b", "https://lenta.ru/", "https://krasnodarmedia.su/news/1673787/?utm_source=yxnews&utm_medium=desktop", "https://www.kommersant.ru/doc/6480758?utm_source=yxnews&utm_medium=desktop" };
    Console.WriteLine("Starting first parsing...");
    await Parse(Urls);
}
if (choose == "2")
{
    int lastId = 0;
    for (int i = 0; i < 3; i++)
    {
        NpgsqlConnection connection = new NpgsqlConnection(connectionString);
        connection.Open();

        string sql = "SELECT url FROM anotherurls";

        if (lastId > 0)
        {
            sql += " WHERE id > @lastId";
        }

        NpgsqlCommand cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@lastId", lastId);

        using (NpgsqlDataReader reader = cmd.ExecuteReader())
        {
            List<string> AnotherLinks = new List<string>();

            while (reader.Read())
            {
                AnotherLinks.Add(reader[0].ToString());
            }

            await Parse(AnotherLinks);
        }

        string sqlNew = "SELECT count(*) FROM anotherurls";

        using (NpgsqlCommand cmdNew = new NpgsqlCommand(sqlNew, connection))
        {
            using (NpgsqlDataReader readerNew = cmdNew.ExecuteReader())
            {
                if (readerNew.Read())
                {
                    lastId = Convert.ToInt32(readerNew[0]);
                    Console.WriteLine("Сейчас строк в anotherurls: " + lastId);
                }
                else
                {
                    Console.WriteLine("Пустовато.");
                }
            }
        }

        connection.Close();
    }
}


if (choose == "3")
{
    using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
    {
        connection.Open();

        string sql = "SELECT count(*) FROM anotherurls";

        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, connection))
        {
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    Console.WriteLine(reader[0].ToString());
                }
                else
                {
                    Console.WriteLine("Пусто там.");
                }
            }
        }
    }
}

async Task InsertUrlEntity(URLEntity urlEntity)
{
    using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
    {
        con.Open();

        string sql = "INSERT INTO urlandhtml (url, html, text,links) VALUES (@url, @html, @text, @links)";

        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
        {
            cmd.Parameters.AddWithValue("url", urlEntity.URL);
            cmd.Parameters.AddWithValue("html", urlEntity.HTML);
            cmd.Parameters.AddWithValue("text", urlEntity.Text);
            cmd.Parameters.AddWithValue("links", urlEntity.Links);

            try
            {
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                Console.WriteLine("Небольшая заминочка на ссылке: " + urlEntity.URL);
            }

        }
    }
}

async Task InsertAnotherLink(List<string> urls)
{
    using (NpgsqlConnection con = new NpgsqlConnection(connectionString))
    {
        con.Open();

        foreach (string url in urls)
        {
            string sql = "INSERT INTO anotherurls (url) VALUES (@url) ON CONFLICT DO NOTHING ";
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("url", url);
                await cmd.ExecuteNonQueryAsync();
            }
        }

    }
}


async Task Parse(List<string> Urls)
{

    using (var client = new HttpClient())
    {
        foreach (var newUrl in Urls)
        {
            try
            {
                List<string> Links = new List<string>();
                List<string> AnotherLinks = new List<string>();

                URLEntity urlEntity = new URLEntity();

                urlEntity.URL = newUrl;
                string url = newUrl;
                url.Remove((url.Length - 1), 1);
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                HttpResponseMessage response = await client.GetAsync(url);
                string html = await response.Content.ReadAsStringAsync();
                urlEntity.HTML = html;

                string textContent = Regex.Replace(html, "<.*?>", "");

                MatchCollection matches = Regex.Matches(html, @"href=""([^""])+""");

                foreach (Match match in matches)
                {

                    int firstIndex = match.Value.IndexOf('"') + 1;
                    int lastIndex = match.Value.LastIndexOf('"');
                    int Lenght = lastIndex - firstIndex;
                    string newUrlString = match.Value.Substring(firstIndex, Lenght);
                    if (newUrlString[0] == '/')
                    {
                        newUrlString = url + newUrlString;
                    }

                    if (!newUrlString.EndsWith(".png") && !newUrlString.EndsWith(".jpg") && !newUrlString.EndsWith(".ico") && !newUrlString.EndsWith(".js") && !newUrlString.EndsWith(".json") && !newUrlString.EndsWith(".css") && !newUrlString.EndsWith(".htm") && !newUrlString.EndsWith(".htm/") && newUrlString != "0" && !newUrlString.StartsWith("ui-") && !newUrlString.StartsWith("vicon") && !newUrlString.Contains(".css"))
                    {
                        Links.Add(newUrlString);
                        try
                        {
                            Uri currentUri = new Uri(url);
                            Uri linkUri = new Uri(newUrlString);
                            if (currentUri.Host != linkUri.Host && !AnotherLinks.Contains(newUrlString))
                            {
                                AnotherLinks.Add(newUrlString);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                    urlEntity.Links = Links;
                }
                urlEntity.Text = textContent;
                await InsertUrlEntity(urlEntity);
                await InsertAnotherLink(AnotherLinks);
            }
            catch
            {
                Console.WriteLine($"Ошибочка с ссылкой:{newUrl}");
            }
        }
    }
    Console.WriteLine("Done succesfully");
}