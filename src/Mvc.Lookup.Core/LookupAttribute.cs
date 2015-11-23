using System;

namespace NonFactors.Mvc.Lookup
{
    public class LookupAttribute : Attribute
    {
        public Type Type { get; protected set; }

        public LookupAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (!typeof(AbstractLookup).IsAssignableFrom(type))
                throw new ArgumentException($"'{type.Name}' type does not implement '{typeof(AbstractLookup).Name}'.");

            Type = type;
        }
    }
}
