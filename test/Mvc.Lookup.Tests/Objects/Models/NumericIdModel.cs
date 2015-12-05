using System;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class NumericIdModel
    {
        [LookupColumn]
        public Decimal Id { get; set; }
    }
}
