using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VSEAppV2.Models
{
    public class VSEExperience: ObservableObject
    {
        private string name;
        private List<string> valueList;
        private List<string> valueURLList;

        private List<string> skillList;
        private List<string> skillURLList;

        private string url;

        [JsonPropertyName("name")]
        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        [JsonPropertyName("value_names")]
        public List<string> ValueList
        {
            get => valueList;
            set => SetProperty(ref valueList, value);
        }
        [JsonPropertyName("value_set")]
        public List<string> ValueURLList
        {
            get => valueURLList;
            set => SetProperty(ref valueURLList, value);
        }

        [JsonPropertyName("skill_names")]
        public List<string> SkillList
        {
            get => skillList;
            set => SetProperty(ref skillList, value);
        }
        [JsonPropertyName("skill_set")]
        public List<string> SkillURLList
        {
            get => skillURLList;
            set => SetProperty(ref skillURLList, value);
        }

        [JsonPropertyName("url")]
        public string Url
        {
            get => url;
            set => SetProperty(ref url, value);
        }
    }
}
