using System;

namespace NonFactors.Mvc.Lookup
{
    public class LookupAttribute : Attribute
    {
        public Type Type { get; protected set; }

        public LookupAttribute(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!typeof(AbstractLookup).IsAssignableFrom(type))
                throw new ArgumentException(
                    String.Format("'{0}' type does not implement '{1}'.", type.Name, typeof(AbstractLookup).Name));

            Type = type;
        }
    }
}
