using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupDataTests
    {
        #region LookupData()

        [Fact]
        public void LookupData_CreatesEmpty()
        {
            LookupData actual = new LookupData();

            Assert.Equal(0, actual.FilteredRecords);
            Assert.Empty(actual.Columns);
            Assert.Empty(actual.Rows);
        }

        #endregion
    }
}
