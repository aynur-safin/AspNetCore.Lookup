using System;
using System.ComponentModel.DataAnnotations;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class Int32Model
    {
        [Key]
        [LookupColumn]
        public Int32 Value { get; set; }
    }
}
