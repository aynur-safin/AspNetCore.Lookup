using System.Collections.Generic;
using System.Linq;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class TestLookup<T> : MvcLookup<T> where T : class
    {
        public IList<T> Models { get; set; }

        public TestLookup()
        {
            Filter.Page = 3;
            Filter.Rows = 7;
            Filter.Sort = "Id";
            Models = new List<T>();
            Filter.Search = "Term";
            Title = "Test lookup title";
            Url = "http://localhost/Test";
            AdditionalFilters.Add("Test1");
            AdditionalFilters.Add("Test2");
        }

        public override IQueryable<T> GetModels()
        {
            return Models.AsQueryable();
        }
    }
}
