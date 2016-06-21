using System.Collections.Generic;
using System.Linq;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class TestLookup<T> : MvcLookup<T> where T : class
    {
        public IList<T> Models { get; set; }

        public TestLookup()
        {
            Models = new List<T>();
            DefaultRecordsPerPage = 7;
            DialogTitle = "Test title";
            DefaultSortColumn = "SortCol";
            Url = "http://localhost/Test";
            AdditionalFilters.Add("Test1");
            AdditionalFilters.Add("Test2");
            CurrentFilter.RecordsPerPage = 10;
            CurrentFilter.SearchTerm = "Look up";
            DefaultSortOrder = LookupSortOrder.Asc;
        }

        public override IQueryable<T> GetModels()
        {
            return Models.AsQueryable();
        }
    }
}
