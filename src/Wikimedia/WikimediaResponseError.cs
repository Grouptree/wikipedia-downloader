using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Wikimedia
{
    public class WikimediaResponseError
    {
        static readonly Regex SecondsLaggedPattern = new Regex(@"\s(\d+)\sseconds\slagged", RegexOptions.Compiled);

        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }

        [JsonProperty("*")]
        public string SeeAlso { get; set; }

        public bool IsMaxLag => Code == "maxlag";

        public int SecondsLagged
        {
            get
            {
                if (!IsMaxLag)
                    throw new InvalidOperationException("Not a maxlag error");
                var match = SecondsLaggedPattern.Match(Info ?? "");
                if (match.Success)
                    return int.Parse(match.Groups[1].Value);
                return 0;
            }
        }
    }
}
