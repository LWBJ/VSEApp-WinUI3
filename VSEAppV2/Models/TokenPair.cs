using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public class TokenPair
    {
        [JsonPropertyName("access")]
        public string AccessToken { get; set; }

        [JsonPropertyName("refresh")]
        public string RefreshToken { get; set; }
    }
}
