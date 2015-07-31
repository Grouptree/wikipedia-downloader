using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WikipediaDownloader
{
    public interface IHttp
    {
        Task<string> Get(string url);
    }
}
