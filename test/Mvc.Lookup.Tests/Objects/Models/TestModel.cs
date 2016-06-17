using System;
using System.ComponentModel.DataAnnotations;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class TestModel
    {
        [LookupColumn(0)]
        public String Id { get; set; }

        [LookupColumn]
        [Display(Name = "TestDisplay")]
        public Int32 Number { get; set; }

        [Lookup(typeof(TestLookupProxy))]
        public String ParentId { get; set; }

        [LookupColumn(-5, Format = "{0:d}")]
        public DateTime CreationDate { get; set; }

        public Decimal Sum { get; set; }
        public String NullableString { get; set; }

        public String RelationId { get; set; }
        public virtual TestRelationModel Relation { get; set; }
    }
}
