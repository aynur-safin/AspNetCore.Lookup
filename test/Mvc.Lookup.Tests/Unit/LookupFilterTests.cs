using NonFactors.Mvc.Lookup;
using System;
using Xunit;

namespace Mvc.Lookup.Tests.Unit
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
        public void LookupFilter_NullId()
        {
            Assert.Null(filter.Id);
        }

        [Fact]
        public void LookupFilter_ZeroPage()
        {
            Assert.Equal(0, filter.Page);
        }

        [Fact]
        public void LookupFilter_NullSearchTerm()
        {
            Assert.Null(filter.SearchTerm);
        }

        [Fact]
        public void LookupFilter_NullSortColumn()
        {
            Assert.Null(filter.SortColumn);
        }

        [Fact]
        public void LookupFilter_AscSortOrder()
        {
            Assert.Equal(LookupSortOrder.Asc, filter.SortOrder);
        }

        [Fact]
        public void LookupFilter_ZeroRecordsPerPage()
        {
            Assert.Equal(0, filter.RecordsPerPage);
        }

        [Fact]
        public void LookupFilter_EmptyAdditionalFilters()
        {
            Assert.Empty(filter.AdditionalFilters);
        }

        #endregion
    }
}
