using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnAttributeTests
    {
        #region LookupColumnAttribute()

        [Fact]
        public void LookupColumnAttribute_NullPosition()
        {
            Assert.Null(new LookupColumnAttribute().Position);
        }

        [Fact]
        public void LookupColumnAttribute_NullRelation()
        {
            Assert.Null(new LookupColumnAttribute().Relation);
        }

        [Fact]
        public void LookupColumnAttribute_NullFormat()
        {
            Assert.Null(new LookupColumnAttribute().Format);
        }

        #endregion

        #region LookupColumnAttribute(Int32 position)

        [Fact]
        public void LookupColumnAttribute_SetsPosition()
        {
            Int32? actual = new LookupColumnAttribute(-5).Position;
            Int32? expected = -5;

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
