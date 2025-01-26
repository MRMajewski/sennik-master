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


    [SerializeField]
    private float playerSpeedParameter = 1f;

    [SerializeField]
    private float offsetDistance = .75f;



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

    //public void InitGame() {

    //    if(player) {

    //    }
    //    ele
    //}




    void Start() {
        previousMousePosition = GetMousePosition();
    }

    void Update() {
     
        CheckInput();
        return;

        void CheckInput() {
            if (Input.GetKeyDown(KeyCode.Escape)) {
                multiplayerCTS.Cancel();
#if UNITY_EDITOR
                EditorApplication.ExitPlaymode();
#endif
                ExitFromGame();
            }

            if (Input.anyKey)
                player.Move(Config.MOVEMENT* playerSpeedParameter);

            var mousePosition = GetMousePosition();
            var mouseDelta = mousePosition - previousMousePosition;
            previousMousePosition = mousePosition;

            if (!Mathf.Approximately(mouseDelta, 0f))
                player.Rotate(mouseDelta);
        }
  
    }

    private void LateUpdate() {
        CheckPortals();
        return;

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

            Portal exitPortal = portal.GetExitPortal();

            Vector3 exitPosition = exitPortal.transform.position;
            Vector3 exitNormal = exitPortal.transform.right;

            Vector3 adjustedPosition = exitPosition + exitNormal * offsetDistance;

            Vector3 positionDiff = portal.transform.position - player.transform.position;

            player.transform.position = adjustedPosition;
            player.transform.rotation = Quaternion.LookRotation(exitPortal.transform.right, Vector3.up);

        }
    }

    #region UI methods


    public void SetPlayerSpeed(float value) {
        playerSpeedParameter = value;
    }

    public void ExitFromGame() {
        Application.Quit();
    }

    #endregion


    float GetMousePosition()
        => Input.mousePosition.x;
}
