using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SuiQuickRecorderCore.Models.KeyValue
{
    public class SuiKVPairs
    {
        private Dictionary<string, string> kvMap = new Dictionary<string, string>();

        public SuiKVPairs(string path)
        {
            TextReader textReader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            CsvReader csvReader = new CsvReader(textReader);

            foreach (var pair in csvReader.GetRecords<SuiKVRecord>())
            {
                AddKVPair(pair.Name, pair.Id);
                foreach (var altValue in pair.Alts.Where(x => !string.IsNullOrEmpty(x)))
                {
                    AddKVPair(altValue, pair.Id);
                }
            }

            textReader.Close();
        }

        private void AddKVPair(string key, string value)
        {
            if (kvMap.ContainsKey(key))
            {
                throw new ArgumentException($"The key / alt key {key} has been added in the collection");
            }
            else
            {
                kvMap.Add(key, value);
            }
        }

        public string this[string name]
        {
            get => kvMap[name];
        }

        public string GetOfficialName(string name) => kvMap.First(x => x.Value == this[name]).Key;

        public bool Contains(string name) => kvMap.ContainsKey(name);
    }
}
