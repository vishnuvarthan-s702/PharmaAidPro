using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Globalization;
using System.Reflection;

namespace Dapper
{
    public static class SqlMapper
    {
        public static int Execute(this SQLiteConnection conn, string sql, object param = null, SQLiteTransaction transaction = null)
        {
            using (var cmd = CreateCommand(conn, sql, param, transaction))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public static T ExecuteScalar<T>(this SQLiteConnection conn, string sql, object param = null, SQLiteTransaction transaction = null)
        {
            using (var cmd = CreateCommand(conn, sql, param, transaction))
            {
                object val = cmd.ExecuteScalar();
                return ConvertValue<T>(val);
            }
        }

        public static List<T> Query<T>(this SQLiteConnection conn, string sql, object param = null, SQLiteTransaction transaction = null) where T : new()
        {
            var list = new List<T>();
            using (var cmd = CreateCommand(conn, sql, param, transaction))
            using (var reader = cmd.ExecuteReader())
            {
                var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
                var propMap = new Dictionary<string, PropertyInfo>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in props)
                {
                    if (p.CanWrite)
                    {
                        propMap[p.Name] = p;
                    }
                }

                int fieldCount = reader.FieldCount;
                var colProps = new PropertyInfo[fieldCount];
                for (int i = 0; i < fieldCount; i++)
                {
                    PropertyInfo matched;
                    if (propMap.TryGetValue(reader.GetName(i), out matched))
                    {
                        colProps[i] = matched;
                    }
                }

                while (reader.Read())
                {
                    var item = new T();
                    for (int i = 0; i < fieldCount; i++)
                    {
                        var prop = colProps[i];
                        if (prop == null) continue;
                        object raw = reader.GetValue(i);
                        if (raw == null || raw is DBNull) continue;
                        object converted = ConvertToType(raw, prop.PropertyType);
                        prop.SetValue(item, converted, null);
                    }
                    list.Add(item);
                }
            }
            return list;
        }

        public static T QuerySingle<T>(this SQLiteConnection conn, string sql, object param = null, SQLiteTransaction transaction = null) where T : new()
        {
            var list = Query<T>(conn, sql, param, transaction);
            if (list.Count == 0)
            {
                throw new InvalidOperationException("QuerySingle returned no elements.");
            }
            return list[0];
        }

        public static T QuerySingleOrDefault<T>(this SQLiteConnection conn, string sql, object param = null, SQLiteTransaction transaction = null) where T : new()
        {
            var list = Query<T>(conn, sql, param, transaction);
            return list.Count > 0 ? list[0] : default(T);
        }

        public static T QueryFirstOrDefault<T>(this SQLiteConnection conn, string sql, object param = null, SQLiteTransaction transaction = null) where T : new()
        {
            var list = Query<T>(conn, sql, param, transaction);
            return list.Count > 0 ? list[0] : default(T);
        }

        private static SQLiteCommand CreateCommand(SQLiteConnection conn, string sql, object param, SQLiteTransaction transaction)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }
            if (param != null)
            {
                var props = param.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var p in props)
                {
                    object val = p.GetValue(param, null);
                    cmd.Parameters.AddWithValue("@" + p.Name, val ?? DBNull.Value);
                }
            }
            return cmd;
        }

        private static T ConvertValue<T>(object val)
        {
            if (val == null || val is DBNull)
            {
                return default(T);
            }
            return (T)ConvertToType(val, typeof(T));
        }

        private static object ConvertToType(object val, Type targetType)
        {
            Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlying.IsInstanceOfType(val))
            {
                return val;
            }
            if (underlying == typeof(string))
            {
                return Convert.ToString(val, CultureInfo.InvariantCulture);
            }
            if (underlying == typeof(int))
            {
                return Convert.ToInt32(val, CultureInfo.InvariantCulture);
            }
            if (underlying == typeof(long))
            {
                return Convert.ToInt64(val, CultureInfo.InvariantCulture);
            }
            if (underlying == typeof(decimal))
            {
                return Convert.ToDecimal(val, CultureInfo.InvariantCulture);
            }
            if (underlying == typeof(double))
            {
                return Convert.ToDouble(val, CultureInfo.InvariantCulture);
            }
            if (underlying == typeof(bool))
            {
                return Convert.ToBoolean(val, CultureInfo.InvariantCulture);
            }
            return Convert.ChangeType(val, underlying, CultureInfo.InvariantCulture);
        }
    }
}
