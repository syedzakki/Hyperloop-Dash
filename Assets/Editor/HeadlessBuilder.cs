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

        // 1. Tunnel Segment
        GameObject tunnel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        tunnel.transform.localScale = new Vector3(20, 1, 20); // Floor
        // Add walls?
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.transform.SetParent(tunnel.transform);
        leftWall.transform.localPosition = new Vector3(-0.45f, 1f, 0); // Local relative
        leftWall.transform.localScale = new Vector3(0.1f, 2, 1);
        
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.transform.SetParent(tunnel.transform);
        rightWall.transform.localPosition = new Vector3(0.45f, 1f, 0);
        rightWall.transform.localScale = new Vector3(0.1f, 2, 1);

        tunnel.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/DarkFloor.mat");
        leftWall.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonBlue.mat");
        rightWall.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonBlue.mat");
        
        PrefabUtility.SaveAsPrefabAsset(tunnel, "Assets/Prefabs/TunnelSegment.prefab");
        Object.DestroyImmediate(tunnel);

        // 2. Obstacle (Generic)
        GameObject obs = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obs.name = "Obstacle_Debris";
        obs.tag = "Obstacle";
        obs.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonRed.mat");
        obs.AddComponent<HyperloopDash.Gameplay.Obstacle>();
        obs.AddComponent<BoxCollider>().isTrigger = true; // Use collider as trigger
        PrefabUtility.SaveAsPrefabAsset(obs, "Assets/Prefabs/Obstacle_Debris.prefab");
        Object.DestroyImmediate(obs);

        // 3. Collectible
        GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "EnergyOrb";
        orb.tag = "Collectible";
        orb.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonGreen.mat");
        orb.AddComponent<HyperloopDash.Gameplay.Collectible>();
        orb.GetComponent<SphereCollider>().isTrigger = true;
        PrefabUtility.SaveAsPrefabAsset(orb, "Assets/Prefabs/EnergyOrb.prefab");
        Object.DestroyImmediate(orb);

        // 4. SignalBar
        GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bar.name = "Obstacle_Bar";
        bar.tag = "SignalBar";
        bar.transform.localScale = new Vector3(10, 0.5f, 0.5f); // Wide
        bar.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonRed.mat");
        bar.AddComponent<HyperloopDash.Gameplay.SignalBar>();
        bar.GetComponent<BoxCollider>().isTrigger = true; 
        PrefabUtility.SaveAsPrefabAsset(bar, "Assets/Prefabs/Obstacle_Bar.prefab");
        Object.DestroyImmediate(bar);

        // 5. GateBlocker
        GameObject gate = new GameObject("Obstacle_Blocker");
        gate.tag = "Obstacle";
        gate.AddComponent<HyperloopDash.Gameplay.GateBlocker>();
        gate.AddComponent<HyperloopDash.Gameplay.Obstacle>(); // Base class needed?
        
        // Children
        for (int i = 0; i < 3; i++)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.transform.SetParent(gate.transform);
            float x = (i - 1) * 3.0f; // -3, 0, 3
            block.transform.localPosition = new Vector3(x, 1, 0);
            block.GetComponent<Renderer>().material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/NeonRed.mat");
            block.GetComponent<BoxCollider>().isTrigger = true;
            block.tag = "Obstacle"; // Child collision
        }
        
        PrefabUtility.SaveAsPrefabAsset(gate, "Assets/Prefabs/Obstacle_Blocker.prefab");
        Object.DestroyImmediate(gate);
    }

    static void CreateScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        
        // 1. Camera
        GameObject cam = new GameObject("Main Camera");
        cam.tag = "MainCamera";
        Camera c = cam.AddComponent<Camera>();
        cam.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(0, 5, -8);
        cam.transform.rotation = Quaternion.Euler(20, 0, 0);
        c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = Color.black;

        // 2. Light
        GameObject light = new GameObject("Directional Light");
        Light l = light.AddComponent<Light>();
        l.type = LightType.Directional;
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

        // 4. Player
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player"; // Default tag
        CharacterController cc = player.AddComponent<CharacterController>();
        PlayerController pc = player.AddComponent<PlayerController>();
        pc.crashParticles = null; // Add if you have a particle prefab
        // Make camera child of player for simple follow if preferred, or keep static relative
        cam.transform.SetParent(player.transform);
        
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
        
        uiManager.mainMenuPanel = mainMenu;
        uiManager.hudPanel = hud;
        uiManager.gameOverPanel = gameOver;

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
        // If panel is transparent (HUD), don't block raycasts
        if (color.a < 0.1f) img.raycastTarget = false;
        
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
        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = new Vector2(300, 100);
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
