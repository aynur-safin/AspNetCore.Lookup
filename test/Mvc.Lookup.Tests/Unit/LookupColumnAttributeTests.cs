using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnAttributeTests
    {
        [Fact]
        public void LookupColumnAttribute_Defaults()
        {
            LookupColumnAttribute actual = new LookupColumnAttribute();

            Assert.Equal(LookupFilterCase.Unspecified, actual.FilterCase);
            Assert.Equal(0, actual.Position);
            Assert.True(actual.Filterable);
        }

        [Fact]
        public void LookupColumnAttribute_PositionDefaults()
        {
            LookupColumnAttribute actual = new LookupColumnAttribute(-5);

            Assert.Equal(LookupFilterCase.Unspecified, actual.FilterCase);
            Assert.Equal(-5, actual.Position);
            Assert.True(actual.Filterable);
        }
    }
}
