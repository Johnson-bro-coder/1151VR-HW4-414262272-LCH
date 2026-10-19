#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneBuilder
{
    [MenuItem("Tools/Build Flight Scene")]
    public static void BuildScene()
    {
        // 1. 建立全新空白場景
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // 2. 設置光源與天空
        Light dirLight = Object.FindFirstObjectByType<Light>();
        if (dirLight != null)
        {
            dirLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            dirLight.color = new Color(1f, 0.96f, 0.9f);
            dirLight.intensity = 1.2f;
        }

        // 3. 建立平整地面 (Runway / Ground)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, 0, 150);
        ground.transform.localScale = new Vector3(50, 1, 50);
        Material groundMat = new Material(Shader.Find("Standard"));
        groundMat.color = new Color(0.25f, 0.35f, 0.28f); // 灰草綠色
        ground.GetComponent<Renderer>().material = groundMat;
        ground.AddComponent<HazardObstacle>(); // 撞到地面也算墜機

        // 4. 建立音效管理器 AudioManager
        GameObject audioMgrObj = new GameObject("AudioManager");
        AudioManager audioMgr = audioMgrObj.AddComponent<AudioManager>();
        AudioClip flyLoop = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Airplane_Fly_Loop.wav");
        AudioClip crash = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Obstacle_Crash.wav");
        AudioClip pass = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pass_Ring_Success.wav");

        SerializedObject audioSo = new SerializedObject(audioMgr);
        audioSo.FindProperty("engineFlyLoopClip").objectReferenceValue = flyLoop;
        audioSo.FindProperty("crashClip").objectReferenceValue = crash;
        audioSo.FindProperty("passRingClip").objectReferenceValue = pass;
        audioSo.ApplyModifiedProperties();

        // 5. 載入 3D 飛行器模型
        GameObject planeObj = new GameObject("PlayerAirplane");
        planeObj.transform.position = new Vector3(0, 8f, 0);
        planeObj.transform.rotation = Quaternion.identity;

        // 載入 Airplane.obj 網格
        GameObject planePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/Airplane.obj");
        if (planePrefab != null)
        {
            GameObject planeVisual = Object.Instantiate(planePrefab, planeObj.transform);
            planeVisual.name = "VisualModel";
            planeVisual.transform.localPosition = Vector3.zero;
            planeVisual.transform.localRotation = Quaternion.identity;
            
            Material planeMat = new Material(Shader.Find("Standard"));
            planeMat.color = new Color(0.12f, 0.45f, 0.85f); // 酷炫飛機藍
            planeVisual.GetComponentInChildren<Renderer>().material = planeMat;
        }

        AirplaneController planeController = planeObj.AddComponent<AirplaneController>();
        BoxCollider planeCollider = planeObj.AddComponent<BoxCollider>();
        planeCollider.size = new Vector3(3.5f, 1.2f, 3.5f);

        // 6. 設置主攝影機跟隨
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            SmoothFollowCamera followCam = mainCam.gameObject.AddComponent<SmoothFollowCamera>();
            followCam.target = planeObj.transform;
            mainCam.transform.position = planeObj.transform.position + new Vector3(0, 2f, -6f);
            mainCam.transform.LookAt(planeObj.transform.position + Vector3.forward * 4f);
        }

        // 7. 載入障礙物模型 (RingObstacle 與 PillarObstacle)
        GameObject ringPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/RingObstacle.obj");
        GameObject pillarPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Models/PillarObstacle.obj");

        // 光圈材質 (亮金黃)
        Material ringMat = new Material(Shader.Find("Standard"));
        ringMat.color = new Color(1f, 0.82f, 0.1f);
        ringMat.EnableKeyword("_EMISSION");
        ringMat.SetColor("_EmissionColor", new Color(0.4f, 0.32f, 0.05f));

        // 石柱材質 (暗灰色)
        Material pillarMat = new Material(Shader.Find("Standard"));
        pillarMat.color = new Color(0.38f, 0.38f, 0.42f);

        // 擺放光圈跑道 (5 個連貫檢查點光圈)
        Vector3[] ringPositions = new Vector3[]
        {
            new Vector3(0f, 8f, 35f),
            new Vector3(8f, 11f, 80f),
            new Vector3(4f, 15f, 130f),
            new Vector3(-10f, 12f, 180f),
            new Vector3(0f, 14f, 240f)
        };

        for (int i = 0; i < ringPositions.Length; i++)
        {
            GameObject ring = new GameObject($"Ring_{i + 1}");
            ring.transform.position = ringPositions[i];

            // 實例化視覺環
            if (ringPrefab != null)
            {
                GameObject ringVisual = Object.Instantiate(ringPrefab, ring.transform);
                ringVisual.name = "RingVisual";
                ringVisual.transform.localPosition = Vector3.zero;
                ringVisual.GetComponentInChildren<Renderer>().material = ringMat;
            }

            // 中心穿透觸發器 (Trigger)
            SphereCollider triggerCollider = ring.AddComponent<SphereCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = 2.2f;

            // 附著光圈邏輯
            RingObstacle ringLogic = ring.AddComponent<RingObstacle>();

            // 外框實體碰撞體 (若撞到外圈算撞毀)
            GameObject outerFrame = new GameObject("HazardOuterFrame");
            outerFrame.transform.SetParent(ring.transform);
            outerFrame.transform.localPosition = Vector3.zero;
            BoxCollider outerColTop = outerFrame.AddComponent<BoxCollider>();
            outerColTop.center = new Vector3(0, 3.1f, 0);
            outerColTop.size = new Vector3(6f, 0.6f, 1f);
            BoxCollider outerColBot = outerFrame.AddComponent<BoxCollider>();
            outerColBot.center = new Vector3(0, -3.1f, 0);
            outerColBot.size = new Vector3(6f, 0.6f, 1f);
            outerFrame.AddComponent<HazardObstacle>();
        }

        // 擺放危險障礙物石柱 (增添飛行閃避刺激感)
        Vector3[] pillarPositions = new Vector3[]
        {
            new Vector3(-8f, 0f, 55f),
            new Vector3(14f, 0f, 105f),
            new Vector3(-4f, 0f, 155f),
            new Vector3(10f, 0f, 210f)
        };

        for (int i = 0; i < pillarPositions.Length; i++)
        {
            GameObject pillar = new GameObject($"PillarHazard_{i + 1}");
            pillar.transform.position = pillarPositions[i];

            if (pillarPrefab != null)
            {
                GameObject pillarVisual = Object.Instantiate(pillarPrefab, pillar.transform);
                pillarVisual.name = "PillarVisual";
                pillarVisual.transform.localPosition = Vector3.zero;
                pillarVisual.transform.localScale = new Vector3(1.4f, 2.0f, 1.4f);
                pillarVisual.GetComponentInChildren<Renderer>().material = pillarMat;
            }

            CapsuleCollider col = pillar.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0, 12f, 0);
            col.radius = 1.4f;
            col.height = 24f;
            pillar.AddComponent<HazardObstacle>();
        }

        // 8. 建立遊戲管理器 GameManager
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        gm.playerAirplane = planeController;

        // 9. 保存場景
        string scenePath = "Assets/Scenes/MainFlightScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[SceneBuilder] 成功建立並保存 3D 飛行過關場景至: {scenePath}");

        // 10. 將場景註冊到 Build Settings
        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("[SceneBuilder] 已自動登錄場景至 EditorBuildSettings.scenes，隨時可輸出手機 Build！");
    }
}
#endif
