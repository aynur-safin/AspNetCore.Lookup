using System;
using System.Collections.Generic;

namespace NonFactors.Mvc.Lookup
{
    public abstract class MvcLookup
    {
        public const String Prefix = "Lookup";
        public const String IdKey = "LookupIdKey";
        public const String AcKey = "LookupAcKey";

        public String Url { get; set; }
        public String DialogTitle { get; set; }

        public LookupColumns Columns { get; set; }
        public LookupFilter CurrentFilter { get; set; }
        public IList<String> AdditionalFilters { get; set; }

        public String DefaultSortColumn { get; set; }
        public UInt32 DefaultRecordsPerPage { get; set; }
        public LookupSortOrder DefaultSortOrder { get; set; }

        protected MvcLookup()
        {
            AdditionalFilters = new List<String>();
            CurrentFilter = new LookupFilter();
            Columns = new LookupColumns();
            DefaultRecordsPerPage = 20;
        }

        public abstract LookupData GetData();
    }
}
