using System;

namespace NonFactors.Mvc.Lookup
{
    public class LookupColumnAttribute : Attribute
    {
        public LookupFilterMethod? FilterMethod { get; set; }
        public LookupFilterCase? FilterCase { get; set; }
        public Boolean Filterable { get; set; }
        public Int32 Position { get; set; }
        public Boolean Hidden { get; set; }
        public String? Format { get; set; }

        public LookupColumnAttribute()
            : this(0)
        {
        }
        public LookupColumnAttribute(Int32 position)
        {
            Filterable = true;
            Position = position;
        }
    }
}
