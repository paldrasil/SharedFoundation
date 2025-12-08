namespace Shared.Foundation
{
    public static class SeedUtil
    {
        // SplitMix64 → trả về int seed “đã trộn”
        public static int SeedFrom(ulong baseSeed, ulong frameIndex, ulong systemId = 0)
        {
            ulong z = baseSeed
                    + 0x9E3779B97F4A7C15UL * (frameIndex + 1)
                    + 0xD2B74407B1CE6E93UL * (systemId + 1);
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            z ^= z >> 31;
            return unchecked((int)z);
        }
    }
}
