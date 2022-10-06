using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public class VSESkill: ObservableObject
    {
        private string name;
        private List<string> experienceList;
        private List<string> experienceURLList;
        private string url;

        [JsonPropertyName("name")]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        [JsonPropertyName("experience_names")]
        public List<string> ExperienceList
        {
            get => experienceList;
            set => SetProperty(ref experienceList, value);
        }

        [JsonPropertyName("experiences")]
        public List<string> ExperienceURLList
        {
            get => experienceURLList;
            set => SetProperty(ref experienceURLList, value);
        }

        [JsonPropertyName("url")]
        public string Url
        {
            get => url;
            set => SetProperty(ref url, value);
        }
    }
}
