using NonFactors.Mvc.Lookup;
using System;
using Xunit;

namespace Mvc.Lookup.Tests.Unit
{
    public class LookupDataTests
    {
        #region Constructor: LookupData()

        [Fact]
        public void LookupData_ZeroFilteredRecords()
        {
            Int32 actual = new LookupData().FilteredRecords;
            Int32 expected = 0;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LookupData_EmptyColumns()
        {
            Assert.Empty(new LookupData().Columns);
        }

        [Fact]
        public void LookupData_EmptyRows()
        {
            Assert.Empty(new LookupData().Rows);
        }

        #endregion
    }
}
