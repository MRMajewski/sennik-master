using System;

namespace Multiplayer {
    public interface GamePeer {
        event Action<byte[]> OnDataReceived;
        void SendData(byte[] data);
    }

    public interface NetworkPeer {
        event Action<byte[]> OnDataToSend;
        void PassReceivedData(byte[] data);
    }

    public class Peer : GamePeer, NetworkPeer {
        public event Action<byte[]> OnDataReceived;
        public event Action<byte[]> OnDataToSend;

        public void SendData(byte[] data) {
            OnDataToSend.Invoke(data);
        }

        public void PassReceivedData(byte[] data) {
            OnDataReceived.Invoke(data);
        }
    }
}
