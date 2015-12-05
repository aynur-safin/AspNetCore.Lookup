using System;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class NoRelationModel
    {
        [LookupColumn(Relation = "None")]
        public String NoRelation { get; set; }
    }
}
