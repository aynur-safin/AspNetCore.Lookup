using System;
using System.Collections.Generic;

namespace NonFactors.Mvc.Lookup
{
    public class LookupData
    {
        public Int32 FilteredRows { get; set; }
        public LookupColumns Columns { get; set; }
        public List<Dictionary<String, String>> Rows { get; set; }

        public LookupData()
        {
            Columns = new LookupColumns();
            Rows = new List<Dictionary<String, String>>();
        }
    }
}