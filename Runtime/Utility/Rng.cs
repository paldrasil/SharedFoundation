using System;

namespace Shared.Foundation
{
    public sealed class Rng
    {
        private readonly Random _rng;

        //seedFrame = baseSeed ^ frameIndex * 0x9E3779B9
        public Rng(int seed) => _rng = new Random(seed);

        public int NextInt(int minInclusive, int maxExclusive) => _rng.Next(minInclusive, maxExclusive);

        public float NextFloat(float minInclusive, float maxInclusive)
        {
            // NextDouble() ∈ [0,1)
            double t = _rng.NextDouble();
            return (float)(minInclusive + t * (maxInclusive - minInclusive));
        }
    }
}
