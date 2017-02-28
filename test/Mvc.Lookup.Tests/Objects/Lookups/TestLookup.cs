using System.Collections.Generic;
using System.Linq;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class TestLookup<T> : MvcLookup<T> where T : class
    {
        public IList<T> Models { get; }

        public TestLookup()
        {
            Models = new List<T>();
        }

        public override IQueryable<T> GetModels()
        {
            return Models.AsQueryable();
        }
    }
}
