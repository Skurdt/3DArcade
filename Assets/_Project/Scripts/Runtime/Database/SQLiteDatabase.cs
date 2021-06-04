/* MIT License

 * Copyright (c) 2020 Skurdt
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

using Dapper;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Arcade
{
    public sealed class SQLiteDatabase
    {
        [System.Serializable]
        public abstract class ReflectedEntry
        {
        }

        private readonly string _connectionString;

        public SQLiteDatabase(string databasePath) => _connectionString = $"Data Source={databasePath},Version=3";

        public void CreateTable(string tableName, bool failsIfExists, params string[] columns)
        {
            string statement = $"CREATE TABLE{(failsIfExists ? string.Empty : " IF NOT EXISTS")} '{tableName}'({string.Join(",", columns)});";

            using IDbConnection connection = GetConnection();
            _ = connection.Execute(statement);
        }

        public bool DropTable(string tableName, bool failsIfNotExists)
        {
            string statement = $"DROP TABLE{(failsIfNotExists ? string.Empty : " IF EXISTS")} '{tableName}';";

            using IDbConnection connection = GetConnection();
            return connection.Execute(statement) > 0;
        }

        public IEnumerable<string> GetTables()
        {
            string statement = "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";

            using IDbConnection connection = GetConnection();
            return connection.Query<string>(statement);
        }

        public T Get<T>(string tableName, string[] columns, string[] parameters, object obj)
            where T : class
        {
            StringBuilder statement = new StringBuilder($"SELECT {JoinWithCommas(columns)} FROM '{tableName}' WHERE ");
            for (int i = 0; i < parameters.Length; ++i)
            {
                string parameter = parameters[i];

                if (i == 0)
                {
                    _ = statement.Append($"{parameter} = @{parameter}");
                    continue;
                }

                _ = statement.Append($" AND {parameter} = @{parameter}");
            }
            _ = statement.Append(';');

            try
            {
                using IDbConnection connection = GetConnection();
                return connection.QueryFirst<T>(statement.ToString(), obj);
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public T[] GetTableEntries<T>(string tableName, string[] columns, object obj)
            where T : class
        {
            if (string.IsNullOrEmpty(tableName))
                return null;

            string statement = $"SELECT {JoinWithCommas(columns)} FROM '{tableName}';";

            try
            {
                using IDbConnection connection   = GetConnection();
                using SqlMapper.GridReader multi = connection.QueryMultiple(statement.ToString(), obj);
                return multi.Read<T>().ToArray();
            }
            catch (System.Exception)
            {
                return null;
            }
        }

        public void Insert<T>(string tableName, T item)
            where T : ReflectedEntry
        {
            string statement = GetInsertIntoStatement<T>(tableName);

            using IDbConnection connection = GetConnection();
            _ = connection.Execute(statement, item);
        }

        public void Insert<T>(string tableName, IEnumerable<T> items)
            where T : ReflectedEntry
        {
            string statement = GetInsertIntoStatement<T>(tableName);

            using IDbConnection connection = GetConnection();
            connection.Open();

            using IDbTransaction transaction = connection.BeginTransaction();
            _ = connection.Execute(statement, items, transaction);

            transaction.Commit();
        }

        private IDbConnection GetConnection() => new SqliteConnection(_connectionString);

        private static string GetInsertIntoStatement<T>(string tableName)
            where T : ReflectedEntry
        {
            (string columns, string parameters) = GetTypeColumnsAndParameters<T>();
            return $"INSERT INTO '{tableName}'({columns}) VALUES({parameters});";
        }

        private static (string, string) GetTypeColumnsAndParameters<T>()
            where T : ReflectedEntry
        {
            IEnumerable<string> columns    = GetTypeProperties<T>();
            IEnumerable<string> parameters = GetParameterNames(columns);
            string columnsString           = JoinWithCommas(columns);
            string parametersString        = JoinWithCommas(parameters);
            return (columnsString, parametersString);
        }

        private static IEnumerable<string> GetTypeProperties<T>()
            where T : ReflectedEntry
        {
            PropertyInfo[] propertiesInfo = typeof(T).GetProperties();
            return propertiesInfo.Select(x => x.Name);
        }

        private static IEnumerable<string> GetParameterNames(IEnumerable<string> names) => names.Select(x => $"@{x}");

        private static string JoinWithCommas(IEnumerable<string> values) => string.Join(", ", values);
    }
}
