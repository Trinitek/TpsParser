﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private DeserializerContext DeserializerContext { get; }

        private TpsParser()
        {
            DeserializerContext = new DeserializerContext();
        }

        internal TpsParser(TpsFile tpsFile)
            : this()
        {
            TpsFile = tpsFile ?? throw new ArgumentNullException(nameof(tpsFile));
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="stream">The stream from which to read the TopSpeed file.</param>
        public TpsParser(Stream stream)
            : this()
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TpsFile = new ImplTpsFile(Stream);
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="stream">The stream from which to read the TopSpeed file.</param>
        /// <param name="password">The password or "owner" to use to decrypt the file.</param>
        public TpsParser(Stream stream, string password)
            : this()
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
            TpsFile = new ImplTpsFile(Stream, new Key(password));
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="filename">The filename of the TopSpeed file.</param>
        public TpsParser(string filename)
            : this()
        {
            Stream = new FileStream(filename, FileMode.Open);
            TpsFile = new ImplTpsFile(Stream);
        }

        /// <summary>
        /// Instantiates a new parser.
        /// </summary>
        /// <param name="filename">The filename of the TopSpeed file.</param>
        /// <param name="password">The password or "owner" to use to decrypt the file.</param>
        public TpsParser(string filename, string password)
            : this()
        {
            Stream = new FileStream(filename, FileMode.Open);
            TpsFile = new ImplTpsFile(Stream, new Key(password));
        }

        private IReadOnlyDictionary<int, IReadOnlyDictionary<string, TpsObject>> GatherDataRecords(int table, ITableDefinitionRecord tableDefinitionRecord, bool ignoreErrors)
        {
            var dataRecords = TpsFile.GetDataRecords(table, tableDefinitionRecord: tableDefinitionRecord, ignoreErrors);

            return dataRecords.ToDictionary(r => r.RecordNumber, r => r.GetFieldValuePairs());
        }

        private IReadOnlyDictionary<int, IReadOnlyDictionary<string, TpsObject>> GatherMemoRecords(int table, ITableDefinitionRecord tableDefinitionRecord, bool ignoreErrors)
        {
            return Enumerable.Range(0, tableDefinitionRecord.Memos.Count())
                .SelectMany(index =>
                {
                    var definition = tableDefinitionRecord.Memos[index];
                    var memoRecordsForIndex = TpsFile.GetMemoRecords(table, index, ignoreErrors);

                    return memoRecordsForIndex.Select(record => (owner: record.Header.OwningRecord, name: definition.Name, value: record.GetValue(definition)));
                })
                .GroupBy(pair => pair.owner, pair => (pair.name, pair.value))
                .ToDictionary(
                    groupedPair => groupedPair.Key,
                    groupedPair => (IReadOnlyDictionary<string, TpsObject>)groupedPair
                        .ToDictionary(pair => pair.name, pair => pair.value));
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

            var unifiedRecords = new Dictionary<int, Dictionary<string, TpsObject>>();

            foreach (var dataKvp in dataRecords)
            {
                unifiedRecords.Add(dataKvp.Key, dataKvp.Value.ToDictionary(pair => pair.Key, pair => pair.Value));
            }

            foreach (var memoRecord in memoRecords)
            {
                int recordNumber = memoRecord.Key;

                var dataNameValues = dataRecords[recordNumber];

                foreach (var memoNameValue in memoRecord.Value)
                {
                    unifiedRecords[recordNumber].Add(memoNameValue.Key, memoNameValue.Value);
                }
            }

            var rows = unifiedRecords.Select(r => new Row(DeserializerContext, r.Key, r.Value));

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
        public IEnumerable<T> Deserialize<T>(bool ignoreErrors = false) where T : class, new() =>
            DeserializeInternal<T>(ignoreErrors, ct: default);

        /// <summary>
        /// Deserializes the first table in the file to a collection of the given type.
        /// </summary>
        /// <typeparam name="T">The type to represent a deserialized row.</typeparam>
        /// <param name="ignoreErrors">If true, the reader will not throw an exception when it encounters unexpected data.</param>
        /// <param name="ct">The cancellation token for the asynchronous operation.</param>
        /// <returns></returns>
        public Task<IEnumerable<T>> DeserializeAsync<T>(bool ignoreErrors = false, CancellationToken ct = default) where T : class, new() =>
            Task.FromResult(DeserializeInternal<T>(ignoreErrors, ct));

        internal IEnumerable<T> DeserializeInternal<T>(bool ignoreErrors, CancellationToken ct) where T : class, new()
        {
            var firstTableDefinition = TpsFile.GetTableDefinitions(ignoreErrors).First();

            var dataRecords = TpsFile.GetDataRecords(firstTableDefinition.Key, firstTableDefinition.Value, ignoreErrors)
                .ToDictionary(r => r.RecordNumber, r => r.Values);

            var memoRecords = GatherMemoRecords(firstTableDefinition.Key, firstTableDefinition.Value, ignoreErrors);

            var columns = BuildColumnLookup(firstTableDefinition.Value.Fields.Select(f => f.Name.ToUpperInvariant()));

            foreach (var dataRecord in dataRecords)
            {
                int recordNumber = dataRecord.Key;
                var values = dataRecord.Value;

                T target = new T();

                var members = DeserializerContext.GetModelMembers<T>(target);

                foreach (var member in members)
                {
                    if (member.IsRecordNumber)
                    {
                        member.SetMember(target, recordNumber);
                    }
                    else
                    {
                        string requestedField = member.FieldAttribute.FieldName.ToUpperInvariant();

                        if (columns.TryGetValue(requestedField, out int index))
                        {
                            object interpretedValue = member.FieldAttribute.InterpretValue(member.MemberInfo, values[index]);
                            member.SetMember(target, interpretedValue);
                        }
                        else if (memoRecords.TryGetValue(recordNumber, out var fieldValueDictionary)
                            && fieldValueDictionary.ToDictionary(fv => fv.Key.ToUpperInvariant(), fv => fv.Value).TryGetValue(requestedField, out TpsObject value))
                        {
                            object interpretedValue = member.FieldAttribute.InterpretValue(member.MemberInfo, value);
                            member.SetMember(target, interpretedValue);
                        }
                        else if (member.FieldAttribute.IsRequired)
                        {
                            throw new TpsParserException($"The required field '{requestedField}' could not be found in record {recordNumber}.");
                        }
                    }

                    ct.ThrowIfCancellationRequested();
                }

                yield return target;

                ct.ThrowIfCancellationRequested();
            }
        }

        private IReadOnlyDictionary<string, int> BuildColumnLookup(IEnumerable<string> columns)
        {
            var lookup = new Dictionary<string, int>();

            int i = 0;

            foreach (var column in columns)
            {
                lookup.Add(column, i);
                i++;
            }

            return lookup;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public void Dispose()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            Stream?.Dispose();
        }
    }
}
