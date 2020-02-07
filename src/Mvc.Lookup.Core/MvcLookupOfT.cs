﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;

namespace NonFactors.Mvc.Lookup
{
    public abstract class MvcLookup<T> : MvcLookup where T : class
    {
        public Func<T, String> GetId { get; set; }
        public Func<T, String> GetLabel { get; set; }
        public virtual IEnumerable<PropertyInfo> AttributedProperties
        {
            get
            {
                return typeof(T)
                    .GetProperties()
                    .Where(property => property.IsDefined(typeof(LookupColumnAttribute), false))
                    .OrderBy(property => property.GetCustomAttribute<LookupColumnAttribute>(false)!.Position);
            }
        }

        protected MvcLookup()
        {
            GetId = (model) => GetValue(model, "Id") ?? "";
            GetLabel = (model) => GetValue(model, Columns.Where(col => !col.Hidden).Select(col => col.Key).FirstOrDefault() ?? "") ?? "";

            foreach (PropertyInfo property in AttributedProperties)
            {
                LookupColumnAttribute column = property.GetCustomAttribute<LookupColumnAttribute>(false)!;
                Columns.Add(new LookupColumn(GetColumnKey(property), GetColumnHeader(property))
                {
                    FilterPredicate = column.FilterPredicate,
                    CssClass = GetColumnCssClass(property),
                    FilterCase = column.FilterCase,
                    Filterable = column.Filterable,
                    Hidden = column.Hidden
                });
            }
        }

        public override LookupData GetData()
        {
            IQueryable<T> models = GetModels();
            IQueryable<T> selected = new T[0].AsQueryable();
            IQueryable<T> notSelected = Sort(FilterByRequest(models));

            if (Filter.Offset == 0 && Filter.Ids.Count == 0 && Filter.Selected.Count > 0)
                selected = Sort(FilterBySelected(models, Filter.Selected));

            return FormLookupData(notSelected, selected, Page(notSelected));
        }
        public abstract IQueryable<T> GetModels();

        private IQueryable<T> FilterByRequest(IQueryable<T> models)
        {
            if (Filter.Ids.Count > 0)
                return FilterByIds(models, Filter.Ids);

            if (Filter.Selected.Count > 0)
                models = FilterByNotIds(models, Filter.Selected);

            if (Filter.CheckIds.Count > 0)
                models = FilterByCheckIds(models, Filter.CheckIds);

            if (Filter.AdditionalFilters.Count > 0)
                models = FilterByAdditionalFilters(models);

            return FilterBySearch(models);
        }
        public virtual IQueryable<T> FilterBySearch(IQueryable<T> models)
        {
            if (String.IsNullOrEmpty(Filter.Search))
                return models;

            List<Object> values = new List<Object>();
            List<String> queries = new List<String>();

            foreach (LookupColumn column in Columns.Where(column => !column.Hidden && column.Filterable))
            {
                PropertyInfo? property = typeof(T).GetProperty(column.Key);

                if (GenerateQuery(column, property, queries.Count) is String query && ParseSearch(column, property) is Object value)
                {
                    queries.Add(query);
                    values.Add(value);
                }
            }

            return queries.Count == 0 ? models : models.Where(String.Join(" || ", queries), values.ToArray());
        }
        public virtual IQueryable<T> FilterByAdditionalFilters(IQueryable<T> models)
        {
            foreach (KeyValuePair<String, Object?> filter in Filter.AdditionalFilters.Where(item => item.Value != null))
                if (filter.Value is IEnumerable && !(filter.Value is String))
                    models = models.Where($"@0.Contains({filter.Key})", filter.Value);
                else
                    models = models.Where($"({filter.Key} != null && {filter.Key} == @0)", filter.Value);

            return models;
        }
        public virtual IQueryable<T> FilterByIds(IQueryable<T> models, IList<String> ids)
        {
            PropertyInfo? key = typeof(T).GetProperties()
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute))) ?? typeof(T).GetProperty("Id");

            if (key == null)
                throw new LookupException($"'{typeof(T).Name}' type does not have key or property named 'Id', required for automatic id filtering.");

            if (!IsFilterable(key.PropertyType))
                throw new LookupException($"'{typeof(T).Name}.{key.Name}' property type is not filterable.");

            return models.Where($"@0.Contains({key.Name})", Parse(key.PropertyType, ids));
        }
        public virtual IQueryable<T> FilterByNotIds(IQueryable<T> models, IList<String> ids)
        {
            PropertyInfo? key = typeof(T).GetProperties()
                .FirstOrDefault(prop => prop.IsDefined(typeof(KeyAttribute))) ?? typeof(T).GetProperty("Id");

            if (key == null)
                throw new LookupException($"'{typeof(T).Name}' type does not have key or property named 'Id', required for automatic id filtering.");

            if (!IsFilterable(key.PropertyType))
                throw new LookupException($"'{typeof(T).Name}.{key.Name}' property type is not filterable.");

            return models.Where($"!@0.Contains({key.Name})", Parse(key.PropertyType, ids));
        }
        public virtual IQueryable<T> FilterByCheckIds(IQueryable<T> models, IList<String> ids)
        {
            return FilterByIds(models, ids);
        }
        public virtual IQueryable<T> FilterBySelected(IQueryable<T> models, IList<String> ids)
        {
            return FilterByIds(models, ids);
        }

        public virtual IQueryable<T> Sort(IQueryable<T> models)
        {
            if (String.IsNullOrWhiteSpace(Filter.Sort))
                if (LookupQuery.IsOrdered(models))
                    return models;
                else
                    return models.OrderBy(model => 0);

            return models.OrderBy(Filter.Sort + " " + Filter.Order);
        }

        public virtual IQueryable<T> Page(IQueryable<T> models)
        {
            Filter.Offset = Math.Max(0, Filter.Offset);
            Filter.Rows = Math.Max(1, Math.Min(Filter.Rows, 100));

            return models.Skip(Filter.Offset).Take(Filter.Rows);
        }

        public virtual LookupData FormLookupData(IQueryable<T> filtered, IQueryable<T> selected, IQueryable<T> notSelected)
        {
            LookupData data = new LookupData();
            data.Columns = Columns;

            foreach (T model in selected)
                data.Selected.Add(FormData(model));

            foreach (T model in notSelected)
                data.Rows.Add(FormData(model));

            return data;
        }

        public virtual Dictionary<String, String?> FormData(T model)
        {
            Dictionary<String, String?> data = new Dictionary<String, String?>();

            foreach (LookupColumn column in Columns)
                data[column.Key] = GetValue(model, column.Key);

            data["Label"] = GetLabel(model);
            data["Id"] = GetId(model);

            return data;
        }

        private String? GenerateQuery(LookupColumn column, PropertyInfo? property, Int32 index)
        {
            if (property?.PropertyType == typeof(String))
            {
                LookupFilterPredicate predicate = column.FilterPredicate == LookupFilterPredicate.Unspecified ? FilterPredicate : column.FilterPredicate;

                return $"({column.Key} != null && {ConvertCase(column)}.{predicate}(@{index}))";
            }

            return IsFilterable(property?.PropertyType) ? $"{column?.Key} = @{index}" : null;
        }
        private Object? ParseSearch(LookupColumn column, PropertyInfo? property)
        {
            if (property?.PropertyType == typeof(String))
            {
                LookupFilterCase filterCase = column.FilterCase == LookupFilterCase.Unspecified ? FilterCase : column.FilterCase;

                return filterCase switch
                {
                    LookupFilterCase.Upper => Filter.Search?.ToUpper(),
                    LookupFilterCase.Lower => Filter.Search?.ToLower(),
                    _ => Filter.Search
                };
            }

            try
            {
                return TypeDescriptor.GetConverter(property?.PropertyType).ConvertFrom(Filter.Search);
            }
            catch
            {
                return null;
            }
        }
        private String? GetValue(T model, String propertyName)
        {
            PropertyInfo? property = typeof(T).GetProperty(propertyName);
            if (property == null)
                return null;

            if (property.GetCustomAttribute<LookupColumnAttribute>(false) is LookupColumnAttribute column && column.Format != null)
                return String.Format(column.Format, property.GetValue(model));

            return property.GetValue(model)?.ToString();
        }
        private Object Parse(Type type, IList<String> ids)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            IList values = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type))!;

            foreach (String value in ids)
                values.Add(converter.ConvertFrom(value));

            return values;
        }
        private String ConvertCase(LookupColumn column)
        {
            LookupFilterCase filterCase = column.FilterCase == LookupFilterCase.Unspecified ? FilterCase : column.FilterCase;

            return filterCase switch
            {
                LookupFilterCase.Upper => $"{column.Key}.ToUpper()",
                LookupFilterCase.Lower => $"{column.Key}.ToLower()",
                _ => column.Key
            };
        }
        private Boolean IsFilterable(Type? type)
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
                case TypeCode.String:
                case TypeCode.DateTime:
                    return true;
                default:
                    return type == typeof(Guid);
            }
        }
    }
}
