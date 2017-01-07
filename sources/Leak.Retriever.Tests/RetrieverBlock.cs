﻿using Leak.Common;

namespace Leak.Retriever.Tests
{
    public class RetrieverBlock : DataBlock
    {
        private readonly byte[] data;
        private readonly int offset;

        public RetrieverBlock()
        {
            this.data = Bytes.Random(20000);
        }

        private RetrieverBlock(byte[] data, int offset)
        {
            this.data = data;
            this.offset = offset;
        }

        public int Size
        {
            get { return data.Length - offset; }
        }

        public byte this[int index]
        {
            get { return data[index + offset]; }
        }

        public void Write(DataBlockCallback callback)
        {
            callback.Invoke(data, offset, data.Length - offset);
        }

        public DataBlock Scope(int shift)
        {
            return new RetrieverBlock(data, shift + offset);
        }

        public void Dispose()
        {
        }
    }
}
