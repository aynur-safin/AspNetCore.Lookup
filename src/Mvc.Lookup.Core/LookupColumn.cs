using System;

namespace NonFactors.Mvc.Lookup
{
    public class LookupColumn
    {
        public String Name { get; protected set; }
        public String Header { get; protected set; }
        public String CssClass { get; protected set; }

        public LookupColumn(String name, String header, String cssClass = "")
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (header == null)
                throw new ArgumentNullException(nameof(header));

            if (cssClass == null)
                throw new ArgumentNullException(nameof(cssClass));

            Name = name;
            Header = header;
            CssClass = cssClass;
        }
    }
}
