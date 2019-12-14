using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnTests
    {
        [Fact]
        public void LookupColumn_NullKey_Throws()
        {
            Assert.Equal("key", Assert.Throws<ArgumentNullException>(() => new LookupColumn(null!, "Test")).ParamName);
        }

        [Fact]
        public void LookupColumn_Defaults()
        {
            LookupColumn actual = new LookupColumn("Test", "Headers");

            Assert.Equal(LookupFilterPredicate.Unspecified, actual.FilterPredicate);
            Assert.Equal(LookupFilterCase.Unspecified, actual.FilterCase);
            Assert.Equal("Headers", actual.Header);
            Assert.Equal("Test", actual.Key);
            Assert.Empty(actual.CssClass);
            Assert.False(actual.Hidden);
        }
    }
}
