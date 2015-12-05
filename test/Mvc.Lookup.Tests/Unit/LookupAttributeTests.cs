using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupAttributeTests
    {
        #region Constructor: LookupAttribute(Type type)

        [Fact]
        public void LookupAttribute_NullThrows()
        {
            Assert.Throws<ArgumentNullException>(() => new LookupAttribute(null));
        }

        [Fact]
        public void LookupAttribute_UnassignableTypeThrows()
        {
            Assert.Throws<ArgumentException>(() => new LookupAttribute(typeof(Object)));
        }

        [Fact]
        public void LookupAttribute_SetsType()
        {
            Type actual = new LookupAttribute(typeof(AbstractLookup)).Type;
            Type expected = typeof(AbstractLookup);

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
