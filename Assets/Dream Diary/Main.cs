using System;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Multiplayer;
using UnityEditor;
using UnityEngine;

public class Main : MonoBehaviour {
    [SerializeField] Player playerPrefab;
    [SerializeField] Reflection reflectionPrefab;

    [SerializeField] Vector2 boardSize;

    Player player;

    CancellationTokenSource multiplayerCTS = new();
    Client client;
    Host host;
    GamePeer peer;

    float previousMousePosition;

    void Awake() {
        SetupMultiplayer();
        player = InstantiatePlayer();
        InstantiateReflection();
        return;

        void SetupMultiplayer() {
            host = new(port: 1410);
            var peer = new Peer();
            peer.OnDataReceived += HandleDataReceived;
            host.Run(peer, multiplayerCTS.Token).Forget();
            this.peer = peer;

            void HandleDataReceived(byte[] data) {
                var encoding = Encoding.UTF8;
                Debug.Log($"Received data: {encoding.GetString(data)}");
                peer.SendData(encoding.GetBytes("General Kenobi"));
            }
        }

        // void SetupMultiplayer() {
        //     client = new(ip: "127.0.0.1", port: 1410);
        //     var peer = new Peer();
        //     peer.OnDataReceived += HandleDataReceived;
        //     client.Run(peer, multiplayerCTS.Token).Forget();
        //     this.peer = peer;
        //     PingHost(multiplayerCTS.Token).Forget();

        //     void HandleDataReceived(byte[] data) {
        //         Debug.Log($"Received data: {Encoding.UTF8.GetString(data)}");
        //     }

        //     async UniTask PingHost(CancellationToken cancellationToken) {
        //         while (!cancellationToken.IsCancellationRequested) {
        //             await UniTask.Delay(TimeSpan.FromSeconds(10), cancellationToken: cancellationToken);
        //             peer.SendData(Encoding.UTF8.GetBytes("Hello there"));
        //         }
        //     }
        // }

        Player InstantiatePlayer()
            => Instantiate(playerPrefab, GetRandomPosition(), rotation: Quaternion.identity);

        void InstantiateReflection()
            => Instantiate(reflectionPrefab, GetRandomPosition(), rotation: Quaternion.identity);

        Vector3 GetRandomPosition() {
            return new Vector3(
                GetRandomOffset() * boardSize.x,
                0f,
                GetRandomOffset() * boardSize.y
            );
        }

        float GetRandomOffset()
            => UnityEngine.Random.value - 0.5f;
    }

    void Start() {
        previousMousePosition = GetMousePosition();
    }

    void Update() {
        CheckPortals();
        CheckInput();
        return;

        void CheckInput() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                multiplayerCTS.Cancel();
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
                Application.Quit();
            }

            if (Input.anyKey)
                player.Move(Config.MOVEMENT);

            var mousePosition = GetMousePosition();
            var mouseDelta = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            if (!Mathf.Approximately(mouseDelta, 0f))
                player.Rotate(mouseDelta);
        }

        void CheckPortals() {
            var trigger = player.enteredTrigger;
            if (trigger == null)
                return;

            var portal = trigger.GetComponent<Portal>();
            if (portal != null)
                UsePortal(portal);

            player.enteredTrigger = null;
        }

        void UsePortal(Portal portal) {
            var positionDiff = portal.transform.position - player.transform.position;
            Debug.Log($"{nameof(UsePortal)}: from {portal.name} to {portal.GetExitPortal().name}"); //TODO remove once the issue is fixed
            player.transform.position = portal.GetExitPortal().transform.position - positionDiff * 2f;
        }
    }

    float GetMousePosition()
        => Input.mousePosition.x;
}
