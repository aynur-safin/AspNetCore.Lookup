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

            Assert.Empty(actual.AdditionalFilters);
            Assert.NotNull(actual.Filter);
            Assert.Empty(actual.Columns);
        }

        #endregion
    }
}
