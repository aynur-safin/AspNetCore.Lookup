using System;
using System.ComponentModel.DataAnnotations;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class NumericModel
    {
        [Key]
        [LookupColumn]
        public Int32 Value { get; set; }
    }
}
