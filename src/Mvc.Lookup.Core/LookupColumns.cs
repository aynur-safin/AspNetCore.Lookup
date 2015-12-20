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
                return Columns.Select(column => column.Name);
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

            if (Columns.Any(col => col.Name == column.Name))
                throw new LookupException($"Can not add lookup column with the same name '{column.Name}'.");

            Columns.Add(column);
        }
        public void Add(String name, String header, String cssClass = "")
        {
            Add(new LookupColumn(name, header, cssClass));
        }
        public Boolean Remove(LookupColumn column)
        {
            return Columns.Remove(column);
        }
        public Boolean Remove(String name)
        {
            return Columns.Remove(Columns.FirstOrDefault(column => column.Name == name));
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
