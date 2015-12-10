using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;

namespace NonFactors.Mvc.Lookup
{
    public abstract class GenericLookup<T> : AbstractLookup where T : class
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

        protected GenericLookup()
        {
            foreach (PropertyInfo property in AttributedProperties)
                Columns.Add(GetColumnKey(property), GetColumnHeader(property), GetColumnCssClass(property));
        }
        protected virtual String GetColumnKey(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            LookupColumnAttribute column = property.GetCustomAttribute<LookupColumnAttribute>(false);
            if (column != null && column.Relation != null)
                return property.Name + "." + GetColumnKey(GetRelationProperty(property, column.Relation));

            return property.Name;
        }
        protected virtual String GetColumnHeader(PropertyInfo property)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));

            LookupColumnAttribute column = property.GetCustomAttribute<LookupColumnAttribute>(false);
            if (column != null && column.Relation != null)
                return GetColumnHeader(GetRelationProperty(property, column.Relation));

            DisplayAttribute header = property.GetCustomAttribute<DisplayAttribute>(false);
            if (header != null) return header.GetName();

            return property.Name;
        }
        protected virtual String GetColumnCssClass(PropertyInfo property)
        {
            return "";
        }
        private PropertyInfo GetRelationProperty(PropertyInfo property, String relation)
        {
            PropertyInfo relationProperty = property.PropertyType.GetProperty(relation);
            if (relationProperty != null)
                return relationProperty;

            throw new LookupException($"'{property.DeclaringType.Name}.{property.Name}' does not have property named '{relation}'.");
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
                models = models.Where(FormEqualsQuery(filter.Key), filter.Value);

            return models;
        }
        protected virtual IQueryable<T> FilterBySearchTerm(IQueryable<T> models)
        {
            if (CurrentFilter.SearchTerm == null) return models;

            String term = CurrentFilter.SearchTerm.ToLower();
            List<String> queries = new List<String>();

            foreach (String propertyName in Columns.Keys)
                if (GetType(propertyName) == typeof(String))
                    queries.Add(FormContainsQuery(propertyName));

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

            IQueryable<T> pagedModels = CurrentFilter.Page != 0 && CurrentFilter.RecordsPerPage != 0
                ? models
                    .Skip(CurrentFilter.Page * CurrentFilter.RecordsPerPage)
                    .Take(CurrentFilter.RecordsPerPage)
                : new T[0].AsQueryable();

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
                AddColumn(row, column, model);
        }
        protected virtual void AddAdditionalData(Dictionary<String, String> row, T model)
        {
        }

        private String FormContainsQuery(String propertyName)
        {
            return $"({FormNotNullQuery(propertyName)} && {propertyName}.ToLower().Contains(@0))";
        }
        private String FormNotNullQuery(String propertyName)
        {
            List<String> queries = new List<String>();
            String[] properties = propertyName.Split('.');

            for (Int32 i = 0; i < properties.Length; ++i)
                queries.Add(String.Join(".", properties.Take(i + 1)) + " != null");

            return String.Join(" && ", queries);
        }
        private String FormEqualsQuery(String propertyName)
        {
            return String.Format(@"({0} && {1} == @0)", FormNotNullQuery(propertyName), propertyName);
        }

        private void AddColumn(Dictionary<String, String> row, String column, T model)
        {
            row.Add(column, GetValue(model, column));
        }
        private String GetValue(Object model, String fullPropertyName)
        {
            if (model == null) return "";

            Type type = model.GetType();
            String[] properties = fullPropertyName.Split('.');
            PropertyInfo property = type.GetProperty(properties[0]);
            if (property == null)
                throw new LookupException($"'{type.Name}' type does not have property named '{properties[0]}'.");

            if (properties.Length > 1)
                return GetValue(property.GetValue(model), String.Join(".", properties.Skip(1)));

            Object value = property.GetValue(model) ?? "";
            LookupColumnAttribute column = property.GetCustomAttribute<LookupColumnAttribute>(false);
            if (column != null && column.Format != null)
                value = String.Format(column.Format, value);

            return value.ToString();
        }
        private Type GetType(String fullPropertyName)
        {
            Type type = typeof(T);
            foreach (String propertyName in fullPropertyName.Split('.'))
            {
                PropertyInfo property = type.GetProperty(propertyName);
                if (property == null)
                    throw new LookupException($"'{type.Name}' type does not have property named '{propertyName}'.");

                type = property.PropertyType;
            }

            return type;
        }
        private Boolean IsNumeric(Type type)
        {
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (type.IsEnum) return false;

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
