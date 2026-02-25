#if UNITY_EDITOR
using System.Collections.Generic;
using SurvivorMaster.Core;
using SurvivorMaster.Player;
using SurvivorMaster.Systems;
using SurvivorMaster.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SurvivorMaster.EditorTools
{
    public static class DemoSceneSetupTool
    {
        private const string DemoScenePath = "Assets/Scenes/DemoScene.unity";

        [MenuItem("Tools/SurvivorMaster/Setup Demo Scene")]
        public static void SetupDemoScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                EditorUtility.DisplayDialog("Scene Setup", "Open a scene first.", "OK");
                return;
            }

            if (scene.path != DemoScenePath)
            {
                bool proceed = EditorUtility.DisplayDialog(
                    "Scene Setup",
                    "Active scene is not Assets/Scenes/DemoScene.unity. Continue anyway?",
                    "Continue",
                    "Cancel");
                if (!proceed)
                {
                    return;
                }
            }

            EnsureEventSystem();
            EnsureArena();

            GameObject player = EnsurePlayer();
            Camera mainCamera = EnsureCamera(player != null ? player.transform : null);

            GameObject root = Ensure("GameRoot");
            GameTimer timer = EnsureComponent<GameTimer>(root);
            EnemyManager enemyManager = EnsureComponent<EnemyManager>(root);
            ProjectileManager projectileManager = EnsureComponent<ProjectileManager>(root);
            XPOrbManager xpOrbManager = EnsureComponent<XPOrbManager>(root);
            XPManager xpManager = EnsureComponent<XPManager>(root);
            EnemySpawner spawner = EnsureComponent<EnemySpawner>(root);
            DemoSceneBootstrap bootstrap = EnsureComponent<DemoSceneBootstrap>(root);

            LevelUpUI levelUpUI;
            VirtualJoystick joystick;
            HUDController hud;
            PerformanceDebugOverlay perfOverlay;
            SetupUI(out levelUpUI, out joystick, out hud, out perfOverlay);

            SetupPlayerReferences(player, joystick);
            SetupManagerReferences(player, mainCamera, enemyManager, projectileManager, xpOrbManager, xpManager, spawner, bootstrap, levelUpUI, hud, perfOverlay);

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("Scene Setup", "Demo scene setup complete.", "OK");
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
        }

        private static void EnsureArena()
        {
            GameObject plane = GameObject.Find("ArenaPlane");
            if (plane == null)
            {
                plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                plane.name = "ArenaPlane";
                Undo.RegisterCreatedObjectUndo(plane, "Create Arena Plane");
            }

            plane.transform.position = Vector3.zero;
            plane.transform.localScale = new Vector3(5f, 1f, 5f);

            Material arenaMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Prefabs/M_Arena_Unlit.mat");
            if (arenaMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader != null)
                {
                    arenaMaterial = new Material(shader) { color = new Color(0.12f, 0.14f, 0.17f, 1f) };
                    AssetDatabase.CreateAsset(arenaMaterial, "Assets/Prefabs/M_Arena_Unlit.mat");
                }
            }

            MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
            if (renderer != null && arenaMaterial != null)
            {
                renderer.sharedMaterial = arenaMaterial;
            }

            Light directional = Object.FindFirstObjectByType<Light>();
            if (directional == null || directional.type != LightType.Directional)
            {
                GameObject lightGo = new GameObject("Directional Light");
                directional = lightGo.AddComponent<Light>();
                directional.type = LightType.Directional;
                Undo.RegisterCreatedObjectUndo(lightGo, "Create Directional Light");
            }

            directional.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            directional.intensity = 1f;
        }

        private static GameObject EnsurePlayer()
        {
            GameObject existing = GameObject.FindWithTag("Player");
            if (existing != null)
            {
                return existing;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
            GameObject player;
            if (prefab != null)
            {
                player = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            }
            else
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                EnsureComponent<CharacterController>(player);
                EnsureComponent<PlayerStats>(player);
                EnsureComponent<PlayerController>(player);
                EnsureComponent<AutoAttack>(player);
            }

            if (player == null)
            {
                return null;
            }

            if (!player.CompareTag("Player"))
            {
                player.tag = "Player";
            }

            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            return player;
        }

        private static Camera EnsureCamera(Transform playerTarget)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject cameraGo = new GameObject("Main Camera");
                cam = cameraGo.AddComponent<Camera>();
                cameraGo.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(cameraGo, "Create Main Camera");
            }

            cam.transform.position = new Vector3(0f, 20f, -14f);
            cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);

            CameraFollow follow = EnsureComponent<CameraFollow>(cam.gameObject);
            if (playerTarget != null)
            {
                follow.SetTarget(playerTarget);
            }

            return cam;
        }

        private static void SetupUI(out LevelUpUI levelUpUI, out VirtualJoystick joystick, out HUDController hud, out PerformanceDebugOverlay perf)
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasGo.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;

                Undo.RegisterCreatedObjectUndo(canvasGo, "Create Canvas");
            }

            GameObject hudRoot = Ensure("HUD", canvas.transform);

            GameObject hpBarGo = EnsureBar("HPBar", new Vector2(280, 24), new Vector2(0.03f, 0.96f), hudRoot.transform, out SimpleBar hpBar);
            GameObject xpBarGo = EnsureBar("XPBar", new Vector2(420, 20), new Vector2(0.5f, 0.04f), hudRoot.transform, out SimpleBar xpBar);

            Text levelText = EnsureText("LevelText", "Lv 1", new Vector2(0.03f, 0.90f), hudRoot.transform);
            Text enemyText = EnsureText("EnemyCountText", "Enemies: 0", new Vector2(0.85f, 0.96f), hudRoot.transform);
            Text timerText = EnsureText("TimerText", "Time: 00:00", new Vector2(0.85f, 0.91f), hudRoot.transform);
            Text perfText = EnsureText("PerformanceText", "", new Vector2(0.03f, 0.78f), hudRoot.transform);

            perf = EnsureComponent<PerformanceDebugOverlay>(perfText.gameObject);
            SetObjectReference(perf, "output", perfText);

            hud = EnsureComponent<HUDController>(hudRoot);
            SetObjectReference(hud, "hpBar", hpBar);
            SetObjectReference(hud, "xpBar", xpBar);
            SetObjectReference(hud, "levelText", levelText);
            SetObjectReference(hud, "enemyCountText", enemyText);
            SetObjectReference(hud, "timerText", timerText);

            GameObject joystickRoot = Ensure("Joystick", canvas.transform);
            RectTransform joyRect = EnsureRect(joystickRoot);
            joyRect.anchorMin = new Vector2(0.08f, 0.12f);
            joyRect.anchorMax = new Vector2(0.08f, 0.12f);
            joyRect.sizeDelta = new Vector2(200f, 200f);

            Image joyBg = EnsureComponent<Image>(joystickRoot);
            joyBg.color = new Color(1f, 1f, 1f, 0.16f);

            GameObject handleGo = Ensure("Handle", joystickRoot.transform);
            RectTransform handleRect = EnsureRect(handleGo);
            handleRect.sizeDelta = new Vector2(80f, 80f);
            Image handleImg = EnsureComponent<Image>(handleGo);
            handleImg.color = new Color(1f, 1f, 1f, 0.35f);

            joystick = EnsureComponent<VirtualJoystick>(joystickRoot);
            SetObjectReference(joystick, "handle", handleRect);

            GameObject levelUpRoot = Ensure("LevelUpPanel", canvas.transform);
            RectTransform panelRect = EnsureRect(levelUpRoot);
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(860f, 320f);
            Image panelImage = EnsureComponent<Image>(levelUpRoot);
            panelImage.color = new Color(0f, 0f, 0f, 0.78f);

            levelUpUI = EnsureComponent<LevelUpUI>(levelUpRoot);
            SetObjectReference(levelUpUI, "root", levelUpRoot);

            List<Button> buttons = new List<Button>();
            List<Text> titles = new List<Text>();
            List<Text> descriptions = new List<Text>();

            for (int i = 0; i < 3; i++)
            {
                GameObject buttonGo = Ensure($"Option{i + 1}", levelUpRoot.transform);
                RectTransform bRect = EnsureRect(buttonGo);
                bRect.anchorMin = new Vector2(0.17f + i * 0.33f, 0.5f);
                bRect.anchorMax = bRect.anchorMin;
                bRect.sizeDelta = new Vector2(220f, 220f);

                Image bImg = EnsureComponent<Image>(buttonGo);
                bImg.color = new Color(1f, 1f, 1f, 0.1f);
                Button button = EnsureComponent<Button>(buttonGo);
                buttons.Add(button);

                Text title = EnsureText("Title", "Upgrade", new Vector2(0.5f, 0.78f), buttonGo.transform);
                title.alignment = TextAnchor.MiddleCenter;
                title.resizeTextForBestFit = true;
                titles.Add(title);

                Text description = EnsureText("Description", "+Stat", new Vector2(0.5f, 0.45f), buttonGo.transform);
                description.alignment = TextAnchor.MiddleCenter;
                description.resizeTextForBestFit = true;
                descriptions.Add(description);
            }

            SetArray(levelUpUI, "optionButtons", buttons);
            SetArray(levelUpUI, "optionTitles", titles);
            SetArray(levelUpUI, "optionDescriptions", descriptions);
            levelUpRoot.SetActive(false);

            _ = hpBarGo;
            _ = xpBarGo;
        }

        private static void SetupPlayerReferences(GameObject player, VirtualJoystick joystick)
        {
            if (player == null)
            {
                return;
            }

            PlayerController controller = EnsureComponent<PlayerController>(player);
            EnsureComponent<CharacterController>(player);
            EnsureComponent<PlayerStats>(player);
            EnsureComponent<AutoAttack>(player);

            SetObjectReference(controller, "virtualJoystick", joystick);
        }

        private static void SetupManagerReferences(
            GameObject player,
            Camera camera,
            EnemyManager enemyManager,
            ProjectileManager projectileManager,
            XPOrbManager xpOrbManager,
            XPManager xpManager,
            EnemySpawner spawner,
            DemoSceneBootstrap bootstrap,
            LevelUpUI levelUpUI,
            HUDController hud,
            PerformanceDebugOverlay perf)
        {
            GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
            GameObject projectilePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Projectile.prefab");
            GameObject orbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/XPOrb.prefab");

            SetObjectReference(enemyManager, "enemyPrefab", enemyPrefab != null ? enemyPrefab.GetComponent<Enemy.Enemy>() : null);
            SetObjectReference(projectileManager, "projectilePrefab", projectilePrefab != null ? projectilePrefab.GetComponent<Projectile>() : null);
            SetObjectReference(xpOrbManager, "xpOrbPrefab", orbPrefab != null ? orbPrefab.GetComponent<XPOrb>() : null);

            PlayerController playerController = player != null ? player.GetComponent<PlayerController>() : null;
            PlayerStats playerStats = player != null ? player.GetComponent<PlayerStats>() : null;
            CameraFollow follow = camera != null ? camera.GetComponent<CameraFollow>() : null;

            SetObjectReference(spawner, "player", playerController);
            SetObjectReference(spawner, "enemyManager", enemyManager);

            SetObjectReference(xpManager, "playerStats", playerStats);
            SetObjectReference(xpManager, "levelUpUI", levelUpUI);

            SetObjectReference(bootstrap, "player", playerController);
            SetObjectReference(bootstrap, "cameraFollow", follow);
            SetObjectReference(bootstrap, "enemyManager", enemyManager);
            SetObjectReference(bootstrap, "xpOrbManager", xpOrbManager);

            SetObjectReference(hud, "playerStats", playerStats);
            SetObjectReference(hud, "xpManager", xpManager);

            UpgradeDefinition[] upgrades = LoadUpgrades();
            SetArray(xpManager, "availableUpgrades", upgrades);

            _ = perf;
        }

        private static UpgradeDefinition[] LoadUpgrades()
        {
            string[] guids = AssetDatabase.FindAssets("t:UpgradeDefinition", new[] { "Assets/ScriptableObjects" });
            List<UpgradeDefinition> list = new List<UpgradeDefinition>(guids.Length);
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                UpgradeDefinition asset = AssetDatabase.LoadAssetAtPath<UpgradeDefinition>(path);
                if (asset != null)
                {
                    list.Add(asset);
                }
            }

            return list.ToArray();
        }

        private static GameObject Ensure(string name, Transform parent = null)
        {
            Transform existing = parent != null ? parent.Find(name) : null;
            if (existing != null)
            {
                return existing.gameObject;
            }

            if (parent == null)
            {
                GameObject rootExisting = GameObject.Find(name);
                if (rootExisting != null)
                {
                    return rootExisting;
                }
            }

            GameObject go = new GameObject(name);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }

        private static RectTransform EnsureRect(GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect == null)
            {
                rect = go.AddComponent<RectTransform>();
            }

            return rect;
        }

        private static GameObject EnsureBar(string name, Vector2 size, Vector2 anchor, Transform parent, out SimpleBar bar)
        {
            GameObject root = Ensure(name, parent);
            RectTransform rootRect = EnsureRect(root);
            rootRect.anchorMin = anchor;
            rootRect.anchorMax = anchor;
            rootRect.sizeDelta = size;

            Image bg = EnsureComponent<Image>(root);
            bg.color = new Color(0f, 0f, 0f, 0.6f);

            GameObject fill = Ensure("Fill", root.transform);
            RectTransform fillRect = EnsureRect(fill);
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(1f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            Image fillImg = EnsureComponent<Image>(fill);
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 1f;
            fillImg.color = name.Contains("HP") ? new Color(0.9f, 0.22f, 0.22f, 1f) : new Color(0.2f, 0.7f, 1f, 1f);

            bar = EnsureComponent<SimpleBar>(root);
            SetObjectReference(bar, "fillImage", fillImg);
            return root;
        }

        private static Text EnsureText(string name, string value, Vector2 anchor, Transform parent)
        {
            GameObject go = Ensure(name, parent);
            RectTransform rect = EnsureRect(go);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(280f, 36f);

            Text text = EnsureComponent<Text>(go);
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = value;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = Color.white;
            return text;
        }

        private static T EnsureComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(go);
            }

            return component;
        }

        private static void SetObjectReference(Object target, string fieldName, Object value)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetArray<T>(Object target, string fieldName, IList<T> values) where T : Object
        {
            if (target == null)
            {
                return;
            }

            SerializedObject so = new SerializedObject(target);
            SerializedProperty property = so.FindProperty(fieldName);
            if (property == null || !property.isArray)
            {
                return;
            }

            property.arraySize = values != null ? values.Count : 0;
            if (values != null)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                }
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetArray<T>(Object target, string fieldName, T[] values) where T : Object
        {
            SetArray(target, fieldName, values as IList<T>);
        }
    }
}
#endif
