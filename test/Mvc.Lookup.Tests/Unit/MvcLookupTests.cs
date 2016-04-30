using Moq;
using System;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupTests
    {
        private MvcLookup lookup;

        public MvcLookupTests()
        {
            lookup = new Mock<MvcLookup>().Object;
        }

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
        public void MvcLookup_SetsDialogTitle()
        {
            Assert.Null(lookup.DialogTitle);
        }

        [Fact]
        public void MvcLookup_SetsUrl()
        {
            Assert.Null(lookup.Url);
        }

        [Fact]
        public void MvcLookup_SetsDefaultSortColumn()
        {
            Assert.Null(lookup.DefaultSortColumn);
        }

        [Fact]
        public void MvcLookup_SetsDefaultRecordsPerPage()
        {
            UInt32 actual = lookup.DefaultRecordsPerPage;
            UInt32 expected = 20;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MvcLookup_SetsAdditionalFilters()
        {
            Assert.Empty(lookup.AdditionalFilters);
        }

        [Fact]
        public void MvcLookup_SetsDefaultSortOrder()
        {
            LookupSortOrder actual = lookup.DefaultSortOrder;
            LookupSortOrder expected = LookupSortOrder.Asc;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MvcLookup_SetsColumns()
        {
            Assert.Empty(lookup.Columns);
        }

        [Fact]
        public void MvcLookup_SetsCurrentFilter()
        {
            Assert.NotNull(lookup.CurrentFilter);
        }

        #endregion
    }
}
