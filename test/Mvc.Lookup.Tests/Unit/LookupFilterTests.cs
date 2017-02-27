using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupFilterTests
    {
        #region LookupFilter()

        [Fact]
        public void LookupFilter_CreatesEmpty()
        {
            LookupFilter filter = new LookupFilter();

            Assert.Empty(filter.AdditionalFilters);
            Assert.Empty(filter.Selected);
            Assert.Empty(filter.Ids);
        }

        #endregion
    }
}
