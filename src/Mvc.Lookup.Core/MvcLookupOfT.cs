using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace NonFactors.Mvc.Lookup
{
    public abstract class MvcLookup<T> : MvcLookup where T : class
    {
        public virtual IEnumerable<PropertyInfo> AttributedProperties
        {
            get
            {
                return typeof(T)
                    .GetProperties()
                    .Where(property => property.IsDefined(typeof(LookupColumnAttribute), false))
                    .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false).Position);
            }
        }

        protected MvcLookup()
        {
            foreach (PropertyInfo property in AttributedProperties)
                Columns.Add(GetColumnKey(property), GetColumnHeader(property), GetColumnCssClass(property));
        }
        public virtual String GetColumnKey(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            return property.Name;
        }
        public virtual String GetColumnHeader(PropertyInfo property)
        {
            return property?.GetCustomAttribute<DisplayAttribute>(false)?.GetShortName();
        }
        public virtual String GetColumnCssClass(PropertyInfo property)
        {
            return null;
        }

        public override LookupData GetData()
        {
            IQueryable<T> models = GetModels();
            models = FilterByRequest(models);
            models = Sort(models);

            return FormLookupData(models);
        }
        public abstract IQueryable<T> GetModels();

        private IQueryable<T> FilterByRequest(IQueryable<T> models)
        {
            if (Filter.Id != null)
                return FilterById(models);

            if (Filter.AdditionalFilters.Count > 0)
                models = FilterByAdditionalFilters(models);

            return FilterBySearch(models);
        }
        public virtual IQueryable<T> FilterById(IQueryable<T> models)
        {
            PropertyInfo idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new LookupException($"'{typeof(T).Name}' type does not have property named 'Id', required for automatic id filtering.");

            if (idProperty.PropertyType == typeof(String))
                return models.Where("Id = @0", Filter.Id);

            Decimal id;
            if (IsNumeric(idProperty.PropertyType) && Decimal.TryParse(Filter.Id, out id))
                return models.Where("Id = @0", id);

            throw new LookupException($"'{typeof(T).Name}.Id' property type has to be a string or a number.");
        }
        public virtual IQueryable<T> FilterBySearch(IQueryable<T> models)
        {
            if (String.IsNullOrEmpty(Filter.Search))
                return models;

            List<String> queries = new List<String>();
            foreach (String property in Columns.Keys)
                if (typeof(T).GetProperty(property)?.PropertyType == typeof(String))
                    queries.Add($"({property} != null && {property}.ToLower().Contains(@0))");

            if (queries.Count == 0) return models;

            return models.Where(String.Join(" || ", queries), Filter.Search.ToLower());
        }
        public virtual IQueryable<T> FilterByAdditionalFilters(IQueryable<T> models)
        {
            foreach (KeyValuePair<String, Object> filter in Filter.AdditionalFilters.Where((KeyValuePair<String, Object> item) => item.Value != null))
                models = models.Where($"({filter.Key} != null && {filter.Key} == @0)", filter.Value);

            return models;
        }

        public virtual IQueryable<T> Sort(IQueryable<T> models)
        {
            String column = Filter.SortColumn ?? DefaultSortColumn ?? Columns.Keys.FirstOrDefault();
            if (String.IsNullOrWhiteSpace(column))
                return models;

            return models.OrderBy(column + " " + Filter.SortOrder);
        }

        public virtual LookupData FormLookupData(IQueryable<T> models)
        {
            LookupData data = new LookupData();
            data.FilteredRows = models.Count();
            data.Columns = Columns;

            IQueryable<T> pagedModels = models
                .Skip(Filter.Page * Filter.Rows)
                .Take(Filter.Rows);

            foreach (T model in pagedModels)
            {
                Dictionary<String, String> row = new Dictionary<String, String>();
                AddId(row, model);
                AddAutocomplete(row, model);
                AddColumns(row, model);
                AddAdditionalData(row, model);

                data.Rows.Add(row);
            }

            return data;
        }
        public virtual void AddId(Dictionary<String, String> row, T model)
        {
            row.Add(IdKey, GetValue(model, "Id"));
        }
        public virtual void AddAutocomplete(Dictionary<String, String> row, T model)
        {
            row.Add(AcKey, GetValue(model, Columns.Keys.FirstOrDefault() ?? ""));
        }
        public virtual void AddColumns(Dictionary<String, String> row, T model)
        {
            foreach (String column in Columns.Keys)
                row[column] = GetValue(model, column);
        }
        public virtual void AddAdditionalData(Dictionary<String, String> row, T model)
        {
        }

        private String GetValue(T model, String propertyName)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            if (property == null) return null;

            LookupColumnAttribute column = property.GetCustomAttribute<LookupColumnAttribute>(false);
            if (column?.Format != null) return String.Format(column.Format, property.GetValue(model));

            return property.GetValue(model)?.ToString();
        }
        private Boolean IsNumeric(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return true;
                default:
                    return false;
            }
        }
    }
}
