using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Multiplayer {
    public class Host {
        const int BUFFER_SIZE = 1024;
        readonly int port;

        List<NetworkStream> activeStreams = new();

        public Host(int port) {
            this.port = port;
        }

        public async UniTask Run(NetworkPeer peer, CancellationToken cancellationToken) {
            var endpoint = new IPEndPoint(IPAddress.Any, port);
            var listener = new TcpListener(endpoint);
            peer.OnDataToSend += SendData;
            try {
                listener.Start();
                while (!cancellationToken.IsCancellationRequested) {
                    if (listener.Pending())
                        HandleClient().Forget();
                    else
                        await UniTask.Yield();
                }
            } finally {
                listener.Stop();
                peer.OnDataToSend -= SendData;
            }
            return;

            void SendData(byte[] data) {
                foreach (var stream in activeStreams)
                    stream.WriteAsync(data);
            }

            async UniTask HandleClient() {
                var buffer = new byte[BUFFER_SIZE];
                using var client = await listener.AcceptTcpClientAsync();
                using var stream = client.GetStream();
                activeStreams.Add(stream);
                while (client.Connected && !cancellationToken.IsCancellationRequested)
                    await Utils.ReadStream(stream, peer, cancellationToken);
                activeStreams.Remove(stream);
            }
        }
    }
}
