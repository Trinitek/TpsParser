using System;
using TpsParser.Binary;

namespace TpsParser.Tps.Type
{
    /// <summary>
    /// Represents a date.
    /// </summary>
    public sealed class TpsDate : TpsObject<DateTime?>
    {
        public override string TypeName => "DATE";

        public override int TypeCode => 0x04;

        public TpsDate(RandomAccess rx)
        {
            if (rx == null)
            {
                throw new ArgumentNullException(nameof(rx));
            }

            // Date, mask encoded

            long date = rx.UnsignedLongLE();

            if (date != 0)
            {
                long years = (date & 0xFFFF0000) >> 16;
                long months = (date & 0x0000FF00) >> 8;
                long days = date & 0x000000FF;
                Value = new DateTime((int)years, (int)months, (int)days);
            }
            else
            {
                Value = null;
            }
        }
    }
}
