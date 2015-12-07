using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupDataTests
    {
        #region Constructor: LookupData()

        [Fact]
        public void LookupData_SetsFilteredRecords()
        {
            Int32 actual = new LookupData().FilteredRecords;
            Int32 expected = 0;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupData_SetsColumns()
        {
            Assert.Empty(new LookupData().Columns);
        }

        [Fact]
        public void LookupData_SetsRows()
        {
            Assert.Empty(new LookupData().Rows);
        }

        #endregion
    }
}
