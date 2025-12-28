#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using HyperloopDash.Managers;
using HyperloopDash.Gameplay;
using HyperloopDash.UI;
using HyperloopDash.Ads;

// Note: Ensure the Runtime scripts are compiled before running this.
// This script assumes standard Namespace.

public class HeadlessBuilder
{
    [MenuItem("Tools/Generate Game Content")]
    public static void GenerateGameContent()
    {
        Debug.Log("Generating Game Content...");
        CreateTags();
        CreateMaterials();
        CreatePrefabs();
        CreateScene();
        Debug.Log("Game Content Generation Complete!");
    }

    [MenuItem("Tools/Build Android APK")]
    public static void PerformAndroidBuild()
    {
        GenerateGameContent(); // Ensure fresh content

        string buildPath = "Build/HyperloopDash.apk";
        string[] levels = new string[] { "Assets/Scenes/Main.unity" };

        Debug.Log("Building Android APK to: " + buildPath);

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = levels;
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
             Debug.Log("Build Succeeded!");
        }
        else
        {
             Debug.LogError("Build Failed: " + report.summary.totalErrors + " errors.");
        }
    }

    static void CreateTags()
    {
        string[] tags = new string[] { "Obstacle", "Collectible", "SignalBar" };
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        foreach (string t in tags)
        {
            bool found = false;
            for (int i = 0; i < tagsProp.arraySize; i++)
            {
                SerializedProperty tProp = tagsProp.GetArrayElementAtIndex(i);
                if (tProp.stringValue.Equals(t)) { found = true; break; }
            }

            if (!found)
            {
                tagsProp.InsertArrayElementAtIndex(0);
                SerializedProperty n = tagsProp.GetArrayElementAtIndex(0);
                n.stringValue = t;
            }
        }
        tagManager.ApplyModifiedProperties();
    }

    static void CreateMaterials()
    {
        if (!Directory.Exists("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");

        CreateMaterial("NeonRed", Color.red, true);
        CreateMaterial("NeonBlue", Color.cyan, true);
        CreateMaterial("NeonGreen", Color.green, true);
        CreateMaterial("DarkFloor", Color.black, false);
    }

    static void CreateMaterial(string name, Color color, bool emissive)
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        if (emissive)
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2f);
        }
        AssetDatabase.CreateAsset(mat, "Assets/Materials/" + name + ".mat");
    }

    static void CreatePrefabs()
    {
        if (!Directory.Exists("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        // 1. SIMPLIFIED WORKING TUNNEL (Track with walls)
        GameObject tunnel = new GameObject("TunnelSegment");
        
        // Floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.SetParent(tunnel.transform);
        floor.transform.localPosition = new Vector3(0, -0.5f, 0);
        floor.transform.localScale = new Vector3(10, 0.2f, 20);
        floor.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DarkFloor.mat");
        Object.DestroyImmediate(floor.GetComponent<Collider>());
        
        // Left Wall
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.SetParent(tunnel.transform);
        leftWall.transform.localPosition = new Vector3(-5, 2, 0);
        leftWall.transform.localScale = new Vector3(0.5f, 5, 20);
        leftWall.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonBlue.mat");
        Object.DestroyImmediate(leftWall.GetComponent<Collider>());
        
        // Right Wall
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.SetParent(tunnel.transform);
        rightWall.transform.localPosition = new Vector3(5, 2, 0);
        rightWall.transform.localScale = new Vector3(0.5f, 5, 20);
        rightWall.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonBlue.mat");
        Object.DestroyImmediate(rightWall.GetComponent<Collider>());
        
        // Ceiling (optional, for enclosed feel)
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ceiling.name = "Ceiling";
        ceiling.transform.SetParent(tunnel.transform);
        ceiling.transform.localPosition = new Vector3(0, 4.5f, 0);
        ceiling.transform.localScale = new Vector3(10, 0.2f, 20);
        ceiling.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DarkFloor.mat");
        Object.DestroyImmediate(ceiling.GetComponent<Collider>());
        
        PrefabUtility.SaveAsPrefabAsset(tunnel, "Assets/Prefabs/TunnelSegment.prefab");
        Object.DestroyImmediate(tunnel);

        // 2. Obstacle (Properly scaled)
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.name = "Obstacle_Debris";
        obs.tag = "Obstacle";
        obs.transform.localScale = new Vector3(1, 1, 1);
        obs.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonRed.mat");
        obs.AddComponent<HyperloopDash.Gameplay.Obstacle>();
        obs.GetComponent<BoxCollider>().isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(obs, "Assets/Prefabs/Obstacle_Debris.prefab");
        Object.DestroyImmediate(obs);

        // 3. Collectible
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "EnergyOrb";
        orb.tag = "Collectible";
        orb.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        orb.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonGreen.mat");
        orb.AddComponent<HyperloopDash.Gameplay.Collectible>();
        orb.GetComponent<SphereCollider>().isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(orb, "Assets/Prefabs/EnergyOrb.prefab");
        Object.DestroyImmediate(orb);

        // 4. SignalBar
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "Obstacle_Bar";
        bar.tag = "SignalBar";
        bar.transform.localScale = new Vector3(8, 0.3f, 0.3f);
        bar.transform.position = new Vector3(0, 1, 0);
        bar.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonRed.mat");
        bar.AddComponent<HyperloopDash.Gameplay.SignalBar>();
        bar.GetComponent<BoxCollider>().isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(bar, "Assets/Prefabs/Obstacle_Bar.prefab");
        Object.DestroyImmediate(bar);

        // 5. GateBlocker
        GameObject gate = new GameObject("Obstacle_Blocker");
        gate.tag = "Obstacle";
        gate.AddComponent<HyperloopDash.Gameplay.GateBlocker>();
        gate.AddComponent<HyperloopDash.Gameplay.Obstacle>();
        
        for (int i = 0; i < 3; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.SetParent(gate.transform);
            float x = (i - 1) * 3f; // -3, 0, 3
            block.transform.localPosition = new Vector3(x, 1, 0);
            block.transform.localScale = new Vector3(2, 2, 0.5f);
            block.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonRed.mat");
            block.GetComponent<BoxCollider>().isTrigger = true;
            block.tag = "Obstacle";
        }
        
        PrefabUtility.SaveAsPrefabAsset(gate, "Assets/Prefabs/Obstacle_Blocker.prefab");
        Object.DestroyImmediate(gate);
    }

    static void CreateScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 1. Camera (Behind and above player for 3D perspective)
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera c = cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(0, 2, -8);
        cam.transform.rotation = Quaternion.Euler(10, 0, 0);
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Color.black;
        c.fieldOfView = 75; // Wider FOV for tunnel effect

        // 2. Light
        GameObject light = new GameObject("Directional Light");
        Light l = light.AddComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 0.8f;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);

        // 3. Managers
        GameObject managers = new GameObject("Managers");
        managers.AddComponent<GameManager>();
        ObjectPooler pooler = managers.AddComponent<ObjectPooler>();
        managers.AddComponent<AdManager>();
        UIManager uiManager = managers.AddComponent<UIManager>();

        // Setup Pooler
        pooler.items = new System.Collections.Generic.List<PoolItem>();
        AddPoolItem(pooler, "TunnelSegment", "Assets/Prefabs/TunnelSegment.prefab", 10);
        AddPoolItem(pooler, "Obstacle_Debris", "Assets/Prefabs/Obstacle_Debris.prefab", 10);
        AddPoolItem(pooler, "Obstacle_Bar", "Assets/Prefabs/Obstacle_Bar.prefab", 10);
        AddPoolItem(pooler, "Obstacle_Blocker", "Assets/Prefabs/Obstacle_Blocker.prefab", 10);
        AddPoolItem(pooler, "EnergyOrb", "Assets/Prefabs/EnergyOrb.prefab", 20);

        // 4. Player (Proper scale for track)
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 0.5f, 0); // Start above floor
        player.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 0.5f, 0);
        cc.radius = 0.3f;
        cc.height = 1.2f;
        
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.crashParticles = null;
        pc.laneDistance = 3f; // 3 lanes: -3, 0, 3
        
        // Camera with follow controller
        CameraController camController = cam.AddComponent<CameraController>();
        camController.target = player.transform;
        camController.offset = new Vector3(0, 4, -6); // Higher and closer
        camController.smoothSpeed = 10f;
        camController.lookAtTarget = false; // Keep camera angle fixed
        
        // Add SwipeInput to managers
        managers.AddComponent<HyperloopDash.Helpers.SwipeInput>();
        
        // 5. Environment / Spawner
        GameObject env = new GameObject("Environment");
        TrackManager track = env.AddComponent<TrackManager>();
        track.tunnelPrefabs = null; // Unused in favor of Pool, or drag prefab here if needed.
        // Actually TrackManager uses pool. Fix TrackManager if it needs direct ref.
        // Ah, current TrackManager code uses ObjectPooler "TunnelSegment". Code is fine.
        track.playerTransform = player.transform;

        GameObject spawnerObj = new GameObject("Spawner");
        Spawner spawner = spawnerObj.AddComponent<Spawner>();
        spawner.playerTransform = player.transform;

        // 6. UI Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        UnityEngine.UI.CanvasScaler scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920); // Portrait
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Panels
        GameObject mainMenu = CreatePanel(canvasObj, "MainMenuPanel", Color.black);
        GameObject hud = CreatePanel(canvasObj, "HUDPanel", Color.clear);
        GameObject gameOver = CreatePanel(canvasObj, "GameOverPanel", new Color(0,0,0, 0.8f));
        
        // Fix: Disable non-menu panels immediately in the serialized scene
        hud.SetActive(false);
        gameOver.SetActive(false);
        mainMenu.SetActive(true);
        
        uiManager.mainMenuPanel = mainMenu;
        uiManager.hudPanel = hud;
        uiManager.gameOverPanel = gameOver;

        // DEBUG CONSOLE (Added for "Overhaul" request)
        GameObject debugPanel = CreatePanel(canvasObj, "DebugPanel", new Color(0,0,0, 0.2f)); // Semi-transparent
        UnityEngine.UI.Text debugText = CreateText(debugPanel, "LogText", "Waiting for logs...", Vector2.zero, Vector2.one, Vector2.zero);
        debugText.rectTransform.offsetMin = new Vector2(20, 20); // Padding
        debugText.rectTransform.offsetMax = new Vector2(-20, -20);
        debugText.alignment = TextAnchor.UpperLeft;
        debugText.fontSize = 20;
        
        HyperloopDash.Helpers.InGameLogConsole console = debugPanel.AddComponent<HyperloopDash.Helpers.InGameLogConsole>();
        console.logText = debugText;
        
        // Ensure Debug Panel is last sibling to render on top
        debugPanel.transform.SetAsLastSibling();
        debugPanel.GetComponent<UnityEngine.UI.Image>().raycastTarget = false; // Pass through clicks
        debugText.raycastTarget = false;

        // Fill HUD
        uiManager.scoreText = CreateText(hud, "ScoreText", "SCORE: 0", new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -50));
        uiManager.energyText = CreateText(hud, "EnergyText", "0", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50));
        uiManager.comboText = CreateText(hud, "ComboText", "x1", new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -100));

        // Fill Main Menu
        CreateText(mainMenu, "Title", "HYPERLOOP DASH", new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero).fontSize = 60;
        UnityEngine.UI.Button playBtn = CreateButton(mainMenu, "PlayButton", "PLAY");
        playBtn.onClick.AddListener(uiManager.OnPlayClicked);

        // Fill Game Over
        uiManager.finalScoreText = CreateText(gameOver, "Score", "SCORE: 0", new Vector2(0.5f, 0.7f), new Vector2(0.5f, 0.7f), Vector2.zero);
        uiManager.bestScoreText = CreateText(gameOver, "Best", "BEST: 0", new Vector2(0.5f, 0.6f), new Vector2(0.5f, 0.6f), Vector2.zero);
        
        UnityEngine.UI.Button restartBtn = CreateButton(gameOver, "RestartButton", "RESTART");
        restartBtn.transform.localPosition = new Vector3(0, 0, 0); // Center
        restartBtn.onClick.AddListener(uiManager.OnRestartClicked);
        
        uiManager.reviveButton = CreateButton(gameOver, "ReviveButton", "REVIVE");
        uiManager.reviveButton.transform.localPosition = new Vector3(-200, -100, 0);
        uiManager.reviveButton.onClick.AddListener(uiManager.OnReviveClicked);
        
        uiManager.doubleEnergyButton = CreateButton(gameOver, "DoubleEnergyButton", "x2 ENERGY");
        uiManager.doubleEnergyButton.transform.localPosition = new Vector3(200, -100, 0);
        uiManager.doubleEnergyButton.onClick.AddListener(uiManager.OnDoubleEnergyClicked);

        // Force Portrait
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        // Force Legacy Input System to ensure StandaloneInputModule works
        // (Use Reflection or int cast if Enum not available in this context, but InputSystemType.Legacy is standard)
        // actually PlayerSettings.activeInputHandler returns an enum. 
        // We need to set it. Legacy is normally 0 or handled via "Both".
        // Let's safe-bet specific assignment if possible, or just add the right module.
        // Easiest is to just enable Legacy input.
        // SerializedProperty logic could be safer if API varies, but let's try direct.
        // Note: New Input System package might force itself if installed.
        // Let's just create the proper InputModule?
        // No, let's force the setting.
#if UNITY_2020_2_OR_NEWER
        // 0 = Legacy, 1 = New, 2 = Both
        // We'll set it to 2 (Both) or 0 (Legacy) to be safe.
        // Unity API: PlayerSettings.activeInputHandler
        // However, this is an internal setting often. 
        // Let's try reflection to set "activeInputHandler" to 0 (Legacy).
        // Actually, let's just use the SerializedObject on ProjectSettings.
#endif

        // Save Scene
        if (!Directory.Exists("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), "Assets/Scenes/Main.unity");
    }

    // Helpers
    static void AddPoolItem(ObjectPooler pooler, string tag, string path, int size)
    {
        PoolItem item = new PoolItem();
        item.tag = tag;
        item.prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        item.size = size;
        pooler.items.Add(item);
    }

    static GameObject CreatePanel(GameObject parent, string name, Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        UnityEngine.UI.Image img = panel.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        // Disable raycast for all panels by default if they are just containers?
        // No, MainMenu needs clicks. HUD does NOT. GameOver DOES.
        // Explicit logic:
        if (name == "HUDPanel") img.raycastTarget = false;
        
        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return panel;
    }

    static UnityEngine.UI.Text CreateText(GameObject parent, string name, string content, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        UnityEngine.UI.Text txt = obj.AddComponent<UnityEngine.UI.Text>();
        txt.text = content;
        txt.fontSize = 36;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.horizontalOverflow = HorizontalWrapMode.Overflow; // FIX: Allow overflow
        txt.verticalOverflow = VerticalWrapMode.Overflow; 
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(800, 200); // FIX: Much wider box
        return txt;
    }

    static UnityEngine.UI.Button CreateButton(GameObject parent, string name, string label)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);
        UnityEngine.UI.Image img = btnObj.AddComponent<UnityEngine.UI.Image>();
        img.color = Color.white;
        UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);
        UnityEngine.UI.Text txt = textObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = label;
        txt.fontSize = 24;
        txt.color = Color.black;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 60);
        
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        return btn;
    }
}
#endif
