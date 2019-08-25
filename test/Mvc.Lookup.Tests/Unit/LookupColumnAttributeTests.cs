using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnAttributeTests
    {
        [Fact]
        public void LookupColumnAttribute()
        {
            LookupColumnAttribute attribute = new LookupColumnAttribute();

            Assert.True(attribute.Filterable);
        }

        [Fact]
        public void LookupColumnAttribute_Position()
        {
            LookupColumnAttribute attribute = new LookupColumnAttribute(-5);

            Assert.Equal(-5, attribute.Position);
            Assert.True(attribute.Filterable);
        }
    }
}
