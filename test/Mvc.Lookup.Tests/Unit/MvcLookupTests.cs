using NSubstitute;
using System.Reflection;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupTests
    {
        #region MvcLookup()

        [Fact]
        public void MvcLookup_Defaults()
        {
            MvcLookup actual = Substitute.For<MvcLookup>();

            Assert.Equal("MvcLookupDialog", actual.Dialog);
            Assert.Empty(actual.AdditionalFilters);
            Assert.Equal(20, actual.Filter.Rows);
            Assert.Empty(actual.Columns);
        }

        #endregion
    }
}
