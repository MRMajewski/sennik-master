using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Multiplayer;
using UnityEditor;
using UnityEngine;

public class Main : MonoBehaviour {
    [SerializeField] Player playerPrefab;
    [SerializeField] Reflection reflectionPrefab;

 
    Player player;
    Reflection reflection;

    CancellationTokenSource multiplayerCTS = new();
    Client client;
    Host host;
    GamePeer peer;

    float previousMousePosition;


    [SerializeField]
    private float playerSpeedParameter = 1f;

    [SerializeField]
    private float offsetDistance = .75f;


    [SerializeField]
    private GameObject winPanel;

    [SerializeField]
    private bool cannotBeMoved=false;

    [Space]
    [Header("Board Parameteres")]

    [SerializeField] Vector2 boardSize;

    [SerializeField]
    private List<GameObject> portalsList;

    [SerializeField]
    private List<GameObject> possibleObstacles;

    [SerializeField]
    private Vector2 numberOfObstaclesRange= new Vector2(5,10);

    [SerializeField]
    private List<GameObject> currentObstaclesList;

    [SerializeField]
    private Transform currentObstaclesParent;

    [SerializeField]
    private Transform currenPortalsParent;

    [SerializeField]
    private Vector2 portalCountRange = new Vector2(2, 10); 

    [SerializeField]
    private GameObject portalPrefab;

    // Zabezpieczenie, aby x <= y
    private void OnValidate() {
        if (numberOfObstaclesRange.x > numberOfObstaclesRange.y) {
            numberOfObstaclesRange.x = numberOfObstaclesRange.y;
        }

        // Zabezpieczenie, aby x <= y oraz wartoœci by³y parzyste.
        if (portalCountRange.x > portalCountRange.y) {
            portalCountRange.x = portalCountRange.y;
        }
    }

    void Awake() {
        SetupMultiplayer();
        player = InstantiatePlayer();
        reflection = InstantiateReflection();
        winPanel.SetActive(false);
        InitBoardAndObjects();
        InitPortals();
        //  InstantiateReflection();
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

        Reflection InstantiateReflection()
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

    public void InitBoardAndObjects() {

        foreach (var obstacle in currentObstaclesList) {
            Destroy(obstacle);
        }
        currentObstaclesList.Clear();


        int obstacleCount = UnityEngine.Random.Range((int)numberOfObstaclesRange.x, (int)numberOfObstaclesRange.y + 1);

        for (int i = 0; i < obstacleCount; i++) {
            GameObject obstaclePrefab = possibleObstacles[UnityEngine.Random.Range(0, possibleObstacles.Count)];


            Vector3 position;
            bool positionValid;
            int attempts = 0;
            const int maxAttempts = 100; // Limit prób, by unikn¹æ nieskoñczonej pêtli

            do {
                positionValid = true;
                position = new Vector3(
                    UnityEngine.Random.Range(-boardSize.x / 2-1, boardSize.x / 2-1),
                    0f,
                    UnityEngine.Random.Range(-boardSize.y / 2-1, boardSize.y / 2 - 1)
                );

                // Sprawdzenie kolizji z istniej¹cymi przeszkodami
                foreach (var existingObstacle in currentObstaclesList) {
                    if (Vector3.Distance(position, existingObstacle.transform.position) < offsetDistance) {
                        positionValid = false;
                        break;
                    }
                }

                attempts++;
            } while (!positionValid && attempts < maxAttempts);


            if (positionValid) {
                Quaternion rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                GameObject newObstacle = Instantiate(obstaclePrefab, position, rotation, currentObstaclesParent);
                currentObstaclesList.Add(newObstacle);
            } else {
                Debug.LogWarning($"Nie uda³o siê wygenerowaæ pozycji dla przeszkody po {maxAttempts} próbach.");
            }
        }
    }

    public void InitPortals() {

        foreach (var portal in portalsList) {
            Destroy(portal);
        }
        portalsList.Clear();

        int portalCount = UnityEngine.Random.Range((int)portalCountRange.x, (int)portalCountRange.y + 1);


        List<Vector3> usedPositions = new List<Vector3>();

        // Generowanie portali.
        for (int i = 0; i < portalCount; i++) {
            Vector3 position;
            int maxAttempts = 100;
            int attempts = 0;


            do {
                position = new Vector3(
                    UnityEngine.Random.Range(-boardSize.x / 2, boardSize.x / 2),
                    0f,
                    UnityEngine.Random.Range(-boardSize.y / 2, boardSize.y / 2)
                );
                attempts++;
            } while (usedPositions.Exists(p => Vector3.Distance(p, position) < 1f) && attempts < maxAttempts);

            if (attempts >= maxAttempts) {
                Debug.LogWarning("Nie uda³o siê znaleŸæ wystarczaj¹cej liczby miejsc na portale.");
                break;
            }
            Quaternion rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

            var portalObject = Instantiate(portalPrefab, position, rotation, currenPortalsParent);
            portalsList.Add(portalObject);
            usedPositions.Add(position);
           
        }

        PreparePortals();

        void PreparePortals() {

            for (int i = 0; i < portalsList.Count - 1; i++) {

                portalsList[i].GetComponent<Portal>().SetExitPortal(portalsList[i + 1].GetComponent<Portal>());
            }

            portalsList[portalsList.Count-1].GetComponent<Portal>().SetExitPortal(portalsList[0].GetComponent<Portal>());
        }
    }


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
            if (cannotBeMoved) return;
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
        CheckTriggers();
        return;

        void CheckTriggers() {
            var trigger = player.enteredTrigger;
            if (trigger == null)
                return;

            var portal = trigger.GetComponent<Portal>();
            if (portal != null) 
            {
                UsePortal(portal);

                player.enteredTrigger = null;
                return;
            }
               else if (trigger.GetComponent<Reflection>()) {
                Debug.Log("Found Reflection");
                     winPanel.SetActive(true);
                cannotBeMoved
                    = true; 
              //  return;
            }
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

    public void SetBoolCannotMove(bool value) {
        cannotBeMoved = value;
    }
    public void ExitFromGame() {
        Application.Quit();
    }

    #endregion


    float GetMousePosition()
        => Input.mousePosition.x;
}
