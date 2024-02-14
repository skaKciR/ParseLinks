using Parse.Domain;
using Parse.Service;

namespace Parse
{
    class Program
    {
        public static async Task Main()
        {
            List<string> urls = new List<string>()
             {
                "https://www.interfax.ru/business/",
                "https://www.interfax.ru/culture/",
                "https://www.sport-interfax.ru/",
                "https://www.interfax.ru/russia/",
                "https://www.interfax.ru/story/",
                "https://www.interfax.ru/photo/",
                "https://kuban.rbc.ru/",
                "https://lenta.ru/",
                "https://krasnodarmedia.su/",
                "https://www.kommersant.ru/"
            };

            string connectionString = "Server=localhost; port=5432; user id=postgres; password=sa; database=JanuaryTaskDB;";
            Parser parser = new Parser(new PostgreDbProvider(connectionString));


            Console.WriteLine("1. First parse");
            Console.WriteLine("2. Another links parse");
            Console.WriteLine("3. Clear DB");
            string choose = Console.ReadLine();

            if (choose == "1")
            {
                await parser.Parse(urls);
            }
            else if( choose == "2") 
            {
                await parser.Parse();
            }
            else
            {
               IDbProvider dbProvider = new PostgreDbProvider(connectionString);
               await dbProvider.Truncate();
               Console.ForegroundColor = ConsoleColor.Red;
               Console.WriteLine("База данных успешно очищена.");
               Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
