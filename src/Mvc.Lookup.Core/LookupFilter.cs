using System;
using System.Collections.Generic;

namespace NonFactors.Mvc.Lookup
{
    public class LookupFilter
    {
        public String Id { get; set; }

        public Int32 Page { get; set; }
        public Int32 Rows { get; set; }
        public String Search { get; set; }

        public String Sort { get; set; }
        public LookupSortOrder Order { get; set; }

        public IDictionary<String, Object> AdditionalFilters { get; set; }

        public LookupFilter()
        {
            AdditionalFilters = new Dictionary<String, Object>();
        }
    }
}
