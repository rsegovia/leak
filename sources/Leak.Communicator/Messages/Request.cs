﻿using Leak.Common;

namespace Leak.Communicator.Messages
{
    public class Request
    {
        private readonly BlockIndex block;

        public Request(BlockIndex block)
        {
            this.block = block;
        }

        public BlockIndex Block
        {
            get { return block; }
        }
    }
}