using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public class VSEUser: ObservableObject
    {
        private string username;
        [JsonPropertyName("username")]
        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        private string url;
        [JsonPropertyName("url")]
        public string Url
        {
            get => url;
            set => SetProperty(ref url, value);
        }
    }
}
