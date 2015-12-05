using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NonFactors.Mvc.Lookup.Tests.Objects
{
    public class GenericLookupProxy<T> : GenericLookup<T> where T : class
    {
        public String BaseDefaultSortColumn
        {
            get
            {
                return DefaultSortColumn;
            }
            set
            {
                DefaultSortColumn = value;
            }
        }
        public IEnumerable<PropertyInfo> BaseAttributedProperties
        {
            get
            {
                return AttributedProperties;
            }
        }

        public IQueryable<T> BaseGetModels()
        {
            return GetModels();
        }
        protected override IQueryable<T> GetModels()
        {
            return new List<T>().AsQueryable();
        }

        public String BaseGetColumnKey(PropertyInfo property)
        {
            return GetColumnKey(property);
        }
        public String BaseGetColumnHeader(PropertyInfo property)
        {
            return GetColumnHeader(property);
        }
        public String BaseGetColumnCssClass(PropertyInfo property)
        {
            return GetColumnCssClass(property);
        }

        public IQueryable<T> BaseFilterById(IQueryable<T> models)
        {
            return FilterById(models);
        }
        public IQueryable<T> BaseFilterByAdditionalFilters(IQueryable<T> models)
        {
            return FilterByAdditionalFilters(models);
        }
        public IQueryable<T> BaseFilterBySearchTerm(IQueryable<T> models)
        {
            return FilterBySearchTerm(models);
        }
        public IQueryable<T> BaseSort(IQueryable<T> models)
        {
            return Sort(models);
        }

        public LookupData BaseFormLookupData(IQueryable<T> models)
        {
            return FormLookupData(models);
        }
        public void BaseAddId(Dictionary<String, String> row, T model)
        {
            AddId(row, model);
        }
        public void BaseAddAutocomplete(Dictionary<String, String> row, T model)
        {
            AddAutocomplete(row, model);
        }
        public void BaseAddColumns(Dictionary<String, String> row, T model)
        {
            AddColumns(row, model);
        }
        public void BaseAddAdditionalData(Dictionary<String, String> row, T model)
        {
            AddAdditionalData(row, model);
        }
    }
}
