using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NonFactors.Mvc.Lookup.Tests.Unit
{
    public class LookupColumnsTests
    {
        private List<LookupColumn> allColumns;
        private LookupColumns columns;

        public LookupColumnsTests()
        {
            allColumns = new List<LookupColumn>
            {
                new LookupColumn("Test1", "Header1", "Class1"),
                new LookupColumn("Test2", "Header2", "Class2")
            };

            columns = new LookupColumns();

            foreach (LookupColumn column in allColumns)
                columns.Add(column);
        }

        #region Keys

        [Fact]
        public void Keys_ReturnsColumnKeys()
        {
            IEnumerable<String> expected = new[] { "Test1", "Test2" };
            IEnumerable<String> actual = columns.Keys;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region LookupColumns()

        [Fact]
        public void LookupColumns_AreEmpty()
        {
            columns = new LookupColumns();

            Assert.Empty(columns);
        }

        #endregion

        #region Add(LookupColumn column)

        [Fact]
        public void Add_NullColumn_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => columns.Add(null));

            Assert.Equal("column", actual.ParamName);
        }

        [Fact]
        public void Add_SameColumnKey_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => columns.Add(columns.First()));

            String expected = $"Can not add lookup column with the same key '{columns.First().Key}'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_Column()
        {
            LookupColumn column = new LookupColumn("Test3", "3");
            allColumns.Add(column);

            columns.Add(column);

            IEnumerable<LookupColumn> expected = allColumns;
            IEnumerable<LookupColumn> actual = columns;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Add(String key, String header, String cssClass = ")

        [Fact]
        public void Add_NullKey_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => columns.Add(null, ""));

            Assert.Equal("key", actual.ParamName);
        }

        [Fact]
        public void Add_NullHeader_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => columns.Add("", null));

            Assert.Equal("header", actual.ParamName);
        }

        [Fact]
        public void Add_NullCssClass_Throws()
        {
            ArgumentNullException actual = Assert.Throws<ArgumentNullException>(() => columns.Add("", "", null));

            Assert.Equal("cssClass", actual.ParamName);
        }

        [Fact]
        public void Add_SameKey_Throws()
        {
            LookupException exception = Assert.Throws<LookupException>(() => columns.Add(columns.First().Key, "1"));

            String expected = $"Can not add lookup column with the same key '{columns.First().Key}'.";
            String actual = exception.Message;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Add_ColumnFromValues()
        {
            columns = new LookupColumns();
            foreach (LookupColumn column in allColumns)
                columns.Add(column.Key, column.Header, column.CssClass);

            IEnumerator<LookupColumn> expected = allColumns.GetEnumerator();
            IEnumerator<LookupColumn> actual = columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Key, actual.Current.Key);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
            }
        }

        #endregion

        #region Remove(LookupColumn column)

        [Fact]
        public void Remove_NoColumn_ReturnsFalse()
        {
            Assert.False(columns.Remove(new LookupColumn("Test1", "")));
            Assert.Equal(allColumns, columns);
        }

        [Fact]
        public void Remove_Column()
        {
            allColumns.RemoveAt(0);

            Assert.True(columns.Remove(columns.First()));
            Assert.Equal(allColumns, columns);
        }

        [Fact]
        public void Remove_ItSelf()
        {
            foreach (LookupColumn column in columns)
                Assert.True(columns.Remove(column));

            Assert.Empty(columns);
        }

        #endregion

        #region Remove(String key)

        [Fact]
        public void Remove_ByKey()
        {
            foreach (LookupColumn column in columns)
                Assert.True(columns.Remove(column.Key));

            Assert.Empty(columns);
        }

        [Fact]
        public void Remove_NoKey_ReturnsFalse()
        {
            Assert.False(columns.Remove("Test3"));
            Assert.Equal(allColumns, columns);
        }

        #endregion

        #region Clear()

        [Fact]
        public void Clear_Columns()
        {
            columns.Clear();

            Assert.Empty(columns);
        }

        #endregion

        #region GetEnumerator()

        [Fact]
        public void GetEnumerator_ReturnsColumns()
        {
            IEnumerable<LookupColumn> actual = columns.ToArray();
            IEnumerable<LookupColumn> expected = allColumns;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetEnumerator_ReturnsSameColumns()
        {
            IEnumerable<LookupColumn> expected = columns.ToArray();
            IEnumerable<LookupColumn> actual = columns;

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
