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
    /// <summary>
    /// An easy to use reader and object deserializer for TopSpeed files.
    /// </summary>
    public sealed class TpsParser : IDisposable
    {
        /// <summary>
        /// Gets the low level representation of the TopSpeed file and its data structures.
        /// </summary>
        public TpsFile TpsFile { get; }

        private Stream Stream { get; }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="stream">The stream from which to read the TopSpeed file.</param>
        public TpsParser(Stream stream)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TpsFile = new TpsFile(Stream);
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="stream">The stream from which to read the TopSpeed file.</param>
        /// <param name="password">The password or "owner" to use to decrypt the file.</param>
        public TpsParser(Stream stream, string password)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TpsFile = new TpsFile(Stream, new Key(password));
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="filename">The filename of the TopSpeed file.</param>
        public TpsParser(string filename)
        {
            Stream = new FileStream(filename, FileMode.Open);
            TpsFile = new TpsFile(Stream);
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="filename">The filename of the TopSpeed file.</param>
        /// <param name="password">The password or "owner" to use to decrypt the file.</param>
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
            return Enumerable.Range(0, tableDefinitionRecord.Memos.Count())
                .SelectMany(index =>
                {
                    var definition = tableDefinitionRecord.Memos[index];
                    var memoRecordsForIndex = TpsFile.GetMemoRecords(table, index, ignoreErrors);

                    return memoRecordsForIndex.Select(record => (owner: record.Header.OwningRecord, name: definition.Name, value: record.GetValue(definition)));
                })
                .GroupBy(pair => pair.owner, pair => (pair.name, pair.value))
                .Select(groupedPair => (
                    groupedPair.Key,
                    (IReadOnlyDictionary<string, TpsObject>)groupedPair
                        .ToDictionary(pair => pair.name, pair => pair.value)));
        }

        /// <summary>
        /// Gets a high level representation of the first table in the file.
        /// </summary>
        /// <param name="ignoreErrors">If true, the reader will not throw an exception when it encounters unexpected data.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deserializes the first table in the file to a collection of the given type.
        /// </summary>
        /// <typeparam name="T">The type to represent a deserialized row.</typeparam>
        /// <param name="ignoreErrors">If true, the reader will not throw an exception when it encounters unexpected data.</param>
        /// <returns></returns>
        public IEnumerable<T> Deserialize<T>(bool ignoreErrors = false) where T : class, new()
        {
            var targetClass = typeof(T);

            var tpsTableAttr = targetClass.GetCustomAttribute(typeof(TpsTableAttribute));

            if (tpsTableAttr is null)
            {
                throw new TpsParserException($"The given class is not marked with {nameof(TpsTableAttribute)}.");
            }

            var table = BuildTable(ignoreErrors);

            var targetObjects = table.Rows
                .Select(r =>
                {
                    var targetObject = new T();

                    SetMembers(targetObject, r);

                    return targetObject;
                });
            
            return targetObjects;
        }

        private void SetMembers<T>(T targetObject, Row row)
        {
            var members = typeof(T).GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (var member in members)
            {
                var tpsFieldAttr = member.GetCustomAttribute<TpsFieldAttribute>();
                var tpsRecordNumberAttr = member.GetCustomAttribute<TpsRecordNumberAttribute>();

                if (tpsFieldAttr != null && tpsRecordNumberAttr != null)
                {
                    throw new TpsParserException($"Members cannot be marked with both {nameof(TpsFieldAttribute)} and {nameof(TpsRecordNumberAttribute)}. Property name '{member.Name}'.");
                }

                if (tpsFieldAttr != null)
                {
                    var tpsFieldName = tpsFieldAttr.FieldName;
                    var tpsFieldValue = GetRowValue(row, tpsFieldName, tpsFieldAttr.IsRequired);
                    var tpsValue = CoerceValue(tpsFieldValue?.Value, tpsFieldAttr.FallbackValue);

                    SetMember(member, targetObject, tpsValue);
                }
                if (tpsRecordNumberAttr != null)
                {
                    SetMember(member, targetObject, row.Id);
                }
            }
        }

        private void SetMember(MemberInfo member, object target, object value)
        {
            if (member is PropertyInfo prop)
            {
                if (!prop.CanWrite)
                {
                    throw new TpsParserException($"The property '{member.Name}' must have a setter.");
                }

                prop.SetValue(target, value);
            }
            else if (member is FieldInfo field)
            {
                field.SetValue(target, value);
            }
        }

        private TpsObject GetRowValue(Row row, string fieldName, bool isRequired)
        {
            try
            {
                return row.GetValueCaseInsensitive(fieldName, isRequired);
            }
            catch (Exception ex)
            {
                throw new TpsParserException("Unable to deserialize field into class member. See the inner exception for details.", ex);
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

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void Dispose()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Stream.Dispose();
        }
    }
}
