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

        public String FirstRelationModelId { get; set; }
        public String SecondRelationModelId { get; set; }

        [LookupColumn(1, Relation = "Value")]
        public virtual TestRelationModel FirstRelationModel { get; set; }

        [LookupColumn(1, Relation = "NoValue")]
        public virtual TestRelationModel SecondRelationModel { get; set; }
    }
}
