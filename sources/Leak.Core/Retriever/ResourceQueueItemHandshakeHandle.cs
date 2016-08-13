﻿using Leak.Core.Common;

namespace Leak.Core.Retriever
{
    public class ResourceQueueItemHandshakeHandle : ResourceQueueItem
    {
        private readonly PeerHash peer;

        public ResourceQueueItemHandshakeHandle(PeerHash peer)
        {
            this.peer = peer;
        }

        public void Handle(ResourceQueueContext context)
        {
            if (context.Collector.SupportExtensions(peer))
            {
                if (context.Storage.IsExtended(peer) == false)
                {
                    context.Storage.Extend(peer);
                }
            }
        }
    }
}