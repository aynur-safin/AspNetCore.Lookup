using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupFilterTests
    {
        private LookupFilter filter;

        public LookupFilterTests()
        {
            filter = new LookupFilter();
        }

        #region Constructor: LookupFilter()

        [Fact]
        public void LookupFilter_SetsId()
        {
            Assert.Null(filter.Id);
        }

        [Fact]
        public void LookupFilter_SetsPage()
        {
            Assert.Equal(0, filter.Page);
        }

        [Fact]
        public void LookupFilter_SetsSearchTerm()
        {
            Assert.Null(filter.SearchTerm);
        }

        [Fact]
        public void LookupFilter_SetsSortColumn()
        {
            Assert.Null(filter.SortColumn);
        }

        [Fact]
        public void LookupFilter_SetsSortOrder()
        {
            Assert.Equal(LookupSortOrder.Asc, filter.SortOrder);
        }

        [Fact]
        public void LookupFilter_SetsRecordsPerPage()
        {
            Assert.Equal(0, filter.RecordsPerPage);
        }

        [Fact]
        public void LookupFilter_SetsAdditionalFilters()
        {
            Assert.Empty(filter.AdditionalFilters);
        }

        #endregion
    }
}
