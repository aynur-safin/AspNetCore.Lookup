using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupFilterTests
    {
        #region LookupFilter()

        [Fact]
        public void LookupFilter_CreatesEmpty()
        {
            Assert.Empty(new LookupFilter().AdditionalFilters);
        }

        #endregion
    }
}
