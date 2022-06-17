using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;

namespace Mahas.Helpers
{
    public class MahasConnection : IDisposable
    {
        private string _ConString { get; set; }
        private SqlConnection _Cnn { get; set; }
        public SqlTransaction Transaction { get; set; }
        public SqlCommand Command { get; set; }

        public MahasConnection(string constring)
        {
            _ConString = constring;
            _Cnn = new SqlConnection(_ConString);
        }

        public SqlParameter Param(string parameterName, object value)
        {
            if (value == null)
                return new SqlParameter(parameterName, DBNull.Value);
            else
                return new SqlParameter(parameterName, value);
        }

        public async Task<int> GetTotalRowConst(string query, List<SqlParameter> parameters = null)
        {
            query = $@"
                SELECT COUNT(*) AS Count
                FROM ({query}) AS ALIAS
            ";
            _Cnn.Open();
            int r;
            using (var comm = new SqlCommand(query, _Cnn))
            {
                if (parameters != null)
                {
                    foreach (var x in parameters)
                        comm.Parameters.Add(x);
                }
                r = (int)await comm.ExecuteScalarAsync();
                comm.Parameters.Clear();
            }
            _Cnn.Close();
            return r;
        }


        public async Task<List<T>> GetDatas<T>(string query, params SqlParameter[] parameters)
            where T : class, new()
        {
            return await GetDatas<T>(query, parameters.ToList());
        }

        public async Task<List<T>> GetDatas<T>(string query, string orderBy, OrderByTypeEnum orderByType, int pageIndex = 0, int pageSize = 1000, params SqlParameter[] parameters)
            where T : class, new()
        {
            return await GetDatas<T>(query, orderBy, orderByType, pageIndex, pageSize, parameters.ToList());
        }

        public async Task<List<T>> GetDatas<T>(string query, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            _Cnn.Open();
            List<T> r;
            using (var comm = new SqlCommand(query, _Cnn))
            {
                r = await GetDatas<T>(comm, parameters);
            }
            _Cnn.Close();
            return r;
        }

        private static async Task<List<T>> GetDatas<T>(SqlCommand comm, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            List<T> r;
            if (parameters != null)
            {
                foreach (var x in parameters)
                    comm.Parameters.Add(x);
            }
            var reader = await comm.ExecuteReaderAsync();
            r = new List<T>();
            while (await reader.ReadAsync())
            {
                var obj = new T();
                var objType = obj.GetType();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    object value;
                    if (reader.GetValue(i) is DBNull)
                    {
                        value = null;
                    }
                    else
                    {
                        value = reader.GetValue(i);
                    }
                    objType.GetProperty(reader.GetName(i))?.SetValue(obj, value);
                }
                r.Add(obj);
            }
            comm.Parameters.Clear();
            return r;
        }

        public async Task<List<T>> GetDatas<T>(string query, string orderBy, OrderByTypeEnum orderByType, int pageIndex = 0, int pageSize = 1000, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            _Cnn.Open();
            List<T> r;
            query = $@"
                    SELECT
                        ROW_NUMBER() OVER (ORDER BY {orderBy} {orderByType}) AS RowNumber,
                        *
                    FROM ({query}) AS ALIAS
                ";
            query = $@"
                    WITH Alias AS ({query})
                    SELECT TOP {pageSize}
                        *
                    FROM
                        Alias
                    WHERE
                        RowNumber > {pageIndex * pageSize}
                ";
            using (var comm = new SqlCommand(query, _Cnn))
            {
                if (parameters != null)
                {
                    foreach (var x in parameters)
                        comm.Parameters.Add(x);
                }
                var reader = await comm.ExecuteReaderAsync();
                r = new List<T>();
                while (await reader.ReadAsync())
                {
                    var obj = new T();
                    var objType = obj.GetType();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        object value;
                        if (reader.GetValue(i) is DBNull)
                        {
                            value = null;
                        }
                        else
                        {
                            value = reader.GetValue(i);
                        }
                        objType.GetProperty(reader.GetName(i))?.SetValue(obj, value);
                    }
                    r.Add(obj);
                }
                comm.Parameters.Clear();
            }
            _Cnn.Close();
            return r;
        }

        public async Task<T> GetData<T>(string query, params SqlParameter[] parameters)
            where T : class, new()
        {
            return await GetData<T>(query, parameters.ToList());
        }

        public async Task<T> GetData<T>(string query, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            _Cnn.Open();
            T r = null;
            using (var comm = new SqlCommand(query, _Cnn))
            {
                r = await GetData<T>(comm, parameters);
            }
            _Cnn.Close();
            return r;
        }

        private static async Task<T> GetData<T>(SqlCommand comm, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            T r = null;
            if (parameters != null)
            {
                foreach (var x in parameters)
                    comm.Parameters.Add(x);
            }
            var reader = await comm.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                r = new T();
                var objType = r.GetType();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        object value;
                        if (reader.GetValue(i) is DBNull)
                        {
                            value = null;
                        }
                        else
                        {
                            value = reader.GetValue(i);
                        }
                        objType.GetProperty(reader.GetName(i))?.SetValue(r, value);
                    }
                    catch (TargetException ex)
                    {
                        var mee = ex.Message;
                    }
                }
                break;
            }
            reader.Close();
            return r;
        }

        public async Task<T> GetDataTransaction<T>(string query, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            Command.CommandText = query;
            Command.Parameters.Clear();
            var r = await GetData<T>(Command, parameters);
            return r;
        }

        public async Task<List<T>> GetDatasTransaction<T>(string query, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            Command.CommandText = query;
            Command.Parameters.Clear();
            var r = await GetDatas<T>(Command, parameters);
            return r;
        }

        public void OpenTransaction()
        {
            _Cnn.Open();
            Command = new SqlCommand();
            Transaction = _Cnn.BeginTransaction("MahasTransaction");
            Command.Connection = _Cnn;
            Command.Transaction = Transaction;
        }

        public void CommitTransaction()
        {
            Transaction.Commit();
            _Cnn.Close();
        }

        public async Task<T> ExecuteNonQuery<T>(string query, List<SqlParameter> parameters = null)
            where T : class, new()
        {
            Command.CommandText = query;
            Command.Parameters.Clear();
            if (parameters != null)
            {
                foreach (var x in parameters)
                {
                    if (x.Value == null)
                        x.Value = DBNull.Value;
                    Command.Parameters.Add(x);
                }
            }
            T r = null;
            var reader = await Command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                r = new T();
                var objType = r.GetType();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    try
                    {
                        object value;
                        if (reader.GetValue(i) is DBNull)
                        {
                            value = null;
                        }
                        else
                        {
                            value = reader.GetValue(i);
                        }
                        objType.GetProperty(reader.GetName(i))?.SetValue(r, value);
                    }
                    catch (TargetException ex)
                    {
                        var mee = ex.Message;
                    }
                }
                break;
            }
            return r;
        }

        public async Task ExecuteNonQuery(string query, List<SqlParameter> parameters = null)
        {
            Command.CommandText = query;
            Command.Parameters.Clear();
            if (parameters != null)
            {
                foreach (var x in parameters)
                {
                    if (x.Value == null)
                        x.Value = DBNull.Value;
                    Command.Parameters.Add(x);
                }
            }
            await Command.ExecuteNonQueryAsync();
        }

        public async Task<int> ExecuteNonQueryAutoIncrement(string query, List<SqlParameter> parameters = null)
        {
            Command.CommandText = query;
            Command.Parameters.Clear();
            if (parameters != null)
            {
                foreach (var x in parameters)
                {
                    if (x.Value == null)
                        x.Value = DBNull.Value;
                    Command.Parameters.Add(x);
                }
            }
            return (int)await Command.ExecuteScalarAsync();
        }

        public string ToWhere(List<string> wheres)
        {
            if (wheres.Count == 0) return "";
            var where = string.Join(" AND ", wheres);
            return "WHERE " + where;
        }

        #region ===== A U T O

        private class AutoDbProperties
        {
            public string Table { get; set; }
            public List<string> Keys { get; set; }
            public List<string> Columns { get; set; }
            public List<SqlParameter> Parameters { get; set; }
        }

        private enum AutoDbState { Insert, Update, Delete }

        private AutoDbProperties AutoDbGetProperties<T>(T model, AutoDbState state) where T : class, new()
        {
            var r = new AutoDbProperties
            {
                Table = typeof(T).GetCustomAttribute<DbTableAttribute>()?.Name ?? typeof(T).Name,
                Keys = new List<string>(),
                Columns = new List<string>(),
                Parameters = new List<SqlParameter>()
            };

            foreach (var prop in typeof(T).GetProperties())
            {
                var keyAtt = prop.GetCustomAttribute<DbKeyAttribute>();
                var columnAtt = prop.GetCustomAttribute<DbColumnAttribute>();
                var key = keyAtt?.Key ?? false;
                var autoIncrement = keyAtt?.AutoIncrement ?? false;
                var column = columnAtt?.Name ?? prop.Name;
                var create = columnAtt?.Create ?? false;
                var update = columnAtt?.Update ?? false;
                var isImage = columnAtt?.IsImage ?? false;
                var p = Param($"@{column}", prop.GetValue(model));
                if (isImage)
                {
                    p.SqlDbType = SqlDbType.Image;
                }
                if (state == AutoDbState.Update)
                {
                    if (key)
                    {
                        r.Keys.Add(column);
                        r.Parameters.Add(p);
                    }
                    else
                    {
                        if (update)
                        {
                            r.Columns.Add(column);
                            r.Parameters.Add(p);
                        }
                    }
                }
                else if (state == AutoDbState.Insert)
                {
                    if (!autoIncrement && create)
                    {
                        r.Columns.Add(column);
                        r.Parameters.Add(p);
                    }
                }
                else
                {
                    if (key)
                    {
                        r.Keys.Add(column);
                        r.Parameters.Add(p);
                    }
                }
            }
            return r;
        }

        public async Task<int> Insert<T>(T model, bool autoIncrement = false, params string[] exclude) where T : class, new()
        {
            var validation = Validation(model);
            if (!validation.IsValid) throw new Exception(validation.Message);

            var m = AutoDbGetProperties(model, AutoDbState.Insert);
            var columns = m.Columns.Where(x => !exclude.Any(y => y == x)).ToList();
            var query = $"INSERT INTO {m.Table} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", columns.Select(x => "@" + x))})";
            if (autoIncrement) query += "; SELECT SCOPE_IDENTITY();";
            Command.CommandText = query;
            Command.Parameters.Clear();
            foreach (var x in m.Parameters)
                Command.Parameters.Add(x);
            if (autoIncrement)
                return (int)(decimal)await Command.ExecuteScalarAsync();
            else
                return await Command.ExecuteNonQueryAsync();
        }

        public async Task Update<T>(T model, params string[] exclude) where T : class, new()
        {
            var validation = Validation(model);
            if (!validation.IsValid) throw new Exception(validation.Message);

            var m = AutoDbGetProperties(model, AutoDbState.Update);
            var columns = m.Columns.Where(x => !exclude.Any(y => y == x)).ToList();
            Command.CommandText = $"UPDATE {m.Table} SET {string.Join(", ", columns.Where(x => !exclude.Any(y => y == x)).Select(x => $"{x}=@{x}"))} WHERE {string.Join(" AND ", m.Keys.Select(x => $"{x}=@{x}"))}";
            Command.Parameters.Clear();
            foreach (var x in m.Parameters)
                Command.Parameters.Add(x);
            await Command.ExecuteNonQueryAsync();
        }

        public async Task Delete<T>(T model) where T : class, new()
        {
            var m = AutoDbGetProperties(model, AutoDbState.Delete);
            Command.CommandText = $"DELETE FROM {m.Table} WHERE {string.Join(" AND ", m.Keys.Select(x => $"{x}=@{x}"))}";
            Command.Parameters.Clear();
            foreach (var x in m.Parameters)
                Command.Parameters.Add(x);
            await Command.ExecuteNonQueryAsync();
        }

        #endregion

        public static DbValidationModel Validation<T>(T model) where T : class, new()
        {
            var isValid = true;
            var message = new List<string>();
            foreach (var prop in typeof(T).GetProperties())
            {
                var required = prop.GetCustomAttribute<DbRequiredAttribute>();
                var displayName = prop.GetCustomAttribute<DbDisplayNameAttribute>()?.DisplayName ?? prop.Name;
                var value = prop.GetValue(model);
                if (required != null)
                {
                    if (prop.PropertyType == typeof(string))
                    {
                        if (string.IsNullOrEmpty((string)value))
                        {
                            isValid = false;
                            message.Add($"{displayName} is Required");
                        }
                    }
                    else
                    {
                        if (value == null)
                        {
                            isValid = false;
                            message.Add($"{displayName} is Required");
                        }
                    }
                }
            }
            return new DbValidationModel(isValid, message);
        }

        public void Dispose()
        {
            if (Command != null) Command.Dispose();
            if (Transaction != null) Transaction.Dispose();
            _Cnn.Close();
            _Cnn.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    public class DbValidationModel
    {
        public bool IsValid { get; set; }
        private List<string> Msg { get; set; }

        public DbValidationModel(bool isValid, List<string> message)
        {
            IsValid = isValid;
            Msg = message;
        }

        public string Message
        {
            get
            {
                return string.Join("<br/>", Msg);
            }
        }
    }
}