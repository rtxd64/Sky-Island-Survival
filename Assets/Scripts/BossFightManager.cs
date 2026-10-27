
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossFightManager : MonoBehaviour
{
    public static BossFightManager Instance { get; private set; }

    [Header("Red Orb Unlock")]
    public GameObject redOrbPrefab;
    public Transform[] redOrbSpawnPoints = new Transform[4];
    public int orbsNeededToSpawnBoss = 1;
    public int collectedRedOrbs = 0;

    private bool bossUnlocked;
    private bool bossSpawned;
    private bool spawningBoss;

    [Header("Boss Arena")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public Transform player;
    public Collider arenaTrigger;
    public GameObject[] barrierObjects;

    [Header("Boss Ground Placement")]
    public LayerMask groundLayers = ~0;
    public float groundRayHeight = 30f;
    public float groundRayDistance = 100f;
    public float bossGroundOffset = 0f;

    [Header("Random Zombie Waves")]
    public GameObject[] zombiePrefabs;
    public Transform[] zombieSpawnPoints;
    public float zombieSpawnInterval = 12f;
    public int maxExtraZombiesAlive = 5;

    [Header("Victory")]
    public string mainMenuSceneName = "MainMenu";
    public int victoryMoneyReward = 1000;

    private readonly List<GameObject> liveZombies =
        new List<GameObject>();

    private Canvas canvas;
    private Text centerMessage;
    private GameObject victoryPanel;

    private bool arenaLocked;
    private bool victory;
    private bool rewardGiven;
    private Coroutine messageRoutine;

    public bool IsBossFightActive =>
        bossSpawned && arenaLocked && !victory;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("พบ BossFightManager ซ้ำ จะลบตัวที่ซ้ำ");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        FindPlayer();
        CreateUI();
        SetBarrier(false);

        if (arenaTrigger != null)
            arenaTrigger.enabled = false;

        collectedRedOrbs = 0;
        bossUnlocked = false;
        bossSpawned = false;
        spawningBoss = false;
        victory = false;
        rewardGiven = false;
        arenaLocked = false;

        BossHealth existingBoss = FindExistingBoss();

        if (existingBoss != null)
        {
            bossSpawned = true;
            bossUnlocked = true;

            existingBoss.manager = this;

            if (player != null)
                existingBoss.player = player;

            Debug.LogWarning(
                "พบ Boss ใน Scene แล้ว: " + existingBoss.name);

            if (arenaTrigger != null)
                arenaTrigger.enabled = true;

            StartCoroutine(ZombieWaveRoutine());
            return;
        }

        SpawnRedOrbs();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning(
                "ไม่พบ Player Tag กรุณาตรวจสอบ Tag ของผู้เล่น");
    }

    private BossHealth FindExistingBoss()
    {
        BossHealth[] bosses =
            FindObjectsByType<BossHealth>(FindObjectsSortMode.None);

        foreach (BossHealth boss in bosses)
        {
            if (boss != null &&
                boss.gameObject.scene.IsValid() &&
                boss.gameObject.scene == gameObject.scene)
            {
                return boss;
            }
        }

        return null;
    }

    private void SpawnRedOrbs()
    {
        if (redOrbPrefab == null)
        {
            Debug.LogError("ยังไม่ได้กำหนด Red Orb Prefab");
            return;
        }

        if (redOrbSpawnPoints == null ||
            redOrbSpawnPoints.Length == 0)
        {
            Debug.LogError("ยังไม่ได้กำหนด Red Orb Spawn Points");
            return;
        }

        int spawnedCount = 0;
        int count = Mathf.Min(4, redOrbSpawnPoints.Length);

        for (int i = 0; i < count; i++)
        {
            Transform point = redOrbSpawnPoints[i];

            if (point == null)
            {
                Debug.LogWarning(
                    "Red Orb Spawn Point ช่อง " + i + " ยังว่าง");
                continue;
            }

            GameObject orb = Instantiate(
                redOrbPrefab, point.position, point.rotation);

            if (orb.GetComponentInChildren<Collider>() == null)
                Debug.LogError("Orb ไม่มี Collider: " + orb.name);

            if (orb.GetComponent<BossOrb>() == null)
                orb.AddComponent<BossOrb>();

            spawnedCount++;
        }

        Debug.Log("สร้าง Red Orb สำเร็จ " + spawnedCount + " ลูก");
    }

    public void CollectRedOrb()
    {
        if (bossUnlocked || bossSpawned || spawningBoss || victory)
            return;

        collectedRedOrbs++;

        ShowMessage(
            "RED ORB COLLECTED " + collectedRedOrbs, 2f);

        Debug.Log(
            "Red Orb collected: " + collectedRedOrbs +
            " / " + Mathf.Max(1, orbsNeededToSpawnBoss));

        if (collectedRedOrbs >= Mathf.Max(1, orbsNeededToSpawnBoss))
        {
            bossUnlocked = true;
            spawningBoss = true;

            ShowMessage("BOSS HAVE SPAWN", 3f);
            StartCoroutine(SpawnBossRoutine());
        }
    }

    private IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (bossSpawned || victory)
        {
            spawningBoss = false;
            yield break;
        }

        BossHealth existingBoss = FindExistingBoss();

        if (existingBoss != null)
        {
            bossSpawned = true;
            spawningBoss = false;
            existingBoss.manager = this;

            FindPlayer();

            if (player != null)
                existingBoss.player = player;

            if (arenaTrigger != null)
                arenaTrigger.enabled = true;

            StartCoroutine(ZombieWaveRoutine());
            yield break;
        }

        if (bossPrefab == null || bossSpawnPoint == null)
        {
            Debug.LogError(
                "กรุณากำหนด Boss Prefab และ Boss Spawn Point ใน Inspector");

            spawningBoss = false;
            bossUnlocked = false;
            yield break;
        }

        Vector3 spawnPosition = bossSpawnPoint.position;

        Vector3 rayOrigin =
            bossSpawnPoint.position + Vector3.up * groundRayHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundRayDistance + groundRayHeight,
            groundLayers,
            QueryTriggerInteraction.Ignore))
        {
            spawnPosition =
                hit.point + Vector3.up * bossGroundOffset;
        }
        else
        {
            Debug.LogWarning(
                "Raycast หาพื้นไม่พบ ใช้ Boss Spawn Point โดยตรง");
        }

        GameObject boss = Instantiate(
            bossPrefab, spawnPosition, bossSpawnPoint.rotation);

        BossHealth health =
            boss.GetComponentInChildren<BossHealth>(true);

        if (health == null)
        {
            Debug.LogError("Boss Prefab ไม่มี BossHealth");

            Destroy(boss);
            spawningBoss = false;
            bossUnlocked = false;
            yield break;
        }

        health.manager = this;

        FindPlayer();
        health.player = player;

        bossSpawned = true;
        spawningBoss = false;

        if (arenaTrigger != null)
            arenaTrigger.enabled = true;

        Debug.Log("Boss spawned successfully: " + boss.name);

        StartCoroutine(ZombieWaveRoutine());
    }

    public void PlayerEnteredArena()
    {
        if (!bossSpawned || !bossUnlocked || victory || arenaLocked)
            return;

        arenaLocked = true;
        SetBarrier(true);

        ShowMessage("DEFEAT THE BOSS TO ESCAPE!", 3f);
    }

    public void BossDefeated()
    {
        if (victory)
            return;

        victory = true;
        arenaLocked = false;

        SetBarrier(false);
        StopAllCoroutines();

        if (centerMessage != null)
            centerMessage.text = "";

        messageRoutine = null;

        // ลบ Orb โดยไม่ต้องพึ่ง Tag RedOrb
        BossOrb[] remainingOrbs =
            FindObjectsByType<BossOrb>(FindObjectsSortMode.None);

        foreach (BossOrb orb in remainingOrbs)
        {
            if (orb != null)
                Destroy(orb.gameObject);
        }

        if (!rewardGiven)
        {
            rewardGiven = true;

            if (MoneyManager.Instance != null)
            {
                MoneyManager.Instance.AddMoney(victoryMoneyReward);
            }
            else
            {
                Debug.LogWarning("MoneyManager.Instance not found");
            }
        }

        ShowVictory();
    }

    private void SetBarrier(bool active)
    {
        if (barrierObjects == null)
            return;

        foreach (GameObject barrier in barrierObjects)
        {
            if (barrier != null)
                barrier.SetActive(active);
        }
    }

    private IEnumerator ZombieWaveRoutine()
    {
        while (!victory)
        {
            yield return new WaitForSeconds(
                Mathf.Max(2f, zombieSpawnInterval));

            liveZombies.RemoveAll(z => z == null);

            if (liveZombies.Count >= maxExtraZombiesAlive ||
                zombiePrefabs == null ||
                zombieSpawnPoints == null)
                continue;

            List<GameObject> prefabs = new List<GameObject>();

            foreach (GameObject prefab in zombiePrefabs)
            {
                if (prefab != null)
                    prefabs.Add(prefab);
            }

            List<Transform> points = new List<Transform>();

            foreach (Transform point in zombieSpawnPoints)
            {
                if (point != null)
                    points.Add(point);
            }

            if (prefabs.Count == 0 || points.Count == 0)
                continue;

            GameObject chosenPrefab =
                prefabs[Random.Range(0, prefabs.Count)];

            Transform chosenPoint =
                points[Random.Range(0, points.Count)];

            GameObject zombie = Instantiate(
                chosenPrefab,
                chosenPoint.position,
                chosenPoint.rotation);

            liveZombies.Add(zombie);
        }
    }

    private void CreateUI()
    {
        if (canvas != null)
            return;

        EventSystem eventSystem =
            FindFirstObjectByType<EventSystem>();

        if (eventSystem == null)
        {
            GameObject eventObj = new GameObject("EventSystem");
            eventSystem = eventObj.AddComponent<EventSystem>();
        }

        // รองรับ Input Manager แบบเก่า
        if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
        }

        GameObject root = new GameObject("BossFightCanvas");

        canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        root.AddComponent<GraphicRaycaster>();

        GameObject msg = new GameObject("BossMessage");
        msg.transform.SetParent(root.transform, false);

        RectTransform rect = msg.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.82f);
        rect.sizeDelta = new Vector2(1100, 100);

        centerMessage = msg.AddComponent<Text>();
        centerMessage.font =
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        centerMessage.fontSize = 44;
        centerMessage.fontStyle = FontStyle.Bold;
        centerMessage.alignment = TextAnchor.MiddleCenter;
        centerMessage.color = Color.red;
        centerMessage.text = "";
        centerMessage.raycastTarget = false;
    }

    public void ShowMessage(string message, float seconds)
    {
        if (centerMessage == null)
            return;

        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(MessageRoutine(message, seconds));
    }

    private IEnumerator MessageRoutine(string message, float seconds)
    {
        centerMessage.text = message;

        yield return new WaitForSeconds(seconds);

        if (centerMessage != null)
            centerMessage.text = "";

        messageRoutine = null;
    }

    private void ShowVictory()
    {
        if (canvas == null)
            CreateUI();

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }

        victoryPanel = new GameObject("BossVictoryPanel");
        victoryPanel.transform.SetParent(canvas.transform, false);

        RectTransform rect = victoryPanel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image bg = victoryPanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.88f);
        bg.raycastTarget = true;

        CreateText(
            victoryPanel.transform, "YOU WIN", 70,
            new Vector2(0, 150), new Vector2(800, 120), Color.yellow);

        CreateText(
            victoryPanel.transform, "BOSS DEFEATED!", 32,
            new Vector2(0, 70), new Vector2(800, 60), Color.white);

        CreateButton(
            victoryPanel.transform, "CONTINUE",
            new Vector2(0, -10), ContinueAfterVictory);

        CreateButton(
            victoryPanel.transform, "EXIT TO MAIN MENU",
            new Vector2(0, -110), ExitAfterVictory);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("Victory UI shown. Time paused.");
    }

    private void CreateText(
        Transform parent,
        string value,
        int size,
        Vector2 pos,
        Vector2 dims,
        Color color)
    {
        GameObject obj = new GameObject(value);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = dims;
        rect.anchoredPosition = pos;

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;

        // ป้องกันข้อความบังการคลิก Button
        text.raycastTarget = false;
    }

    private void CreateButton(
        Transform parent,
        string label,
        Vector2 pos,
        UnityEngine.Events.UnityAction action)
    {
        GameObject obj = new GameObject(label);
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(520, 70);
        rect.anchoredPosition = pos;

        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.65f, 0.05f, 0.05f);
        image.raycastTarget = true;

        Button button = obj.AddComponent<Button>();
        button.targetGraphic = image;
        button.interactable = true;
        button.onClick.AddListener(action);

        CreateText(
            obj.transform, label, 26,
            Vector2.zero, new Vector2(500, 65), Color.white);

        Debug.Log("สร้างปุ่ม: " + label);
    }

    public void ContinueAfterVictory()
    {
        Debug.Log("CONTINUE BUTTON CLICKED");

        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        Time.timeScale = 1f;

        // ปลดล็อกเมาส์ให้เกม FPS จัดการต่อเอง
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Continue after boss victory.");
    }

    public void ExitAfterVictory()
    {
        Debug.Log("EXIT TO MAIN MENU BUTTON CLICKED");

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            Debug.LogError("ยังไม่ได้กำหนดชื่อ Main Menu Scene");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
