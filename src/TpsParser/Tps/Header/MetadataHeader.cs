using TpsParser.Binary;

namespace TpsParser.Tps.Header
{
    public sealed class MetadataHeader : Header
    {
        public int AboutType { get; }

        public bool IsAboutData => AboutType == 0xF3;

        public bool IsAboutKeyOrIndex => AboutType < 0xF3;

        public MetadataHeader(TpsRandomAccess rx)
            : base(rx)
        {
            AssertIsType(0xF6);

            AboutType = rx.Byte();
        }

        public override string ToString() =>
            $"IndexHeader({(IsAboutData ? "Data" : IsAboutKeyOrIndex ? $"Index({AboutType})" : "??")})";
    }
}
