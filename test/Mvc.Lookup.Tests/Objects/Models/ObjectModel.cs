using System;
using System.ComponentModel.DataAnnotations;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class ObjectModel
    {
        [Key]
        public Object? Id { get; set; }
    }
}
