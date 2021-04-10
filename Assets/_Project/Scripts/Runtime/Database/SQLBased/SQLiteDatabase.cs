﻿using Dapper;
using Dapper.Contrib.Extensions;
using Mono.Data.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Arcade
{
    public sealed class SQLiteDatabase
    {
        public abstract class MappedEntry
        {
            [Key] public string Id { get; set; }
        }

        public abstract class ReflectedEntry
        {
        }

        private readonly string _connectionString;

        public SQLiteDatabase(string databasePath) => _connectionString = $"Data Source={Path.GetFullPath(databasePath)},Version=3";

        public void CreateTable(string tableName, bool failsIfExists, params string[] columns)
        {
            string statement = $"CREATE TABLE {(failsIfExists ? string.Empty : "IF NOT EXISTS ")}'{tableName}'({string.Join(",", columns)});";

            using IDbConnection connection = GetConnection();
            _ = connection.Execute(statement);
        }

        public bool DropTable(string tableName, bool failsIfNotExists)
        {
            string statement = $"DROP TABLE {(failsIfNotExists ? string.Empty : "IF EXISTS ")}'{tableName}';";

            using IDbConnection connection = GetConnection();
            return connection.Execute(statement) > 0;
        }

        public int GetId<TParameter>(string tableName, string where, TParameter parameter) where TParameter : MappedEntry
        {
            string statement             = $"SELECT Id FROM {tableName} WHERE {where}=@{where};";
            DynamicParameters parameters = new DynamicParameters(parameter);

            using IDbConnection connection = GetConnection();
            return connection.QueryFirst<int>(statement, parameters);
        }

        public void Insert<T>(T item) where T : MappedEntry
        {
            using IDbConnection connection = GetConnection();
            _ = connection.Insert(item);
        }

        public void Insert<T>(IEnumerable<T> items) where T : MappedEntry
        {
            using IDbConnection connection = GetConnection();
            connection.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            _ = connection.Insert(items, transaction);
            transaction.Commit();
        }

        public void Insert<T>(string tableName, T item) where T : ReflectedEntry
        {
            string statement = GetInsertIntoStatement<T>(tableName);

            using IDbConnection connection = GetConnection();
            _ = connection.Execute(statement, item);
        }

        public void Insert<T>(string tableName, IEnumerable<T> items) where T : ReflectedEntry
        {
            string statement = GetInsertIntoStatement<T>(tableName);

            using IDbConnection connection = GetConnection();
            connection.Open();

            using IDbTransaction transaction = connection.BeginTransaction();
            _ = connection.Execute(statement, items, transaction);

            transaction.Commit();
        }

        private IDbConnection GetConnection() => new SqliteConnection(_connectionString);

        private static string GetInsertIntoStatement<T>(string tableName) where T : ReflectedEntry
        {
            (string columns, string parameters) = GetTypeColumnsAndParameters<T>();
            return $"INSERT INTO '{tableName}'({columns}) VALUES({parameters});";
        }

        private static (string, string) GetTypeColumnsAndParameters<T>() where T : ReflectedEntry
        {
            IEnumerable<string> columns    = GetTypeProperties<T>();
            IEnumerable<string> parameters = GetParameterNames(columns);
            string columnsString           = JoinWithCommas(columns);
            string parametersString        = JoinWithCommas(parameters);
            return (columnsString, parametersString);
        }

        private static IEnumerable<string> GetTypeProperties<T>() where T : ReflectedEntry
        {
            PropertyInfo[] propertiesInfo = typeof(T).GetProperties();
            return propertiesInfo.Select(x => x.Name);
        }

        private static IEnumerable<string> GetParameterNames(IEnumerable<string> columns) => columns.Select(x => $"@{x}");

        private static string JoinWithCommas(IEnumerable<string> values) => string.Join(",", values);
    }
}
