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
        protected IEnumerable<PropertyInfo> AttributedProperties
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
        protected virtual String GetColumnKey(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            return property.Name;
        }
        protected virtual String GetColumnHeader(PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));

            return property.GetCustomAttribute<DisplayAttribute>(false)?.GetName() ?? "";
        }
        protected virtual String GetColumnCssClass(PropertyInfo property)
        {
            return "";
        }

        public override LookupData GetData()
        {
            IQueryable<T> models = GetModels();
            models = Filter(models);
            models = Sort(models);

            return FormLookupData(models);
        }
        protected abstract IQueryable<T> GetModels();

        private IQueryable<T> Filter(IQueryable<T> models)
        {
            if (CurrentFilter.Id != null)
                return FilterById(models);

            if (CurrentFilter.AdditionalFilters.Count > 0)
                models = FilterByAdditionalFilters(models);

            return FilterBySearchTerm(models);
        }
        protected virtual IQueryable<T> FilterById(IQueryable<T> models)
        {
            PropertyInfo idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new LookupException($"'{typeof(T).Name}' type does not have property named 'Id'.");

            if (idProperty.PropertyType == typeof(String))
                return models.Where("Id = @0", CurrentFilter.Id);

            Decimal temp;
            if (IsNumeric(idProperty.PropertyType) && Decimal.TryParse(CurrentFilter.Id, out temp))
                return models.Where("Id = @0", temp);

            throw new LookupException($"'{typeof(T).Name}.Id' can not be filtered by using '{CurrentFilter.Id}' value, because it's not a string nor a number.");
        }
        protected virtual IQueryable<T> FilterByAdditionalFilters(IQueryable<T> models)
        {
            foreach (KeyValuePair<String, Object> filter in CurrentFilter.AdditionalFilters.Where(item => item.Value != null))
                models = models.Where($@"({filter.Key} != null && {filter.Key} == @0)", filter.Value);

            return models;
        }
        protected virtual IQueryable<T> FilterBySearchTerm(IQueryable<T> models)
        {
            if (CurrentFilter.SearchTerm == null) return models;

            String term = CurrentFilter.SearchTerm.ToLower();
            List<String> queries = new List<String>();

            foreach (String property in Columns.Keys)
                if (GetType(property) == typeof(String))
                    queries.Add($"({property} != null && {property}.ToLower().Contains(@0))");

            if (queries.Count == 0) return models;

            return models.Where(String.Join(" || ", queries), term);
        }
        protected virtual IQueryable<T> Sort(IQueryable<T> models)
        {
            String sortColumn = CurrentFilter.SortColumn ?? DefaultSortColumn;
            if (sortColumn != null)
                if (Columns.Keys.Contains(sortColumn))
                    return models.OrderBy(sortColumn + " " + CurrentFilter.SortOrder);
                else
                    throw new LookupException($"Lookup does not contain sort column named '{sortColumn}'.");

            if (Columns.Any())
                return models.OrderBy(Columns.First().Key + " " + CurrentFilter.SortOrder);

            throw new LookupException("Lookup should have at least one column.");
        }

        protected virtual LookupData FormLookupData(IQueryable<T> models)
        {
            LookupData data = new LookupData();
            data.FilteredRecords = models.Count();
            data.Columns = Columns;

            IQueryable<T> pagedModels = models
                .Skip(CurrentFilter.Page * CurrentFilter.RecordsPerPage)
                .Take(CurrentFilter.RecordsPerPage);

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
        protected virtual void AddId(Dictionary<String, String> row, T model)
        {
            row.Add(IdKey, GetValue(model, "Id"));
        }
        protected virtual void AddAutocomplete(Dictionary<String, String> row, T model)
        {
            if (!Columns.Any())
                throw new LookupException("Lookup should have at least one column.");

            row.Add(AcKey, GetValue(model, Columns.Keys.First()));
        }
        protected virtual void AddColumns(Dictionary<String, String> row, T model)
        {
            if (!Columns.Any())
                throw new LookupException("Lookup should have at least one column.");

            foreach (String column in Columns.Keys)
                row.Add(column, GetValue(model, column));
        }
        protected virtual void AddAdditionalData(Dictionary<String, String> row, T model)
        {
        }

        private String GetValue(T model, String propertyName)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            if (property == null)
                throw new LookupException($"'{typeof(T).Name}' type does not have property named '{propertyName}'.");

            Object value = property.GetValue(model) ?? "";
            LookupColumnAttribute column = property.GetCustomAttribute<LookupColumnAttribute>(false);
            if (column?.Format != null)
                value = String.Format(column.Format, value);

            return value.ToString();
        }
        private Type GetType(String propertyName)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            if (property == null)
                throw new LookupException($"'{typeof(T).Name}' type does not have property named '{propertyName}'.");

            return property.PropertyType;
        }
        private Boolean IsNumeric(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.GetTypeInfo().IsEnum) return false;

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
