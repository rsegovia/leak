﻿using Leak.Glue;
using System;

namespace Leak.Extensions.Peers.Tests
{
    public class PeersInstance : IDisposable
    {
        private readonly GlueService service;

        public PeersInstance(GlueService service)
        {
            this.service = service;
        }

        public GlueService Service
        {
            get { return service; }
        }

        public void Dispose()
        {
        }
    }
}