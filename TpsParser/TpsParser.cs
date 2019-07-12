using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TpsParser.Tps;
using TpsParser.Tps.Record;
using TpsParser.Tps.Type;

namespace TpsParser
{
    public sealed class TpsParser : IDisposable
    {
        public TpsFile TpsFile { get; }

        private Stream Stream { get; }

        public TpsParser(Stream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TpsFile = new TpsFile(Stream);
        }

        public TpsParser(Stream stream, string password)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TpsFile = new TpsFile(Stream, new Key(password));
        }

        public TpsParser(string filename)
        {
            Stream = new FileStream(filename, FileMode.Open);
            TpsFile = new TpsFile(Stream);
        }

        public TpsParser(string filename, string password)
        {
            Stream = new FileStream(filename, FileMode.Open);
            TpsFile = new TpsFile(Stream, new Key(password));
        }

        private IEnumerable<(int recordNumber, IReadOnlyDictionary<string, TpsObject> nameValuePairs)> GatherDataRecords(int table, TableDefinitionRecord tableDefinitionRecord, bool ignoreErrors)
        {
            var dataRecords = TpsFile.GetDataRecords(table, tableDefinition: tableDefinitionRecord, ignoreErrors);

            return dataRecords.Select(r => (r.RecordNumber, r.GetFieldValuePairs()));
        }

        private IEnumerable<(int recordNumber, IReadOnlyDictionary<string, TpsObject> nameValuePairs)> GatherMemoRecords(int table, TableDefinitionRecord tableDefinitionRecord, bool ignoreErrors)
        {
            var memoRecords = TpsFile.GetMemoRecords(table, ignoreErrors);

            return tableDefinitionRecord.Memos
                .Zip(memoRecords, (memoDef, memoRec) => (owner: memoRec.Owner, name: memoDef.Name, value: memoRec.GetValue(memoDef)))
                .GroupBy(pair => pair.owner, pair => (pair.name, pair.value))
                .Select(groupedPair => (
                    groupedPair.Key,
                    (IReadOnlyDictionary<string, TpsObject>)groupedPair
                        .ToDictionary(p => p.name, p => p.value)));
        }

        public Table BuildTable(bool ignoreErrors = false)
        {
            var tableNameDefinitions = TpsFile.GetTableNameRecords();

            var tableDefinitions = TpsFile.GetTableDefinitions(ignoreErrors: ignoreErrors);

            var firstTableDefinition = tableDefinitions.First();

            var dataRecords = GatherDataRecords(firstTableDefinition.Key, firstTableDefinition.Value, ignoreErrors);
            var memoRecords = GatherMemoRecords(firstTableDefinition.Key, firstTableDefinition.Value, ignoreErrors);

            IEnumerable<(int recordNumber, IReadOnlyDictionary<string, TpsObject> nameValuePairs)> unifiedRecords = Enumerable.Concat(dataRecords, memoRecords)
                .GroupBy(numberNVPairs => numberNVPairs.recordNumber)
                .Select(groupedNumberNVPairs => (
                    recordNumber: groupedNumberNVPairs.Key,
                    nameValuePairs: (IReadOnlyDictionary<string, TpsObject>)groupedNumberNVPairs
                        .SelectMany(pair => pair.nameValuePairs)
                        .ToDictionary(kv => kv.Key, kv => kv.Value)));

            var rows = unifiedRecords.Select(r => new Row(r.recordNumber, r.nameValuePairs));

            string tableName = tableNameDefinitions
                .First(n => n.TableNumber == firstTableDefinition.Key).Header.Name;

            var table = new Table(tableName, rows);

            return table;
        }

        public IEnumerable<T> Deserialize<T>(bool ignoreErrors = false) where T : class, new()
        {
            var targetClass = typeof(T);

            var tpsTableAttr = targetClass.GetCustomAttribute(typeof(TpsTableAttribute));

            if (tpsTableAttr is null)
            {
                throw new Exception($"The given class is not marked with {nameof(TpsTableAttribute)}.");
            }

            var table = BuildTable(ignoreErrors);

            var targetObjects = table.Rows
                .Select(r =>
                {
                    var targetObject = new T();

                    SetProperties(targetObject, r);
                    SetFields(targetObject, r);

                    return targetObject;
                });
            
            return targetObjects;
        }

        private void SetProperties<T>(T targetObject, Row row)
        {
            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(p => (p, tpsFieldAttr: p.GetCustomAttribute<TpsFieldAttribute>()))
                    .Where(pair => pair.tpsFieldAttr != null);

            foreach (var property in properties)
            {
                if (!property.p.CanWrite)
                {
                    throw new Exception($"The property '{property.p.Name}' must have a setter.");
                }

                var tpsFieldName = property.tpsFieldAttr.FieldName;
                var tpsFieldValue = row.GetValueCaseInsensitive(tpsFieldName, property.tpsFieldAttr.IsRequired);

                property.p.SetValue(targetObject, CoerceValue(tpsFieldValue?.Value, property.tpsFieldAttr.FallbackValue));
            }
        }

        private void SetFields<T>(T targetObject, Row row)
        {
            var fields = typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Select(f => (f, tpsFieldAttr: f.GetCustomAttribute<TpsFieldAttribute>()))
                .Where(pair => pair.tpsFieldAttr != null);

            foreach (var field in fields)
            {
                var tpsFieldName = field.tpsFieldAttr.FieldName;
                var tpsFieldValue = row.GetValueCaseInsensitive(tpsFieldName, field.tpsFieldAttr.IsRequired);

                field.f.SetValue(targetObject, CoerceValue(tpsFieldValue?.Value, field.tpsFieldAttr.FallbackValue));
            }
        }

        private object CoerceValue(object value, object fallback)
        {
            if (fallback != null)
            {
                return value ?? fallback;
            }
            else
            {
                return value;
            }
        }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}
