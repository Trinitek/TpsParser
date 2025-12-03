using System;

namespace TpsParser;

/// <summary></summary>
public interface IRecordPayload
{
    /// <summary>
    /// Gets the memory region that backs this payload.
    /// </summary>
    ReadOnlyMemory<byte> PayloadData { get; }
}

/// <summary>
/// A payload that has a table number.
/// </summary>
public interface IPayloadTableNumber
{
    /// <summary>
    /// Gets the table number to which this record belongs.
    /// </summary>
    int TableNumber { get; }
}

/// <summary>
/// A payload that has a record number.
/// </summary>
public interface IPayloadRecordNumber
{
    /// <summary>
    /// Gets the record number to which this record belongs.
    /// </summary>
    int RecordNumber { get; }
}
