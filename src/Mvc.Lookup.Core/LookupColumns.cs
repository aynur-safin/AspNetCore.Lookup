using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NonFactors.Mvc.Lookup
{
    public class LookupColumns : IEnumerable<LookupColumn>
    {
        private List<LookupColumn> Columns
        {
            get;
            set;
        }
        public IEnumerable<String> Keys
        {
            get
            {
                return Columns.Select(column => column.Key);
            }
        }

        public LookupColumns()
        {
            Columns = new List<LookupColumn>();
        }

        public void Add(LookupColumn column)
        {
            if (column == null)
                throw new ArgumentNullException(nameof(column));

            if (Columns.Any(col => col.Key == column.Key))
                throw new LookupException($"Can not add lookup column with the same key '{column.Key}'.");

            Columns.Add(column);
        }
        public void Add(String key, String header, String cssClass = "")
        {
            Add(new LookupColumn(key, header, cssClass));
        }
        public Boolean Remove(LookupColumn column)
        {
            return Columns.Remove(column);
        }
        public Boolean Remove(String key)
        {
            return Columns.Remove(Columns.FirstOrDefault(column => column.Key == key));
        }
        public void Clear()
        {
            Columns.Clear();
        }

        public IEnumerator<LookupColumn> GetEnumerator()
        {
            return Columns.ToList().GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
