namespace TpsParser;

public interface IRecordPayload;

public interface IPayloadTableNumber
{
    /// <summary>
    /// Gets the table number to which this record belongs.
    /// </summary>
    int TableNumber { get; }
}

public interface IPayloadRecordNumber
{
    /// <summary>
    /// Gets the record number to which this record belongs.
    /// </summary>
    int RecordNumber { get; }
}
