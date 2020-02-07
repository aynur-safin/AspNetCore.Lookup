using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using NonFactors.Mvc.Lookup.Tests.Objects;
using NSubstitute;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupTests
    {
        private MvcLookup lookup;

        public MvcLookupTests()
        {
            lookup = Substitute.ForPartsOf<MvcLookup>();
        }

        [Fact]
        public void MvcLookup_Defaults()
        {
            MvcLookup actual = Substitute.For<MvcLookup>();

            Assert.Equal(LookupFilterPredicate.Contains, actual.FilterPredicate);
            Assert.Equal(LookupFilterCase.Lower, actual.FilterCase);
            Assert.Empty(actual.AdditionalFilters);
            Assert.NotNull(actual.Filter);
            Assert.Empty(actual.Columns);
        }

        [Fact]
        public void GetColumnKey_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => lookup.GetColumnKey(null!));

            Assert.Equal("property", actual.ParamName);
        }

        [Fact]
        public void GetColumnKey_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Count))!;

            String actual = lookup.GetColumnKey(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_NullProperty_ReturnsEmpty()
        {
            Assert.Empty(lookup.GetColumnHeader(null!));
        }

        [Fact]
        public void GetColumnHeader_NoDisplayName_ReturnsEmpty()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Value))!;

            Assert.Empty(lookup.GetColumnHeader(property));
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Date))!;

            String? expected = property.GetCustomAttribute<DisplayAttribute>()?.Name;
            String? actual = lookup.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayShortName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty(nameof(TestModel.Count))!;

            String? expected = property.GetCustomAttribute<DisplayAttribute>()?.ShortName;
            String? actual = lookup.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnCssClass_ReturnsEmpty()
        {
            Assert.Empty(lookup.GetColumnCssClass(null!));
        }
    }
}
