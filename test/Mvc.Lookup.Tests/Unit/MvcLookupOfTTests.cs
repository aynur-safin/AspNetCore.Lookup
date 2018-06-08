using NonFactors.Mvc.Lookup.Tests.Objects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class MvcLookupOfTTests
    {
        private TestLookup<TestModel> lookup;

        public MvcLookupOfTTests()
        {
            lookup = new TestLookup<TestModel>();
            lookup.Filter.Page = 0;

            for (Int32 i = 0; i < 200; i++)
                lookup.Models.Add(new TestModel
                {
                    Id = i + "I",
                    Count = i + 10,
                    Value = i + "V",
                    ParentId = "1000",
                    Date = new DateTime(2014, 12, 10).AddDays(i)
                });
        }

        #region GetId

        [Fact]
        public void GetId_NoProperty()
        {
            Assert.Null(new TestLookup<NoIdModel>().GetId(new NoIdModel()));
        }

        [Fact]
        public void GetId_Value()
        {
            String actual = lookup.GetId(new TestModel { Id = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetLabel

        [Fact]
        public void GetLabel_NoColumns()
        {
            lookup.Columns.Clear();

            Assert.Null(lookup.GetLabel(new TestModel()));
        }

        [Fact]
        public void GetLabel_Value()
        {
            String actual = lookup.GetLabel(new TestModel { Value = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        #endregion

        #region AttributedProperties

        [Fact]
        public void AttributedProperties_GetsOrderedProperties()
        {
            IEnumerable<PropertyInfo> actual = lookup.AttributedProperties;
            IEnumerable<PropertyInfo> expected = typeof(TestModel).GetProperties()
                .Where(property => property.GetCustomAttribute<LookupColumnAttribute>(false) != null)
                .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false).Position);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region MvcLookup()

        [Fact]
        public void MvcLookup_AddsColumns()
        {
            List<LookupColumn> columns = new List<LookupColumn>();
            columns.Add(new LookupColumn("Value", null) { Hidden = false, Filterable = true });
            columns.Add(new LookupColumn("Date", "Date") { Hidden = false, Filterable = true });
            columns.Add(new LookupColumn("Count", "Value") { Hidden = false, Filterable = false });

            IEnumerator<LookupColumn> expected = columns.GetEnumerator();
            IEnumerator<LookupColumn> actual = lookup.Columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Key, actual.Current.Key);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.Hidden, actual.Current.Hidden);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
                Assert.Equal(expected.Current.Filterable, actual.Current.Filterable);
            }
        }

        #endregion

        #region GetColumnKey(PropertyInfo property)

        [Fact]
        public void GetColumnKey_NullProperty_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => lookup.GetColumnKey(null));

            Assert.Equal("property", actual.ParamName);
        }

        [Fact]
        public void GetColumnKey_ReturnsPropertyName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Count");

            String actual = lookup.GetColumnKey(property);
            String expected = property.Name;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetColumnHeader(PropertyInfo property)

        [Fact]
        public void GetColumnHeader_NullProperty_ReturnsNull()
        {
            Assert.Null(lookup.GetColumnHeader(null));
        }

        [Fact]
        public void GetColumnHeader_NoDisplayName_ReturnsNull()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Value");

            Assert.Null(lookup.GetColumnHeader(property));
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Date");

            String expected = property.GetCustomAttribute<DisplayAttribute>().Name;
            String actual = lookup.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetColumnHeader_ReturnsDisplayShortName()
        {
            PropertyInfo property = typeof(TestModel).GetProperty("Count");

            String expected = property.GetCustomAttribute<DisplayAttribute>().ShortName;
            String actual = lookup.GetColumnHeader(property);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetColumnCssClass(PropertyInfo property)

        [Fact]
        public void GetColumnCssClass_ReturnsNull()
        {
            Assert.Null(lookup.GetColumnCssClass(null));
        }

        #endregion

        #region GetData()

        [Fact]
        public void GetData_FiltersByIds()
        {
            lookup.Filter.Ids.Add("9I");
            lookup.Filter.Ids.Add("15I");
            lookup.Filter.Sort = "Count";
            lookup.Filter.Search = "Term";
            lookup.Filter.Selected.Add("17I");
            lookup.Filter.AdditionalFilters.Add("Value", "5V");

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("9V", actual.Rows[0]["Label"]);
            Assert.Equal("9V", actual.Rows[0]["Value"]);
            Assert.Equal("19", actual.Rows[0]["Count"]);
            Assert.Equal("9I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("15V", actual.Rows[1]["Label"]);
            Assert.Equal("15V", actual.Rows[1]["Value"]);
            Assert.Equal("25", actual.Rows[1]["Count"]);
            Assert.Equal("15I", actual.Rows[1]["Id"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.FilteredRows);
            Assert.Equal(2, actual.Rows.Count);
        }

        [Fact]
        public void GetData_FiltersByCheckIds()
        {
            lookup.Filter.Sort = "Count";
            lookup.Filter.CheckIds.Add("9I");
            lookup.Filter.CheckIds.Add("15I");
            lookup.Filter.AdditionalFilters.Add("ParentId", "1000");

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 19).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("9V", actual.Rows[0]["Label"]);
            Assert.Equal("9V", actual.Rows[0]["Value"]);
            Assert.Equal("19", actual.Rows[0]["Count"]);
            Assert.Equal("9I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("15V", actual.Rows[1]["Label"]);
            Assert.Equal("15V", actual.Rows[1]["Value"]);
            Assert.Equal("25", actual.Rows[1]["Count"]);
            Assert.Equal("15I", actual.Rows[1]["Id"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.FilteredRows);
            Assert.Equal(2, actual.Rows.Count);
        }

        [Fact]
        public void GetData_FiltersByNotSelected()
        {
            lookup.Filter.Sort = "Count";
            lookup.Filter.Search = "133V";
            lookup.Filter.Selected.Add("15I");
            lookup.Filter.Selected.Add("125I");

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("15V", actual.Rows[0]["Label"]);
            Assert.Equal("15V", actual.Rows[0]["Value"]);
            Assert.Equal("25", actual.Rows[0]["Count"]);
            Assert.Equal("15I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 14).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("125V", actual.Rows[1]["Label"]);
            Assert.Equal("125V", actual.Rows[1]["Value"]);
            Assert.Equal("135", actual.Rows[1]["Count"]);
            Assert.Equal("125I", actual.Rows[1]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 22).ToString("d"), actual.Rows[2]["Date"]);
            Assert.Equal("133V", actual.Rows[2]["Label"]);
            Assert.Equal("133V", actual.Rows[2]["Value"]);
            Assert.Equal("143", actual.Rows[2]["Count"]);
            Assert.Equal("133I", actual.Rows[2]["Id"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(1, actual.FilteredRows);
            Assert.Equal(3, actual.Rows.Count);
        }

        [Fact]
        public void GetData_FiltersByAdditionalFilters()
        {
            lookup.Filter.Search = "6V";
            lookup.Filter.AdditionalFilters.Add("Count", 16);

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2014, 12, 16).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("6V", actual.Rows[0]["Label"]);
            Assert.Equal("6V", actual.Rows[0]["Value"]);
            Assert.Equal("16", actual.Rows[0]["Count"]);
            Assert.Equal("6I", actual.Rows[0]["Id"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(1, actual.FilteredRows);
            Assert.Single(actual.Rows);
        }

        [Fact]
        public void GetData_FiltersBySearch()
        {
            lookup.Filter.Search = "33V";
            lookup.Filter.Sort = "Count";

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2015, 1, 12).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("33V", actual.Rows[0]["Label"]);
            Assert.Equal("33V", actual.Rows[0]["Value"]);
            Assert.Equal("43", actual.Rows[0]["Count"]);
            Assert.Equal("33I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 22).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("133V", actual.Rows[1]["Label"]);
            Assert.Equal("133V", actual.Rows[1]["Value"]);
            Assert.Equal("143", actual.Rows[1]["Count"]);
            Assert.Equal("133I", actual.Rows[1]["Id"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.FilteredRows);
            Assert.Equal(2, actual.Rows.Count);
        }

        [Fact]
        public void GetData_Sorts()
        {
            lookup.Filter.Order = LookupSortOrder.Asc;
            lookup.Filter.Search = "55V";
            lookup.Filter.Sort = "Count";

            lookup.GetData();

            LookupData actual = lookup.GetData();

            Assert.Equal(new DateTime(2015, 2, 3).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("55V", actual.Rows[0]["Label"]);
            Assert.Equal("55V", actual.Rows[0]["Value"]);
            Assert.Equal("65", actual.Rows[0]["Count"]);
            Assert.Equal("55I", actual.Rows[0]["Id"]);

            Assert.Equal(new DateTime(2015, 5, 14).ToString("d"), actual.Rows[1]["Date"]);
            Assert.Equal("155V", actual.Rows[1]["Label"]);
            Assert.Equal("155V", actual.Rows[1]["Value"]);
            Assert.Equal("165", actual.Rows[1]["Count"]);
            Assert.Equal("155I", actual.Rows[1]["Id"]);
        }

        #endregion

        #region FilterBySearch(IQueryable<T> models)

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void FilterBySearch_SkipsEmptySearch(String search)
        {
            lookup.Filter.Search = search;

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotExistingProperties()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn("Test", "Test"));

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_UsesContainsSearch()
        {
            lookup.Filter.Search = "1";

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Id.Contains("1"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNonStringProperties()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn("Count", null));

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotFilterableColumns()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn("Value", null) { Filterable = false });

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterByAdditionalFilters(IQueryable<T> models)

        [Fact]
        public void FilterByAdditionalFilters_SkipsNullValues()
        {
            lookup.Filter.AdditionalFilters.Add("Id", null);

            IQueryable<TestModel> actual = lookup.FilterByAdditionalFilters(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByAdditionalFilters_Filters()
        {
            lookup.Filter.AdditionalFilters.Add("Id", "9I");
            lookup.Filter.AdditionalFilters.Add("Count", new[] { 19, 30 });
            lookup.Filter.AdditionalFilters.Add("Date", new DateTime(2014, 12, 19));

            IQueryable<TestModel> actual = lookup.FilterByAdditionalFilters(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels().Where(model =>
                model.Id == "9I" && model.Count == 19 && model.Date == new DateTime(2014, 12, 19));

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterByIds(IQueryable<T> models, IList<String> ids)

        [Fact]
        public void FilterByIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByIds(null, null));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NumberKey()
        {
            TestLookup<NumericModel> testLookup = new TestLookup<NumericModel>();
            for (Int32 i = 0; i < 20; i++) testLookup.Models.Add(new NumericModel { Value = i });

            IQueryable<NumericModel> actual = testLookup.FilterByIds(testLookup.GetModels(), new List<String> { "9.0", "10" });
            IQueryable<NumericModel> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<GuidModel>().FilterByIds(null, new String[0]));

            String expected = $"'{typeof(GuidModel).Name}.Id' property type has to be a string or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterByNotIds(IQueryable<T> models, IList<String> ids)

        [Fact]
        public void FilterByNotIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByNotIds(null, null));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByNotIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => !ids.Contains(model.Id));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NumberKey()
        {
            TestLookup<NumericModel> testLookup = new TestLookup<NumericModel>();
            for (Int32 i = 0; i < 20; i++) testLookup.Models.Add(new NumericModel { Value = i });

            IQueryable<NumericModel> actual = testLookup.FilterByNotIds(testLookup.GetModels(), new List<String> { "9.0", "10" });
            IQueryable<NumericModel> expected = testLookup.GetModels().Where(model => !new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<GuidModel>().FilterByNotIds(null, new String[0]));

            String expected = $"'{typeof(GuidModel).Name}.Id' property type has to be a string or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterByCheckIds(IQueryable<T> models, IList<String> ids)

        [Fact]
        public void FilterByCheckIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByCheckIds(null, null));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByCheckIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NumberKey()
        {
            TestLookup<NumericModel> testLookup = new TestLookup<NumericModel>();
            for (Int32 i = 0; i < 20; i++) testLookup.Models.Add(new NumericModel { Value = i });

            IQueryable<NumericModel> actual = testLookup.FilterByCheckIds(testLookup.GetModels(), new List<String> { "9.0", "10" });
            IQueryable<NumericModel> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<GuidModel>().FilterByCheckIds(null, new String[0]));

            String expected = $"'{typeof(GuidModel).Name}.Id' property type has to be a string or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region FilterBySelected(IQueryable<T> models, IList<String> ids)

        [Fact]
        public void FilterBySelected_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterBySelected(null, null));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterBySelected(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NumberKey()
        {
            TestLookup<NumericModel> testLookup = new TestLookup<NumericModel>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new NumericModel { Value = i });

            IQueryable<NumericModel> actual = testLookup.FilterBySelected(testLookup.GetModels(), new List<String> { "9.0", "10" });
            IQueryable<NumericModel> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<GuidModel>().FilterBySelected(null, new String[0]));

            String expected = $"'{typeof(GuidModel).Name}.Id' property type has to be a string or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Sort(IQueryable<T> models)

        [Fact]
        public void Sort_ByColumn()
        {
            lookup.Filter.Sort = "Count";

            IQueryable<TestModel> expected = lookup.GetModels().OrderBy(model => model.Count);
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Sort_NoSortColumns(String column)
        {
            lookup.Columns.Clear();
            lookup.Filter.Sort = column;

            IQueryable<TestModel> expected = lookup.GetModels();
            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Sort_NoSortOrderedColumns(String column)
        {
            lookup.Columns.Clear();
            lookup.Filter.Sort = column;

            IQueryable<TestModel> actual = lookup.Sort(lookup.GetModels().OrderByDescending(model => model.Id).Where(model => true));
            IQueryable<TestModel> expected = lookup.GetModels().OrderByDescending(model => model.Id);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Page(IQueryable<T> models)

        [Theory]
        [InlineData(-1, -1, 0, 1)]
        [InlineData(-1, 0, 0, 1)]
        [InlineData(-1, 1, 0, 1)]
        [InlineData(-1, 5, 0, 5)]
        [InlineData(-1, 99, 0, 99)]
        [InlineData(-1, 100, 0, 99)]
        [InlineData(0, -1, 0, 1)]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 5, 0, 5)]
        [InlineData(0, 99, 0, 99)]
        [InlineData(0, 100, 0, 99)]
        [InlineData(1, -1, 1, 1)]
        [InlineData(1, 0, 1, 1)]
        [InlineData(1, 1, 1, 1)]
        [InlineData(1, 5, 1, 5)]
        [InlineData(1, 99, 1, 99)]
        [InlineData(1, 100, 1, 99)]
        [InlineData(5, -1, 5, 1)]
        [InlineData(5, 0, 5, 1)]
        [InlineData(5, 1, 5, 1)]
        [InlineData(5, 5, 5, 5)]
        [InlineData(5, 99, 2, 99)]
        [InlineData(5, 100, 2, 99)]
        [InlineData(199, -1, 199, 1)]
        [InlineData(199, 0, 199, 1)]
        [InlineData(199, 1, 199, 1)]
        [InlineData(199, 5, 39, 5)]
        [InlineData(200, -1, 199, 1)]
        [InlineData(200, 0, 199, 1)]
        [InlineData(200, 1, 199, 1)]
        [InlineData(200, 5, 39, 5)]
        [InlineData(200, 99, 2, 99)]
        [InlineData(200, 100, 2, 99)]
        public void Page_Rows(Int32 page, Int32 rows, Int32 expectedPage, Int32 expectedRows)
        {
            lookup.Filter.Page = page;
            lookup.Filter.Rows = rows;
            lookup.Filter.TotalRows = 1;

            IQueryable<TestModel> expected = lookup.GetModels().Skip(expectedPage * expectedRows).Take(expectedRows);
            IQueryable<TestModel> actual = lookup.Page(lookup.GetModels());

            Assert.Equal(lookup.Filter.Rows, expectedRows);
            Assert.Equal(lookup.Filter.Page, expectedPage);
            Assert.Equal(200, lookup.Filter.TotalRows);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region FormLookupData(IQueryable<T> filtered, IQueryable<T> selected, IQueryable<T> notSelected)

        [Fact]
        public void FormLookupData_FilteredRows()
        {
            lookup.Filter.TotalRows = 140;

            Int32 actual = lookup.FormLookupData(lookup.GetModels(), new[] { new TestModel() }.AsQueryable(), lookup.GetModels().Take(1)).FilteredRows;
            Int32 expected = lookup.Filter.TotalRows;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FormLookupData_Columns()
        {
            Object actual = lookup.FormLookupData(lookup.GetModels(), new[] { new TestModel() }.AsQueryable(), lookup.GetModels()).Columns;
            Object expected = lookup.Columns;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void FormLookupData_Rows()
        {
            IQueryable<TestModel> selected = new TestModel[0].AsQueryable();
            IQueryable<TestModel> notSelected = lookup.GetModels().Skip(6).Take(3);

            IEnumerator<Dictionary<String, String>> actual = lookup.FormLookupData(lookup.GetModels(), selected, notSelected).Rows.GetEnumerator();
            IEnumerator<Dictionary<String, String>> expected = new List<Dictionary<String, String>>
            {
                new Dictionary<String, String>
                {
                    ["Id"] = "6I",
                    ["Label"] = "6V",
                    ["Date"] = new DateTime(2014, 12, 16).ToString("d"),
                    ["Count"] = "16",
                    ["Value"] = "6V"
                },
                new Dictionary<String, String>
                {
                    ["Id"] = "7I",
                    ["Label"] = "7V",
                    ["Date"] = new DateTime(2014, 12, 17).ToString("d"),
                    ["Count"] = "17",
                    ["Value"] = "7V"
                },
                new Dictionary<String, String>
                {
                    ["Id"] = "8I",
                    ["Label"] = "8V",
                    ["Date"] = new DateTime(2014, 12, 18).ToString("d"),
                    ["Count"] = "18",
                    ["Value"] = "8V"
                }
            }.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
                Assert.Equal(expected.Current, actual.Current);
        }

        #endregion

        #region FormData(T model)

        [Fact]
        public void FormData_EmptyValues()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", null));

            Dictionary<String, String> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Id", "Label", "Test" }, row.Keys);
            Assert.Equal(new[] { "1", null,null }, row.Values);
        }

        [Fact]
        public void FormData_Values()
        {
            Dictionary<String, String> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "1", "Test", "Test", DateTime.Now.Date.ToString("d"), "4" }, row.Values);
            Assert.Equal(new[] { "Id", "Label", "Value", "Date", "Count" }, row.Keys);
        }

        #endregion
    }
}
