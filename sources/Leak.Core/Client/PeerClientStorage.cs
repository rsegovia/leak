﻿using Leak.Core.Collector;
using Leak.Core.Common;
using Leak.Core.Messages;
using Leak.Core.Metadata;
using Leak.Core.Repository;
using Leak.Core.Retriever;
using System.Collections.Generic;
using System.IO;

namespace Leak.Core.Client
{
    public class PeerClientStorage
    {
        private readonly PeerClientConfiguration configuration;
        private readonly PeerClientStorageEntryCollection collection;
        private readonly PeerClientCallback callback;

        public PeerClientStorage(PeerClientConfiguration configuration)
        {
            this.configuration = configuration;
            this.callback = configuration.Callback;

            this.collection = new PeerClientStorageEntryCollection();
        }

        public void Register(FileHash hash, PeerCollectorView collector)
        {
            string root = configuration.Destination;
            string path = Path.Combine(root, hash.ToString());

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            ResourceRepository repository = new ResourceRepositoryToHash(hash, configuration.Destination);

            ResourceRetriever retriever = new ResourceRetrieverToQuery(with =>
            {
                with.Collector = collector;
                with.Repository = repository;
                with.Callback = new PeerClientToRetriever(hash, configuration, this);
            });

            collection.Add(hash, new PeerClientStorageEntry
            {
                Hash = hash,
                Retriever = retriever,
                Repository = repository,
                Peers = new HashSet<PeerHash>()
            });
        }

        public void Register(Metainfo metainfo, PeerCollectorView collector)
        {
            string root = configuration.Destination;
            string hash = metainfo.Hash.ToString();
            string path = Path.Combine(root, hash);

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            ResourceRepository repository = new ResourceRepositoryToMetainfo(metainfo, path);

            ResourceRetriever retriever = new ResourceRetrieverToGet(with =>
            {
                with.Repository = repository;
                with.Collector = collector;
                with.Callback = new PeerClientToRetriever(metainfo.Hash, configuration, this);
            });

            collection.Add(metainfo.Hash, new PeerClientStorageEntry
            {
                Hash = metainfo.Hash,
                Metainfo = metainfo,
                Repository = repository,
                Retriever = retriever,
                Peers = new HashSet<PeerHash>()
            });
        }

        public void WithBitfield(FileHash hash, Bitfield bitfield)
        {
            PeerClientStorageEntry entry = collection.ByHash(hash);

            entry.Retriever = entry.Retriever.WithBitfield(bitfield);
        }

        public void WithMetainfo(FileHash hash)
        {
            PeerClientStorageEntry entry = collection.ByHash(hash);
            Metainfo metainfo;

            entry.Repository = entry.Repository.WithMetainfo(out metainfo);
            Bitfield bitfield = entry.Repository.Initialize();
            callback.OnInitialized(hash, new PeerClientMetainfo(bitfield));

            entry.Retriever = entry.Retriever.WithRepository(entry.Repository);
            entry.Retriever = entry.Retriever.WithBitfield(bitfield);
            entry.Metainfo = metainfo;
        }

        public bool Contains(PeerAddress remote)
        {
            return collection.Contains(remote);
        }

        public bool AddEndpoint(PeerEndpoint endpoint)
        {
            return collection.AddEndpoint(endpoint);
        }

        public void RemovePeer(PeerHash peer)
        {
            collection.ByPeer(peer).Peers.Remove(peer);
        }

        public bool HasMetainfo(PeerHash peer)
        {
            return collection.ByPeer(peer).Metainfo != null;
        }

        public ResourceRepository GetRepository(FileHash hash)
        {
            return collection.ByHash(hash).Repository;
        }

        public ResourceRetriever GetRetriever(FileHash hash)
        {
            return collection.ByHash(hash).Retriever;
        }

        public ResourceRetriever GetRetriever(PeerHash peer)
        {
            return collection.ByPeer(peer).Retriever;
        }

        public PeerClientCallback GetCallback(PeerHash peer)
        {
            return configuration.Callback;
        }

        public FileHash GetHash()
        {
            return collection.Hash();
        }

        public FileHash GetHash(PeerHash peer)
        {
            return collection.ByPeer(peer).Hash;
        }

        public Metainfo GetMetainfo(PeerHash peer)
        {
            return collection.ByPeer(peer).Metainfo;
        }
    }
}