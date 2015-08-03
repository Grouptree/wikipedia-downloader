using System;
using System.Threading.Tasks;

namespace Wikimedia
{
    public interface IHttpClient
    {
        Task<string> Get(string url, string userAgent);
    }
}
