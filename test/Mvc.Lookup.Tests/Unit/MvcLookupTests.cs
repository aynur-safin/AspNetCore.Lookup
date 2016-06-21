using NSubstitute;
using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupTests
    {
        #region Constants

        [Fact]
        public void Prefix_IsConstant()
        {
            Assert.True(typeof(MvcLookup).GetField("Prefix").IsLiteral);
            Assert.Equal("Lookup", MvcLookup.Prefix);
        }

        [Fact]
        public void IdKey_IsConstant()
        {
            Assert.True(typeof(MvcLookup).GetField("IdKey").IsLiteral);
            Assert.Equal("LookupIdKey", MvcLookup.IdKey);
        }

        [Fact]
        public void AcKey_IsConstant()
        {
            Assert.True(typeof(MvcLookup).GetField("AcKey").IsLiteral);
            Assert.Equal("LookupAcKey", MvcLookup.AcKey);
        }

        #endregion

        #region MvcLookup()

        [Fact]
        public void MvcLookup_Defaults()
        {
            MvcLookup actual = Substitute.For<MvcLookup>();

            Assert.Equal<UInt32>(20, actual.DefaultRecordsPerPage);
            Assert.Empty(actual.AdditionalFilters);
            Assert.NotNull(actual.CurrentFilter);
            Assert.Empty(actual.Columns);
        }

        #endregion
    }
}
