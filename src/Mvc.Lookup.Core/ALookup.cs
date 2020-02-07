using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace NonFactors.Mvc.Lookup
{
    public abstract class ALookup
    {
        public String? Url { get; set; }
        public String? Name { get; set; }
        public String? Title { get; set; }
        public Boolean Multi { get; set; }
        public String? Dialog { get; set; }
        public Boolean ReadOnly { get; set; }
        public String? Placeholder { get; set; }
        public LookupFilterCase FilterCase { get; set; }
        public LookupFilterMethod FilterMethod { get; set; }

        public LookupFilter Filter { get; set; }
        public IList<LookupColumn> Columns { get; set; }
        public IList<String> AdditionalFilters { get; set; }

        protected ALookup()
        {
            FilterMethod = LookupFilterMethod.Contains;
            AdditionalFilters = new List<String>();
            FilterCase = LookupFilterCase.Lower;
            Columns = new List<LookupColumn>();
            Filter = new LookupFilter();
        }

        public virtual String GetColumnKey(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            return property.Name;
        }
        public virtual String GetColumnHeader(PropertyInfo property)
        {
            return property?.GetCustomAttribute<DisplayAttribute>(false)?.GetShortName() ?? "";
        }
        public virtual String GetColumnCssClass(PropertyInfo property)
        {
            return "";
        }

        public abstract LookupData GetData();
    }
}
