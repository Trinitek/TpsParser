using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics;

namespace TpsParser;

/// <summary>
/// Represents a block within a TPS file. Blocks contain pages.
/// </summary>
public sealed record TpsBlock
{
    /// <summary>
    /// Gets the position in the file where the block starts.
    /// </summary>
    public uint StartOffset { get; init; }

    /// <summary>
    /// Gets the position in the file where the block ends.
    /// </summary>
    public uint EndOffset { get; init; }

    /// <summary>
    /// Gets the length of the block in bytes.
    /// </summary>
    public uint Length { get; init; }

    /// <summary>
    /// Gets the <see cref="TpsRandomAccess"/> reader used to access the data for this block.
    /// </summary>
    public required TpsRandomAccess DataRx { private get; init; }
    
    ///// <summary>
    ///// Gets a memory region that reflects the data for this <see cref="TpsBlock"/>.
    ///// </summary>
    //public required ReadOnlyMemory<byte> BlockData { get; init; }

    private IReadOnlyList<TpsPage>? _pages = null;

    /// <summary>
    /// Creates a new <see cref="TpsBlock"/> from the given descriptor and data reader.
    /// </summary>
    /// <param name="blockDescriptor"></param>
    /// <param name="rx"></param>
    /// <returns></returns>
    public static TpsBlock Parse(TpsBlockDescriptor blockDescriptor, TpsRandomAccess rx)
    {
        ArgumentNullException.ThrowIfNull(blockDescriptor);
        ArgumentNullException.ThrowIfNull(rx);

        // Create a new reader owned by this block; don't share.
        // Block address calculations are relative to the beginning of the file.

        var blockRx = new TpsRandomAccess(
            rx,
            additiveOffset: 0,
            length: (int)blockDescriptor.EndOffset);

        //var blockData = rx.

        return new TpsBlock
        {
            StartOffset = blockDescriptor.StartOffset,
            EndOffset = blockDescriptor.EndOffset,
            Length = blockDescriptor.Length,
            DataRx = blockRx
            //BlockData = 
        };
    }

    /// <summary>
    /// Gets all pages in this block.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<TpsPage> GetPages(bool ignorePageErrors)
    {
        if (_pages is not null)
        {
            return _pages;
        }

        // Some blocks are 0 in length and should be skipped
        if (Length == 0)
        {
            return [];
        }

        var rx = DataRx;

        List<TpsPage> pages = [];

        rx.JumpAbsolute((int)StartOffset);

        while (rx.Position < EndOffset)
        {
            if (IsCompletePage(rx))
            {
                try
                {
                    var page = TpsPage.Parse(rx);

                    pages.Add(page);
                }
                catch (RunLengthEncodingException ex)
                {
                    if (ignorePageErrors)
                    {
                        Debug.WriteLine($"Ignored RLE error: {ex}");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                rx.JumpRelative(0x100);
            }

            NavigateToNextPage(rx);
        }

        _pages = pages.AsReadOnly();

        return _pages;
    }

    private static void NavigateToNextPage(TpsRandomAccess rx)
    {
        if ((rx.Position & 0xFF) != 0x00)
        {
            // Jump to next probable page if we aren't already at a new page.
            rx.JumpAbsolute((int)(rx.Position & 0xFFFF_FF00) + 0x100);
        }

        if (!rx.IsAtEnd)
        {
            int address;

            do
            {
                address = rx.PeekLongLE();

                // Check if there is really a new page here.
                // If so, the offset in the file must match the new value.
                // If not, we continue.
                if (address != rx.Position)
                {
                    rx.JumpRelative(0x100);
                }
            }
            while (address != rx.Position && !rx.IsAtEnd);
        }
    }

    private static bool IsCompletePage(TpsRandomAccess rx)
    {
        var mem = rx.PeekRemainingMemory();
        var span = mem.Span;

        ushort pageSize = BinaryPrimitives.ReadUInt16LittleEndian(span[4..]);

        int offset = 0;
        int position = rx.Position;

        while (offset < pageSize)
        {
            offset += 0x100;
            position += 0x100;

            if (offset < pageSize)
            {
                int address = BinaryPrimitives.ReadInt32LittleEndian(span[offset..]);

                if (address == position)
                {
                    Debug.WriteLine("Incomplete page");
                    return false;
                }
            }
        }

        return true;
    }
}
