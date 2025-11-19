namespace TpsParser;

/// <summary></summary>
public sealed record class ErrorHandlingOptions
{
    /// <summary></summary>
    public static readonly ErrorHandlingOptions Default = new();

    /// <summary>
    /// Gets an <see cref="ErrorHandlingOptions"/> instance that uses the strictest error handling behavior, throwing exceptions where possible.
    /// </summary>
    public static readonly ErrorHandlingOptions Strict = new()
    {
        //ThrowOnInvalidStructure = true,
        ThrowOnRleDecompressionError = true,
        RleUndersizedDecompressionBehavior = RleSizeMismatchBehavior.Throw,
        RleOversizedDecompressionBehavior = RleSizeMismatchBehavior.Throw
    };

    // Not currently used...
    // /// <summary>
    // /// Whether to throw an exception when an invalid TPS file structure is encountered.
    // /// If <see langword="false"/>, the parser will attempt to continue parsing despite errors.
    // /// Default is <see langword="true"/>.
    // /// </summary>
    // public bool ThrowOnInvalidStructure { get; init; } = true;

    /// <summary>
    /// Whether to throw an exception when an unrecoverable error occurs while decompressing RLE data in a <see cref="TpsPage"/>.
    /// If <see langword="false"/>, the parser will skip pages that cannot be decompressed.
    /// Default is <see langword="true"/>.
    /// </summary>
    public bool ThrowOnRleDecompressionError { get; init; } = true;

    /// <summary>
    /// Behavior to use when decompressing RLE data and the decompressed size is smaller than the expected size.
    /// Default is <see cref="RleSizeMismatchBehavior.Allow"/>.
    /// </summary>
    public RleSizeMismatchBehavior RleUndersizedDecompressionBehavior { get; init; } = RleSizeMismatchBehavior.Allow;

    /// <summary>
    /// <para>
    /// Behavior to use when decompressing RLE data and the decompressed size is larger than the expected size.
    /// Default is <see cref="RleSizeMismatchBehavior.Throw"/>.
    /// </para>
    /// <para>
    /// <b>WARNING:</b> Using <see cref="RleSizeMismatchBehavior.Allow"/> with malformed RLE data may result
    /// in extremely large memory allocations.
    /// </para>
    /// </summary>
    public RleSizeMismatchBehavior RleOversizedDecompressionBehavior { get; init; } = RleSizeMismatchBehavior.Throw;
}

/// <summary></summary>
public enum RleSizeMismatchBehavior
{
    /// <summary>
    /// Throw an exception when a page decompresses to an amount of memory different than the expected size.
    /// </summary>
    Throw,

    /// <summary>
    /// Decompress the RLE data, and allow the decompression to allocate a different amount of memory than the expected size.
    /// </summary>
    Allow,

    /// <summary>
    /// Skip pages that decompress to an amount of memory different than the expected size.
    /// </summary>
    Skip
}
