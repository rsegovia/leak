﻿using System;
using System.Collections.Generic;
using Leak.Common;
using Leak.Data.Get;
using Leak.Data.Map;
using Leak.Data.Store;
using Leak.Extensions.Metadata;
using Leak.Glue;
using Leak.Memory;
using Leak.Meta.Get;
using Leak.Meta.Store;
using Leak.Networking;

namespace Leak.Client
{
    public static class ClientExtensions
    {
        public static MetagetGlue AsMetaGet(this GlueService service)
        {
            return new MetaGetToGlueForwarder(service);
        }

        public static MetagetMetafile AsMetaGet(this MetafileService service)
        {
            return new MetaGetToMetaStoreForwarder(service);
        }

        public static DataGetToGlue AsDataGet(this GlueService service)
        {
            return new DataGetToGlueForwarder(service);
        }

        public static DataGetToDataStore AsDataGet(this RepositoryService service)
        {
            return new DataGetToDataStoreForwarder(service);
        }

        public static DataGetToDataMap AsDataGet(this OmnibusService service)
        {
            return new DataGetToDataMapForwarder(service);
        }

        public static NetworkPoolMemory AsNetwork(this MemoryService service)
        {
            return new MemoryToNetwork(service);
        }

        public static RepositoryMemory AsDataStore(this MemoryService service)
        {
            return new MemoryToDataStore(service);
        }

        private class MetaGetToGlueForwarder : MetagetGlue
        {
            private readonly GlueService service;

            public MetaGetToGlueForwarder(GlueService service)
            {
                this.service = service;
            }

            public IEnumerable<PeerHash> Peers
            {
                get
                {
                    List<PeerHash> peers = new List<PeerHash>();

                    service.ForEachPeer(peer =>
                    {
                        if (service.IsSupported(peer, MetadataPlugin.Name))
                        {
                            peers.Add(peer);
                        }
                    });

                    return peers;
                }
            }

            public void SendMetadataRequest(PeerHash peer, int piece)
            {
                service.SendMetadataRequest(peer, piece);
            }
        }

        private class MetaGetToMetaStoreForwarder : MetagetMetafile
        {
            private readonly MetafileService service;

            public MetaGetToMetaStoreForwarder(MetafileService service)
            {
                this.service = service;
            }

            public bool IsCompleted()
            {
                return service.IsCompleted();
            }

            public void Write(int piece, byte[] data)
            {
                service.Write(piece, data);
            }

            public void Verify()
            {
                service.Verify();
            }
        }

        private class DataGetToGlueForwarder : DataGetToGlue
        {
            private readonly GlueService service;

            public DataGetToGlueForwarder(GlueService service)
            {
                this.service = service;
            }

            public void SendInterested(PeerHash peer)
            {
                service.SendInterested(peer);
            }

            public void SendRequest(PeerHash peer, BlockIndex block)
            {
                service.SendRequest(peer, block);
            }
        }

        private class DataGetToDataStoreForwarder : DataGetToDataStore
        {
            private readonly RepositoryService service;

            public DataGetToDataStoreForwarder(RepositoryService service)
            {
                this.service = service;
            }

            public void Verify(PieceInfo piece)
            {
                service.Verify(piece);
            }

            public void Write(BlockIndex block, DataBlock data)
            {
                service.Write(block, data);
            }
        }

        private class DataGetToDataMapForwarder : DataGetToDataMap
        {
            private readonly OmnibusService service;

            public DataGetToDataMapForwarder(OmnibusService service)
            {
                this.service = service;
            }

            public bool IsComplete(PieceInfo piece)
            {
                return service.IsComplete(piece.Index);
            }

            public void Complete(BlockIndex block)
            {
                service.Complete(block);
            }

            public void Complete(PieceInfo piece)
            {
                service.Complete(piece);
            }

            public void Invalidate(PieceInfo piece)
            {
                service.Invalidate(piece.Index);
            }

            public void Schedule(string strategy, PeerHash peer, int count)
            {
                switch (strategy)
                {
                    case "rarest-first":
                        service.Schedule(OmnibusStrategy.RarestFirst, peer, count);
                        return;

                    case "sequential":
                        service.Schedule(OmnibusStrategy.Sequential, peer, count);
                        return;
                }
            }

            public void Query(Action<PeerHash, Bitfield, PeerState> callback)
            {
                service.Query(callback);
            }

            public IEnumerable<PeerHash> Find(int ranking, int count)
            {
                return service.Find(ranking, count);
            }
        }
    }
}