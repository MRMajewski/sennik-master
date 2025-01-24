using System;
using System.Buffers;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Multiplayer {
    public static class Utils {
        public static async UniTask ReadStream(Stream stream, NetworkPeer peer, CancellationToken cancellationToken) {
            if (stream.CanRead)
                await Read();
            else
                await UniTask.Yield();
            return;

            async UniTask Read() {
                var buffer = ArrayPool<byte>.Shared.Rent(1024);

                var byteCount = await stream.ReadAsync(buffer, cancellationToken);
                var output = new byte[byteCount];
                Array.Copy(sourceArray: buffer, destinationArray: output, length: byteCount);
                peer.PassReceivedData(output);

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}