﻿using Leak.Common;

namespace Leak.Core.Events
{
    public class ExtensionDataSent
    {
        public PeerHash Peer;

        public string Extension;

        public int Size;
    }
}