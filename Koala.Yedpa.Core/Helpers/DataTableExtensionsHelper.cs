// Koala.Yedpa.Core/Extensions/DataTableExtensions.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Koala.Yedpa.Core.Extensions
{
    public static class DataTableExtensions
    {
        /// <summary>
        /// DataTable'ı generic List<T>'ye dönüştürür
        /// Kolon isimleri property isimleriyle eşleşmeli (case-insensitive)
        /// </summary>
        public static List<T> AsList<T>(this DataTable table) where T : class, new()
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            var list = new List<T>();
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (DataRow row in table.Rows)
            {
                var item = new T();

                foreach (var prop in properties)
                {
                    if (prop.CanWrite == false) continue;

                    var columnName = prop.Name;
                    if (!table.Columns.Contains(columnName)) continue;

                    var value = row[columnName];
                    if (value == DBNull.Value || value == null)
                    {
                        // Nullable türler için null, değilse default
                        if (prop.PropertyType.IsValueType &&
                            Nullable.GetUnderlyingType(prop.PropertyType) == null)
                        {
                            prop.SetValue(item, Activator.CreateInstance(prop.PropertyType));
                        }
                        else
                        {
                            prop.SetValue(item, null);
                        }
                        continue;
                    }

                    try
                    {
                        var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                        var convertedValue = Convert.ChangeType(value, targetType);
                        prop.SetValue(item, convertedValue);
                    }
                    catch
                    {
                        // Tip dönüşümü başarısız olursa default bırak
                        prop.SetValue(item, null);
                    }
                }

                list.Add(item);
            }

            return list;
        }
    }
}