﻿using Leak.Common;

namespace Leak.Core.Events
{
    public class ExtensionListSent
    {
        public PeerHash Peer;

        public string[] Extensions;
    }
}