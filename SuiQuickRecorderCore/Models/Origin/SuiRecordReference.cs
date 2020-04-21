using SuiQuickRecorderCore.Models.KeyValue;

namespace SuiQuickRecorderCore.Models.Origin
{
    public class SuiRecordReference
    {
        public SuiKVPairs Accounts { get; private set; }
        public SuiKVPairs CategoriesIn { get; private set; }
        public SuiKVPairs CategoriesOut { get; private set; }
        public SuiKVPairs Stores { get; private set; }
        public SuiKVPairs Loaners { get; private set; }

        public SuiRecordReference(SuiKVPairs accounts, SuiKVPairs categoriesIn, SuiKVPairs categoriesOut, SuiKVPairs stores, SuiKVPairs loaners)
        {
            Accounts = accounts;
            CategoriesIn = categoriesIn;
            CategoriesOut = categoriesOut;
            Stores = stores;
            Loaners = loaners;
        }
    }
}
