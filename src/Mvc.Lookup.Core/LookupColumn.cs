using System;

namespace NonFactors.Mvc.Lookup
{
    public class LookupColumn
    {
        public String Key { get; }
        public String Header { get; set; }
        public String CssClass { get; set; }

        public LookupColumn(String key, String header)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            Key = key;
            Header = header;
        }
    }
}
