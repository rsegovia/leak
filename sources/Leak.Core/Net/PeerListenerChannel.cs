﻿namespace Leak.Core.Net
{
    public class PeerListenerChannel : PeerChannel
    {
        private readonly PeerConnection connection;
        private readonly PeerCallback callback;

        public PeerListenerChannel(PeerConnection connection, PeerCallback callback)
        {
            this.connection = connection;
            this.callback = callback;
        }

        public override PeerDescription Description
        {
            get { return null; }
        }

        public override void Send(PeerMessageFactory data)
        {
            connection.Send(data.GetMessage());
        }

        public void Start(PeerMessageLoop loop)
        {
            connection.ReceiveOrCallback(loop.Process);
        }
    }
}