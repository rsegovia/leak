﻿namespace Leak.Core.Metadata
{
    public class MetainfoHash
    {
        private readonly byte[] value;

        public MetainfoHash(byte[] value)
        {
            this.value = value;
        }

        public byte[] ToBytes()
        {
            return value;
        }
    }
}