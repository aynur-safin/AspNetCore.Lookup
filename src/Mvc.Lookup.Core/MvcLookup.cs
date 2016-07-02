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
        public String Title { get; set; }

        public LookupFilter Filter { get; set; }
        public LookupColumns Columns { get; set; }
        public IList<String> AdditionalFilters { get; set; }

        public UInt32 DefaultRows { get; set; }
        public String DefaultSortColumn { get; set; }
        public LookupSortOrder DefaultSortOrder { get; set; }

        protected MvcLookup()
        {
            AdditionalFilters = new List<String>();
            Columns = new LookupColumns();
            Filter = new LookupFilter();
            DefaultRows = 20;
        }

        public abstract LookupData GetData();
    }
}
