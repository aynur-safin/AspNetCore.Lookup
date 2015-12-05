using System;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class NonNumericIdModel
    {
        [LookupColumn]
        public Guid Id { get; set; }
    }
}
