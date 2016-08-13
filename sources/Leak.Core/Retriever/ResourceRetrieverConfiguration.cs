﻿using Leak.Core.Collector;
using Leak.Core.Repository;

namespace Leak.Core.Retriever
{
    public class ResourceRetrieverConfiguration
    {
        public ResourceRepository Repository { get; set; }

        public PeerCollectorView Collector { get; set; }

        public ResourceRetrieverCallback Callback { get; set; }
    }
}