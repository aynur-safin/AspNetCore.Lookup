using System.Collections.Generic;
using System.Linq;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class TestLookup<T> : MvcLookup<T> where T : class
    {
        public IList<T> Models { get; set; }

        public TestLookup()
        {
            DefaultRows = 7;
            Filter.Rows = 10;
            Models = new List<T>();
            Filter.Search = "Look up";
            Title = "Test lookup title";
            DefaultSortColumn = "SortCol";
            Url = "http://localhost/Test";
            AdditionalFilters.Add("Test1");
            AdditionalFilters.Add("Test2");
            DefaultSortOrder = LookupSortOrder.Asc;
        }

        public override IQueryable<T> GetModels()
        {
            return Models.AsQueryable();
        }
    }
}
