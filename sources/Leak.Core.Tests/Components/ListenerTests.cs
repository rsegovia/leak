﻿using FluentAssertions;
using Leak.Common;
using Leak.Completion;
using Leak.Core.Connector;
using Leak.Core.Core;
using Leak.Core.Events;
using Leak.Core.Listener;
using Leak.Core.Network;
using Leak.Core.Tests.Core;
using NUnit.Framework;

namespace Leak.Core.Tests.Components
{
    public class ListenerTests
    {
        private LeakPipeline pipeline;
        private CompletionThread worker;
        private NetworkPool pool;
        private PeerListener listener;
        private PeerListenerHooks hooks;
        private PeerListenerConfiguration configuration;
        private PeerConnector connector;

        [SetUp]
        public void SetUp()
        {
            pipeline = new LeakPipeline();
            worker = new CompletionThread();

            pool = new NetworkPool(pipeline, worker, new NetworkPoolHooks());
            connector = new PeerConnector(pool, new PeerConnectorHooks(), new PeerConnectorConfiguration());

            hooks = new PeerListenerHooks();
            configuration = new PeerListenerConfiguration();
            listener = new PeerListener(pool, hooks, configuration);

            worker.Start();
            pipeline.Start();
            pool.Start();
            connector.Start(pipeline);
        }

        [TearDown]
        public void TearDown()
        {
            worker.Dispose();
            pipeline.Stop();
            listener.Stop();
        }

        [Test]
        public void ShouldTriggerListenerStarted()
        {
            configuration.Port = new PeerListenerPortRandom();
            configuration.Peer = PeerHash.Random();

            var handler = hooks.OnListenerStarted.Trigger(data =>
            {
                data.Port.Should().BeGreaterThan(0);
                data.Peer.Should().Be(configuration.Peer);
            });

            hooks.OnListenerStarted = handler;

            listener.Start();
            handler.Wait().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerOnConnectionArrived()
        {
            FileHash hash = FileHash.Random();
            ListenerStarted started = null;

            configuration.Port = new PeerListenerPortRandom();
            configuration.Peer = PeerHash.Random();

            hooks.OnListenerStarted = data => started = data;

            var handler = hooks.OnConnectionArrived.Trigger(data =>
            {
                data.Remote.Should().NotBeNull();
                data.Remote.Host.Should().Be("127.0.0.1");
                data.Remote.Port.Should().BeGreaterThan(0);
                data.Remote.Port.Should().NotBe(started.Port);
            });

            hooks.OnConnectionArrived = handler;
            listener.Start();

            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", started.Port));
            handler.Wait().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerOnHandshakeCompleted()
        {
            FileHash hash = FileHash.Random();
            ListenerStarted started = null;

            configuration.Port = new PeerListenerPortRandom();
            configuration.Peer = PeerHash.Random();

            hooks.OnListenerStarted = data => started = data;

            var handler = hooks.OnHandshakeCompleted.Trigger(data =>
            {
                data.Connection.Should().NotBeNull();
                data.Handshake.Hash.Should().Be(hash);
            });

            hooks.OnHandshakeCompleted = handler;

            listener.Enable(hash);
            listener.Start();

            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", started.Port));
            handler.Wait().Should().BeTrue();
        }

        [Test]
        public void ShouldTriggerOnHandshakeRejected()
        {
            FileHash hash = FileHash.Random();
            ListenerStarted started = null;

            configuration.Port = new PeerListenerPortRandom();
            configuration.Peer = PeerHash.Random();

            hooks.OnListenerStarted = data => started = data;

            var handler = hooks.OnHandshakeRejected.Trigger(data =>
            {
                data.Connection.Should().NotBeNull();
            });

            hooks.OnHandshakeRejected = handler;
            listener.Start();

            connector.ConnectTo(hash, new PeerAddress("127.0.0.1", started.Port));
            handler.Wait().Should().BeTrue();
        }
    }
}