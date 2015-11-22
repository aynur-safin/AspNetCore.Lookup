using System;
using System.Collections.Generic;

namespace NonFactors.Mvc.Lookup
{
    public class LookupFilter
    {
        public String Id { get; set; }
        public Int32 Page { get; set; }
        public String SearchTerm { get; set; }
        public String SortColumn { get; set; }
        public Int32 RecordsPerPage { get; set; }
        public LookupSortOrder SortOrder { get; set; }

        public IDictionary<String, Object> AdditionalFilters { get; set; }

        public LookupFilter()
        {
            AdditionalFilters = new Dictionary<String, Object>();
        }
    }
}
