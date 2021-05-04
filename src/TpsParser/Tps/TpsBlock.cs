using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TpsParser.Tps
{
    /// <summary>
    /// Represents a block of <see cref="TpsPage"/> objects.
    /// </summary>
    public sealed class TpsBlock
    {
        /// <summary>
        /// Gets the pages that belong to this block.
        /// </summary>
        public IReadOnlyList<TpsPage> Pages => _pages;
        private readonly List<TpsPage> _pages;

        private int Start { get; }
        private int End { get; }
        private TpsReader Data { get; }

        public TpsBlock(TpsReader rx, int start, int end, bool ignorePageErrors)
        {
            Data = rx ?? throw new ArgumentNullException(nameof(rx));
            Start = start;
            End = end;
            _pages = new List<TpsPage>();

            Data.PushPosition();
            Data.JumpAbsolute(Start);

            try
            {
                // Some blocks are 0 in length and should be skipped
                while (Data.Position < End)
                {
                    if (IsCompletePage())
                    {
                        try
                        {
                            var page = new TpsPage(Data);
                            _pages.Add(page);
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
                        Data.JumpRelative(0x100);
                    }

                    NavigateToNextPage();
                }
            }
            finally
            {
                Data.PopPosition();
            }
        }

        private void NavigateToNextPage()
        {
            if ((Data.Position & 0xFF) != 0x00)
            {
                // Jump to next probable page if we aren't already at a new page.
                Data.JumpAbsolute((int)(Data.Position & 0xFFFF_FF00) + 0x100);
            }

            if (!Data.IsAtEnd)
            {
                int address;

                do
                {
                    Data.PushPosition();
                    address = Data.ReadLongLE();
                    Data.PopPosition();

                    // Check if there is really a new page here.
                    // If so, the offset in the file must match the new value.
                    // If not, we continue.
                    if (address != Data.Position)
                    {
                        Data.JumpRelative(0x100);
                    }
                }
                while ((address != Data.Position) && !Data.IsAtEnd);
            }
        }

        private bool IsCompletePage()
        {
            int pageSize;

            Data.PushPosition();

            try
            {
                Data.ReadLongLE();
                pageSize = Data.ReadShortLE();
            }
            finally
            {
                Data.PopPosition();
            }

            Data.PushPosition();

            try
            {
                int offset = 0;

                while (offset < pageSize)
                {
                    offset += 0x100;
                    Data.JumpRelative(0x100);

                    if (offset < pageSize)
                    {
                        int address;

                        Data.PushPosition();

                        try
                        {
                            address = Data.ReadLongLE();
                        }
                        finally
                        {
                            Data.PopPosition();
                        }

                        if (address == Data.Position)
                        {
                            Debug.WriteLine("Incomplete page");
                            return false;
                        }
                    }
                }
            }
            finally
            {
                Data.PopPosition();
            }

            return true;
        }

        /// <inheritdoc/>
        public override string ToString() =>
            $"TpsBlock({StringUtils.ToHex8(Start)},{StringUtils.ToHex8(End)},{Pages.Count})";
    }
}
