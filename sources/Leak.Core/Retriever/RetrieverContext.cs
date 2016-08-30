﻿using Leak.Core.Collector;
using Leak.Core.Metadata;
using Leak.Core.Omnibus;
using Leak.Core.Repository;
using Leak.Core.Retriever.Callbacks;
using System;

namespace Leak.Core.Retriever
{
    public class RetrieverContext
    {
        private readonly RetrieverConfiguration configuration;
        private readonly RepositoryService repository;
        private readonly OmnibusService omnibus;
        private readonly RetrieverQueue queue;
        private readonly RetrieverTimer timer;

        public RetrieverContext(Action<RetrieverConfiguration> configurer)
        {
            configuration = configurer.Configure(with =>
            {
                with.Callback = new RetrieverCallbackNothing();
            });

            repository = new RepositoryService(with =>
            {
                with.Metainfo = configuration.Metainfo;
                with.Destination = configuration.Destination;
                with.Callback = new RetrieverToRepository(this);
            });

            omnibus = new OmnibusService(with =>
            {
                with.Metainfo = configuration.Metainfo;
                with.Bitfield = configuration.Bitfield;
                with.Callback = new RetrieverToOmnibus(this);
            });

            queue = new RetrieverQueue();
            timer = new RetrieverTimer(TimeSpan.FromSeconds(0.25));
        }

        public RetrieverConfiguration Configuration
        {
            get { return configuration; }
        }

        public PeerCollectorView Collector
        {
            get { return configuration.Collector; }
        }

        public RetrieverCallback Callback
        {
            get { return configuration.Callback; }
        }

        public Metainfo Metainfo
        {
            get { return configuration.Metainfo; }
        }

        public RepositoryService Repository
        {
            get { return repository; }
        }

        public RetrieverQueue Queue
        {
            get { return queue; }
        }

        public RetrieverTimer Timer
        {
            get { return timer; }
        }

        public OmnibusService Omnibus
        {
            get { return omnibus; }
        }
    }
}