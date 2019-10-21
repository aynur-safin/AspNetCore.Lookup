using System;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class NoIdModel
    {
        [LookupColumn]
        public String? Title { get; set; }
    }
}
