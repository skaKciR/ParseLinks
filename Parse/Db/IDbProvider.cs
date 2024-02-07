using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.Domain
{
    internal interface IDbProvider
    {
        Task<string> GetAnotherUrlsCount();

        Task InsertUrlEntity(URLEntity urlEntity);

        Task<List<Robots>> InsertRobots(Robots robots);

        Task<List<Robots>> GetRobots();

        Task InsertAnotherLink(List<string> urls);

        Task ClearAnotherUrls();

        Task<List<string>> GetAnotherUrls();

        Task DeleteAnotherUrl(string url);

        Task<bool> Contains(string url);
    }
}
