﻿using Leak.Common;
using Leak.Core.Network;

namespace Leak.Core.Messages
{
    public class BitfieldOutgoingMessage : NetworkOutgoingMessage
    {
        private readonly byte[] data;

        public BitfieldOutgoingMessage(Bitfield bitfield)
        {
            this.data = new byte[(bitfield.Length - 1) / 8 + 1];

            for (int i = 0; i < bitfield.Length; i++)
            {
                if (bitfield[i])
                {
                    data[i / 8] += (byte)(1 << (byte)(7 - i % 8));
                }
            }
        }

        public int Length
        {
            get { return data.Length + 5; }
        }

        public byte[] ToBytes()
        {
            byte[] result = { 0x00, 0x00, 0x00, 0x00, 0x05 };

            Bytes.Write(data.Length + 1, result, 0);
            Bytes.Append(ref result, data);

            return result;
        }
    }
}