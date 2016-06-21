using System;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class GuidModel
    {
        [LookupColumn]
        public Guid Id { get; set; }
    }
}
