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
            lookup.Filter.Rows = 20;

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

        [Fact]
        public void GetId_NoProperty()
        {
            Assert.Empty(new TestLookup<NoIdModel>().GetId(new NoIdModel()));
        }

        [Fact]
        public void GetId_Value()
        {
            String actual = lookup.GetId(new TestModel { Id = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetLabel_NoColumns()
        {
            lookup.Columns.Clear();

            Assert.Empty(lookup.GetLabel(new TestModel()));
        }

        [Fact]
        public void GetLabel_Value()
        {
            String actual = lookup.GetLabel(new TestModel { Value = "Test" });
            String expected = "Test";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void AttributedProperties_GetsOrderedProperties()
        {
            IEnumerable<PropertyInfo> actual = lookup.AttributedProperties;
            IEnumerable<PropertyInfo> expected = typeof(TestModel).GetProperties()
                .Where(property => property.GetCustomAttribute<LookupColumnAttribute>(false) != null)
                .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false)!.Position);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MvcLookup_AddsColumns()
        {
            List<LookupColumn> columns = new List<LookupColumn>
            {
                new LookupColumn("Value", "") { Hidden = false, Filterable = true },
                new LookupColumn("Date", "Date") { Hidden = false, Filterable = true },
                new LookupColumn("Count", "Value") { Hidden = false, Filterable = false }
            };

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
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
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
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
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

            Assert.Equal(new DateTime(2014, 12, 25).ToString("d"), actual.Selected[0]["Date"]);
            Assert.Equal("15V", actual.Selected[0]["Label"]);
            Assert.Equal("15V", actual.Selected[0]["Value"]);
            Assert.Equal("25", actual.Selected[0]["Count"]);
            Assert.Equal("15I", actual.Selected[0]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 14).ToString("d"), actual.Selected[1]["Date"]);
            Assert.Equal("125V", actual.Selected[1]["Label"]);
            Assert.Equal("125V", actual.Selected[1]["Value"]);
            Assert.Equal("135", actual.Selected[1]["Count"]);
            Assert.Equal("125I", actual.Selected[1]["Id"]);

            Assert.Equal(new DateTime(2015, 4, 22).ToString("d"), actual.Rows[0]["Date"]);
            Assert.Equal("133V", actual.Rows[0]["Label"]);
            Assert.Equal("133V", actual.Rows[0]["Value"]);
            Assert.Equal("143", actual.Rows[0]["Count"]);
            Assert.Equal("133I", actual.Rows[0]["Id"]);

            Assert.Equal(lookup.Columns, actual.Columns);
            Assert.Equal(2, actual.Selected.Count);
            Assert.Single(actual.Rows);
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
            Assert.Empty(actual.Selected);
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
            Assert.Equal(2, actual.Rows.Count);
            Assert.Empty(actual.Selected);
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

            IQueryable<TestModel> expected = lookup.GetModels().Where(model => model.Id!.Contains("1"));
            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNonStringProperties()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn("Count", ""));

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySearch_DoesNotFilterNotFilterableColumns()
        {
            lookup.Columns.Clear();
            lookup.Filter.Search = "1";
            lookup.Columns.Add(new LookupColumn("Value", "") { Filterable = false });

            IQueryable<TestModel> actual = lookup.FilterBySearch(lookup.GetModels());
            IQueryable<TestModel> expected = lookup.GetModels();

            Assert.Equal(expected, actual);
        }

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

        [Fact]
        public void FilterByIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_Guids()
        {
            TestLookup<GuidModel> testLookup = new TestLookup<GuidModel>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new GuidModel { Id = Guid.NewGuid() });
            List<String> ids = new List<String> { testLookup.Models[4].Id.ToString(), testLookup.Models[9].Id.ToString() };

            IQueryable<GuidModel> expected = testLookup.GetModels().Where(model => ids.Contains(model.Id.ToString()));
            IQueryable<GuidModel> actual = testLookup.FilterByIds(testLookup.GetModels(), ids);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterByIds(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterByIds(Array.Empty<ObjectModel>().AsQueryable(), new String[0]));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByNotIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByNotIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => !ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_Guids()
        {
            TestLookup<GuidModel> testLookup = new TestLookup<GuidModel>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new GuidModel { Id = Guid.NewGuid() });
            List<String> ids = new List<String> { testLookup.Models[4].Id.ToString(), testLookup.Models[9].Id.ToString() };

            IQueryable<GuidModel> expected = testLookup.GetModels().Where(model => !ids.Contains(model.Id.ToString()));
            IQueryable<GuidModel> actual = testLookup.FilterByNotIds(testLookup.GetModels(), ids);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterByNotIds(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => !new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByNotIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterByNotIds(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterByCheckIds(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterByCheckIds(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterByCheckIds(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterByCheckIds_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterByCheckIds(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NoIdProperty_Throws()
        {
            TestLookup<NoIdModel> testLookup = new TestLookup<NoIdModel>();

            LookupException exception = Assert.Throws<LookupException>(() => testLookup.FilterBySelected(Array.Empty<NoIdModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(NoIdModel).Name}' type does not have key or property named 'Id', required for automatic id filtering.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_String()
        {
            List<String> ids = new List<String> { "9I", "10I" };

            IQueryable<TestModel> actual = lookup.FilterBySelected(lookup.GetModels(), ids);
            IQueryable<TestModel> expected = lookup.GetModels().Where(model => ids.Contains(model.Id!));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NumberKey()
        {
            TestLookup<Int32Model> testLookup = new TestLookup<Int32Model>();
            for (Int32 i = 0; i < 20; i++)
                testLookup.Models.Add(new Int32Model { Value = i });

            IQueryable<Int32Model> actual = testLookup.FilterBySelected(testLookup.GetModels(), new List<String> { "9", "10" });
            IQueryable<Int32Model> expected = testLookup.GetModels().Where(model => new[] { 9, 10 }.Contains(model.Value));

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void FilterBySelected_NotSupportedIdType_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => new TestLookup<ObjectModel>().FilterBySelected(Array.Empty<ObjectModel>().AsQueryable(), Array.Empty<String>()));

            String expected = $"'{typeof(ObjectModel).Name}.Id' property type has to be a string, guid or a number.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

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

        [Theory]
        [InlineData(-1, -1, 0, 1)]
        [InlineData(-1, 0, 0, 1)]
        [InlineData(-1, 1, 0, 1)]
        [InlineData(-1, 100, 0, 100)]
        [InlineData(-1, 101, 0, 100)]
        [InlineData(0, -1, 0, 1)]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 100, 0, 100)]
        [InlineData(0, 101, 0, 100)]
        [InlineData(50, -1, 50, 1)]
        [InlineData(50, 0, 50, 1)]
        [InlineData(50, 1, 50, 1)]
        [InlineData(50, 100, 50, 100)]
        [InlineData(50, 101, 50, 100)]
        public void Page_Rows(Int32 offset, Int32 rows, Int32 expectedOffset, Int32 expectedRows)
        {
            lookup.Filter.Rows = rows;
            lookup.Filter.Offset = offset;

            IQueryable<TestModel> expected = lookup.GetModels().Skip(expectedOffset).Take(expectedRows);
            IQueryable<TestModel> actual = lookup.Page(lookup.GetModels());

            Assert.Equal(expectedOffset, lookup.Filter.Offset);
            Assert.Equal(expectedRows, lookup.Filter.Rows);
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
        public void FormLookupData_Selected()
        {
            IQueryable<TestModel> notSelected = new TestModel[0].AsQueryable();
            IQueryable<TestModel> selected = lookup.GetModels().Skip(6).Take(3);

            IEnumerator<Dictionary<String, String>> actual = lookup.FormLookupData(lookup.GetModels(), selected, notSelected).Selected.GetEnumerator();
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

        [Fact]
        public void FormData_EmptyValues()
        {
            lookup.Columns.Clear();
            lookup.Columns.Add(new LookupColumn("Test", ""));

            Dictionary<String, String?> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", "Label", "Id" }, row.Keys);
            Assert.Equal(new[] { null, "", "1" }, row.Values);
        }

        [Fact]
        public void FormData_Values()
        {
            Dictionary<String, String?> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", DateTime.Now.Date.ToString("d"), "4", "Test", "1" }, row.Values);
            Assert.Equal(new[] { "Value", "Date", "Count", "Label", "Id" }, row.Keys);
        }

        [Fact]
        public void FormData_OverridenValues()
        {
            lookup.GetId = (model) => $"Test {model.Id}";
            lookup.GetLabel = (model) => $"Test label {model.Id}";
            Dictionary<String, String?> row = lookup.FormData(new TestModel { Id = "1", Value = "Test", Date = DateTime.Now.Date, Count = 4 });

            Assert.Equal(new[] { "Test", DateTime.Now.Date.ToString("d"), "4", "Test label 1", "Test 1" }, row.Values);
            Assert.Equal(new[] { "Value", "Date", "Count", "Label", "Id" }, row.Keys);
        }
    }
}
