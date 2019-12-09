using System;
using System.Collections.Generic;
using System.Text;

namespace SuiQuickRecorderCore.Models
{
    public class SuiKVRecord
    {
        private string id { get; set; }

        public string Id
        {
            get => id;
            set
            {
                id = value.Substring(value.LastIndexOf('_') + 1);
                id = id.Substring(id.LastIndexOf('-') + 1);
            }
        }

        public string[] Alts { get; private set; }

        public string Alt
        {
            get => string.Join("/", Alts);
            set => Alts = value.Split('/');
        }

        public string Name { get; set; }

        public override string ToString() => $"{Id}, {Name}, {Alt}";
    }
}
