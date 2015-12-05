using NonFactors.Mvc.Lookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Mvc.Lookup.Tests.Unit
{
    public class LookupColumnsTests
    {
        private List<LookupColumn> testColumns;
        private LookupColumns columns;

        public LookupColumnsTests()
        {
            columns = new LookupColumns();
            testColumns = new List<LookupColumn>
            {
                new LookupColumn("Test1", ""),
                new LookupColumn("Test2", ""),
            };
        }

        #region Property: Keys

        [Fact]
        public void Keys_EqualsToColumKeys()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            IEnumerable<String> expected = testColumns.Select(column => column.Key);
            IEnumerable<String> actual = testColumns.Select(column => column.Key);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Constructor: LookupColumn()

        [Fact]
        public void LookupColumn_EmptyColumn()
        {
            Assert.Empty(columns);
        }

        #endregion

        #region Method: Add(LookupColumn column)

        [Fact]
        public void Add_OnNullColumnThrows()
        {
            Assert.Throws<ArgumentNullException>(() => columns.Add(null));
        }

        [Fact]
        public void Add_OnSameColumnKeyThrows()
        {
            LookupColumn column = new LookupColumn("TestKey", "");
            columns.Add(column);

            Assert.Throws<LookupException>(() => columns.Add(column));
        }

        [Fact]
        public void Add_AddsColumn()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            IEnumerable<LookupColumn> expected = testColumns;
            IEnumerable<LookupColumn> actual = columns;

            Assert.Equal(expected, actual);
        }

        #endregion

        #region Method: Add(String key, String header, String cssClass = ")

        [Fact]
        public void Add_OnNullKeyThrows()
        {
            Assert.Throws<ArgumentNullException>(() => columns.Add(null, ""));
        }

        [Fact]
        public void Add_OnNullHeaderThrows()
        {
            Assert.Throws<ArgumentNullException>(() => columns.Add("", null));
        }

        [Fact]
        public void Add_OnNullCssClass()
        {
            Assert.Throws<ArgumentNullException>(() => columns.Add("", "", null));
        }

        [Fact]
        public void Add_OnSameKeyThrows()
        {
            Assert.Throws<LookupException>(() =>
            {
                columns.Add("TestKey", "");
                columns.Add("TestKey", "");
            });
        }

        [Fact]
        public void Add_AddsColumnByValues()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column.Key, column.Header, column.CssClass);

            IEnumerator<LookupColumn> expected = testColumns.GetEnumerator();
            IEnumerator<LookupColumn> actual = columns.GetEnumerator();

            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(expected.Current.Key, actual.Current.Key);
                Assert.Equal(expected.Current.Header, actual.Current.Header);
                Assert.Equal(expected.Current.CssClass, actual.Current.CssClass);
            }
        }

        #endregion

        #region Method: Remove(LookupColumn column)

        [Fact]
        public void Remove_RemovesColumn()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            LookupColumn firstColumn = testColumns[0];
            testColumns.RemoveAt(0);

            Assert.True(columns.Remove(firstColumn));
            Assert.Equal(testColumns, columns);
        }

        [Fact]
        public void Remove_DoesNotRemoveColumn()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            Assert.False(columns.Remove(new LookupColumn("Test1", "")));
            Assert.Equal(testColumns, columns);
        }

        [Fact]
        public void Remove_RemovesItSelf()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            foreach (LookupColumn column in columns as IEnumerable)
                Assert.True(columns.Remove(column));

            Assert.Empty(columns);
        }

        #endregion

        #region Method: Remove(String key)

        [Fact]
        public void Remove_RemovesByKey()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            foreach (LookupColumn column in columns)
                Assert.True(columns.Remove(column.Key));

            Assert.Empty(columns);
        }

        [Fact]
        public void Remove_DoesNotRemoveByKey()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            foreach (LookupColumn column in columns)
                Assert.False(columns.Remove(column.Key + column.Key));

            Assert.Equal(testColumns, columns);
        }

        #endregion

        #region Method: Clear()

        [Fact]
        public void Clear_ClearsColumns()
        {
            columns.Add("Test1", "");
            columns.Add("Test2", "");
            columns.Clear();

            Assert.Empty(columns);
        }

        #endregion

        #region Method: GetEnumerator()

        [Fact]
        public void GetEnumerator_ReturnsColumnsCopy()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            foreach (LookupColumn column in columns)
                columns.Remove(column);

            Assert.Empty(columns);
        }

        [Fact]
        public void GetEnumerator_ReturnsColumns()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            IEnumerable<LookupColumn> actual = columns.ToArray();
            IEnumerable<LookupColumn> expected = testColumns;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetEnumerator_ReturnsSameColumns()
        {
            foreach (LookupColumn column in testColumns)
                columns.Add(column);

            IEnumerable<LookupColumn> expected = columns.ToArray();
            IEnumerable<LookupColumn> actual = columns;

            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
