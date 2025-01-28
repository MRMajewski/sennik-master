using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Multiplayer;
using Unity.Plastic.Newtonsoft.Json.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class Main : MonoBehaviour {
   
    [SerializeField]
    private UIManager uiManager;

    Player player;
    Reflection reflection;

    CancellationTokenSource multiplayerCTS = new();
    Client client;
    Host host;
    GamePeer peer;

    float previousMousePosition;

    [Header("Player movement references")]
    [SerializeField]
    private float playerSpeedParameter = 1f;

    private bool cannotBeMoved = false;
    private bool isGameOn = false;

    [Header("Prefabs referenes")]
    [SerializeField] 
    private Player playerPrefab;
    [SerializeField] 
    private Reflection reflectionPrefab;
    [SerializeField]
    private GameObject portalPrefab;
    [SerializeField]
    private List<GameObject> possibleObstacles;

    [Space]
    [Header("Board Parameteres")]

    [SerializeField] Vector2 boardSize;

    [SerializeField] float objectsInitializationOffset = 2f;

    [SerializeField]
    private List<GameObject> portalsList;

    [SerializeField]
    private Vector2 numberOfObstaclesRange = new Vector2(5, 10);

    [SerializeField]
    private List<GameObject> currentObstaclesList;

    [SerializeField]
    private Transform currentObstaclesParent;

    [SerializeField]
    private Transform currenPortalsParent;

    [SerializeField]
    private Vector2 portalCountRange = new Vector2(2, 10);
    [SerializeField]
    private List<GameObject> spawnedObjects = new List<GameObject>();



    [Header("Ad method references")]
    private Vector3 previousPlayerPosition;
    private float distanceTraveled = 0f;
    private bool wasAdShown = false;

    [Header("Settings references")]
    [SerializeField]
    private float mouseSensitivity = 1f;
    private float masterVolume = 1f;

    private void OnValidate() {
        if (numberOfObstaclesRange.x > numberOfObstaclesRange.y) {
            numberOfObstaclesRange.x = numberOfObstaclesRange.y;
        }

        if (portalCountRange.x > portalCountRange.y) {
            portalCountRange.x = portalCountRange.y;
        }
    }
    void Awake() {
        isGameOn = false;
        SetVolume(PlayerPrefs.GetFloat("Volume", .5f));
        SetMouseSensitivity(PlayerPrefs.GetFloat("Sensitivity", 50.0f));   
        uiManager.InitUI();
    }
    public void StartSoloGame() {

        player = InstantiatePlayer();
        previousPlayerPosition = player.transform.position;
        reflection = InstantiateReflection();
    
        uiManager.CloseMainMenu();
        spawnedObjects.Clear();
        spawnedObjects.TrimExcess();
        InitPortals();
        InitBoardAndObjects();
      
        isGameOn = true;
        cannotBeMoved = false;

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
    public void StartMultiplayer() {

        SetupMultiplayer();
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
                    UnityEngine.Random.Range(-Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset), Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset)),
                    0f,
                    UnityEngine.Random.Range(-Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset), Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset))
                );

                // Sprawdzenie kolizji z istniej¹cymi przeszkodami
                foreach (var existingObstacle in spawnedObjects) {
                    if (Vector3.Distance(position, existingObstacle.transform.position) < objectsInitializationOffset * 2f) {
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
                spawnedObjects.Add(newObstacle);
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

        for (int i = 0; i < portalCount; i++) {
            Vector3 position;
            int maxAttempts = 100;
            int attempts = 0;

            do {
                position = new Vector3(
                    UnityEngine.Random.Range(-Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset), Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset)),
                    0f,
                    UnityEngine.Random.Range(-Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset), Mathf.Abs(boardSize.y / 2 - objectsInitializationOffset))
                );
                attempts++;
            } while (usedPositions.Exists(p => Vector3.Distance(p, position) < objectsInitializationOffset) && attempts < maxAttempts);

            if (attempts >= maxAttempts) {
                Debug.LogWarning("Nie uda³o siê znaleŸæ wystarczaj¹cej liczby miejsc na portale.");
                break;
            }
            Quaternion rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);

            var portalObject = Instantiate(portalPrefab, position, rotation, currenPortalsParent);
            portalsList.Add(portalObject);
            usedPositions.Add(position);
            spawnedObjects.Add(portalObject);

        }

        PreparePortals();

        void PreparePortals() {

            for (int i = 0; i < portalsList.Count - 1; i++) {

                portalsList[i].GetComponent<Portal>().SetExitPortal(portalsList[i + 1].GetComponent<Portal>());
            }
            portalsList[portalsList.Count - 1].GetComponent<Portal>().SetExitPortal(portalsList[0].GetComponent<Portal>());
        }
    }

    void Start() {
        previousMousePosition = GetMousePosition();
    }

    #region Update methods
    void Update() {
        if (!isGameOn) return;
        CheckInput();
        if (!wasAdShown) {
            TrackDistance();
        }

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
            if (Input.anyKey) {
                player.Move(Config.MOVEMENT * playerSpeedParameter);
            }
            else {
                player.Stop();
            }


            var mousePosition = GetMousePosition();
            var mouseDelta = (mousePosition - previousMousePosition) * mouseSensitivity * Time.deltaTime;
            previousMousePosition = mousePosition;
            previousMousePosition = mousePosition;

            if (!Mathf.Approximately(mouseDelta, 0f))
                player.Rotate(mouseDelta);
        }

        void TrackDistance() {
            float distanceThisFrame = Vector3.Distance(previousPlayerPosition, player.transform.position);
            distanceTraveled += distanceThisFrame;
            previousPlayerPosition = player.transform.position;

            if (distanceTraveled >= 10f) {
                uiManager.PlayAdSequence();
                wasAdShown = true;
            }
        }
    }

    private void LateUpdate() {
        if (!isGameOn) return;
        CheckTriggers();
        return;

        void CheckTriggers() {
            var trigger = player.enteredTrigger;
            if (trigger == null)
                return;

            var portal = trigger.GetComponent<Portal>();
            if (portal != null) {
                UsePortal(portal);
                player.enteredTrigger = null;
                return;
            } else if (trigger.GetComponent<Reflection>()) {
             
                SetPlayerWin();
            }
        }
        void UsePortal(Portal portal) {
            Portal exitPortal = portal.GetExitPortal();

            player.transform.position = exitPortal.GetExitPortalPosition().position;
            player.transform.rotation = Quaternion.LookRotation(exitPortal.transform.right, exitPortal.transform.up);

        }
    }

    #endregion

    private void SetPlayerWin() {
        player.Stop();
        uiManager.OpenWinPanel();
        isGameOn = false;
        cannotBeMoved = true;
    }

    public void RestartGame() {
        DestroyObjectsOnStart();
        StartSoloGame();
    }

    private void DestroyObjectsOnStart() {
        Destroy(player.gameObject);
        Destroy(reflection.gameObject);
    }
    #region UI methods

    public void OpenSettingsFromGame() {
        uiManager.ToggleSettingsPanelFromGame(true);
        cannotBeMoved = true;
        isGameOn = false;
    }
    public void CloseSettingsFromGame() {
        uiManager.ToggleSettingsPanelFromGame(false);
        cannotBeMoved = false;
        isGameOn = true;
    }


    public void CloseSettingsPanel() {
        if (player)
            CloseSettingsFromGame();
        else {
            uiManager.ToggleSettingsPanelFromGame(false);
        }
    }

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
    public void SetVolume(float volume) {
        PlayerPrefs.SetFloat("Volume", volume);
        masterVolume = Mathf.Clamp01(volume); 
        AudioListener.volume = masterVolume; 
    }
    public void SetMouseSensitivity(float sensitivity) {
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        mouseSensitivity = Mathf.Clamp(sensitivity,0,100f);
    }

    float GetMousePosition()
        => Input.mousePosition.x;
}
