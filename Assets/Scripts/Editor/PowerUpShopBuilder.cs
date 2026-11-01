using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class PowerUpShopBuilder
{
    private const string ResourceFolder = "Assets/Resources/PowerUps";
    private const string ArtFolder = "Assets/PowerUps/Materials";
    private static TMP_FontAsset font;

    [MenuItem("Tools/Power Ups/Build Shop Assets")]
    public static void Build()
    {
        EnsureFolder(ResourceFolder);
        EnsureFolder(ArtFolder);
        font = TMP_Settings.defaultFontAsset;
        if (font == null) throw new InvalidOperationException("TextMesh Pro needs a default font before building the shop.");
        string catalogPath = ResourceFolder + "/PowerUpCatalog.asset";
        if (AssetDatabase.LoadAssetAtPath<PowerUpCatalog>(catalogPath) == null)
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PowerUpCatalog>(), catalogPath);

        Scene preview = EditorSceneManager.NewPreviewScene();
        try
        {
            BuildShop(preview);
            BuildHud(preview);
            BuildPhone(preview);
        }
        finally { EditorSceneManager.ClosePreviewScene(preview); }
        AssetDatabase.SaveAssets();
        InstallPhones();
        Debug.Log("Built power-up shop, HUD, catalog, and phones for MainGame, GarageSale, and Mall.");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    private static GameObject Root(string name, Scene scene, bool ui)
    {
        GameObject root = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
        SceneManager.MoveGameObjectToScene(root, scene);
        root.hideFlags = HideFlags.None;
        if (ui)
        {
            root.AddComponent<RectTransform>();
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            root.AddComponent<GraphicRaycaster>();
        }
        return root;
    }

    private static RectTransform Rect(Transform parent, string name, float x, float y, float width, float height)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    private static TMP_Text Text(Transform parent, string name, string value, float x, float y,
        float width, float height, float size, Color color, bool bold = false)
    {
        TextMeshProUGUI text = Rect(parent, name, x, y, width, height).gameObject.AddComponent<TextMeshProUGUI>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        text.raycastTarget = false;
        text.enableWordWrapping = true;
        return text;
    }

    private static Button Button(Transform parent, string name, string label, float x, float y,
        float width, out TMP_Text text)
    {
        RectTransform rect = Rect(parent, name, x, y, width, 52);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(0.12f, 0.39f, 0.42f);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.65f, 1, 0.95f);
        colors.disabledColor = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        button.colors = colors;
        text = Text(rect, name + " Label", label, 10, 0, width - 20, 52, 20, Color.white, true);
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private static void BuildShop(Scene preview)
    {
        GameObject root = Root("PowerUpShop", preview, true);
        PowerUpShop shop = root.AddComponent<PowerUpShop>();
        RectTransform backdrop = Rect(root.transform, "Backdrop", 0, 0, 0, 0);
        backdrop.anchorMin = Vector2.zero;
        backdrop.anchorMax = Vector2.one;
        backdrop.offsetMin = backdrop.offsetMax = Vector2.zero;
        backdrop.gameObject.AddComponent<Image>().color = new Color(0.015f, 0.025f, 0.045f, 0.94f);
        RectTransform panel = Rect(backdrop, "Shop Panel", 0, 0, 920, 620);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0.5f, 0.5f);
        panel.anchoredPosition = Vector2.zero;
        panel.gameObject.AddComponent<Image>().color = new Color(0.055f, 0.085f, 0.13f);
        Color accent = new Color(0.45f, 0.92f, 0.84f);
        Color muted = new Color(0.69f, 0.76f, 0.84f);
        shop.heading = Text(panel, "Heading", "GEAR UP", 44, 32, 480, 70, 48, Color.white, true);
        shop.balance = Text(panel, "Balance", "0 POINTS", 580, 45, 296, 48, 28, new Color(1, 0.78f, 0.35f), true);
        shop.balance.alignment = TextAlignmentOptions.Right;
        Text(panel, "Subtitle", "ONE ATTEMPT. YOUR CHOICE OF ADVANTAGE.", 44, 106, 832, 35, 19, muted);

        RectTransform scouter = Card(panel, "Scouter Card", 44, accent);
        Text(scouter, "Title", "01 / SCOUTER", 24, 20, 344, 42, 26, accent, true);
        Text(scouter, "Description", "Track your held item's assigned destination.\n\nActive for this level attempt.", 24, 76, 344, 108, 19, Color.white);
        shop.scouterButton = Button(scouter, "Equip Scouter", "EQUIP", 24, 190, 344, out shop.scouterLabel);
        RectTransform phone = Card(panel, "Phone Card", 484, accent);
        TMP_Text phoneTitle = Text(phone, "Title", "02 / PHONE DISTRACTION", 24, 20, 344, 42, 25, accent, true);
        phoneTitle.enableWordWrapping = false;
        phoneTitle.enableAutoSizing = true;
        phoneTitle.fontSizeMin = 18;
        phoneTitle.fontSizeMax = 25;
        Text(phone, "Description", "Ring the location's phone to draw staff away.\n3 answered calls, then the location blocks your number.", 24, 76, 344, 108, 19, Color.white);
        shop.phoneButton = Button(phone, "Equip Phone", "EQUIP", 24, 190, 344, out shop.phoneLabel);
        shop.totalLabel = Text(panel, "Total", "", 44, 430, 832, 32, 19, accent, true);
        shop.message = Text(panel, "Status", "", 44, 470, 832, 48, 18, muted);
        shop.cancelButton = Button(panel, "Back", "BACK", 44, 542, 392, out _);
        shop.startButton = Button(panel, "Start Attempt", "START ATTEMPT", 484, 542, 392, out _);
        PrefabUtility.SaveAsPrefabAsset(root, ResourceFolder + "/PowerUpShop.prefab");
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static RectTransform Card(Transform parent, string name, float x, Color accent)
    {
        RectTransform card = Rect(parent, name, x, 158, 392, 258);
        card.gameObject.AddComponent<Image>().color = new Color(0.09f, 0.135f, 0.19f);
        Rect(card, "Accent", 0, 0, 392, 3).gameObject.AddComponent<Image>().color = accent;
        return card;
    }

    private static void BuildHud(Scene preview)
    {
        GameObject root = Root("PowerUpHUD", preview, true);
        PowerUpHUD hud = root.AddComponent<PowerUpHUD>();
        root.GetComponent<Canvas>().sortingOrder = 40;
        RectTransform panel = Rect(root.transform, "Power-Up Status", 0, 0, 470, 135);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(1, 0);
        panel.anchoredPosition = new Vector2(-28, 28);
        Image background = panel.gameObject.AddComponent<Image>();
        background.color = new Color(0.025f, 0.05f, 0.08f, 0.87f);
        background.raycastTarget = false;
        hud.scouterText = Text(panel, "Scouter", "SCOUTER ONLINE", 18, 12, 434, 30, 20, new Color(0.45f, 0.92f, 0.84f), true);
        hud.phoneText = Text(panel, "Phone", "[P] CALL LOCATION / 3 LEFT", 18, 48, 434, 30, 20, Color.white, true);
        hud.statusText = Text(panel, "Status", "", 18, 88, 434, 36, 17, new Color(0.72f, 0.8f, 0.86f));
        PrefabUtility.SaveAsPrefabAsset(root, ResourceFolder + "/PowerUpHUD.prefab");
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static Material Material(string name, Color color)
    {
        string path = ArtFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        if (shader == null) throw new InvalidOperationException("No supported shader was found for the phone.");
        material = new Material(shader) { name = name, color = color };
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Renderer Block(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = scale;
        part.layer = 2;
        part.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        MeshRenderer renderer = part.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        return renderer;
    }

    private static void BuildPhone(Scene preview)
    {
        GameObject root = Root("StorePhone", preview, false);
        StorePhone phone = root.AddComponent<StorePhone>();
        phone.ringSource = root.AddComponent<AudioSource>();
        phone.ringSource.playOnAwake = false;
        GameObject answer = new GameObject("Answer Point");
        answer.transform.SetParent(root.transform, false);
        phone.answerPoint = answer.transform;
        Material dark = Material("Phone Body", new Color(0.055f, 0.085f, 0.13f));
        Material teal = Material("Phone Receiver", new Color(0.12f, 0.65f, 0.6f));
        Material light = Material("Phone Keys", new Color(0.7f, 0.8f, 0.84f));
        Block(root.transform, "Phone Body", new Vector3(0, 0.06f, 0), new Vector3(0.5f, 0.12f, 0.36f), dark);
        Block(root.transform, "Receiver", new Vector3(0, 0.17f, -0.1f), new Vector3(0.43f, 0.06f, 0.085f), teal);
        Block(root.transform, "Receiver Left", new Vector3(-0.18f, 0.15f, -0.1f), new Vector3(0.1f, 0.08f, 0.13f), teal);
        Block(root.transform, "Receiver Right", new Vector3(0.18f, 0.15f, -0.1f), new Vector3(0.1f, 0.08f, 0.13f), teal);
        for (int row = 0; row < 3; row++)
        for (int column = 0; column < 3; column++)
            Block(root.transform, "Key", new Vector3((column - 1) * 0.055f, 0.126f, row * 0.05f), new Vector3(0.035f, 0.012f, 0.03f), light);
        phone.ringIndicator = Block(root.transform, "Ring Light", new Vector3(0.18f, 0.13f, 0.12f), new Vector3(0.045f, 0.02f, 0.045f), teal);
        Renderer stand = Block(root.transform, "Phone Stand", new Vector3(0, -0.45f, 0), new Vector3(0.65f, 0.9f, 0.5f), dark);
        stand.gameObject.SetActive(false);
        PrefabUtility.SaveAsPrefabAsset(root, ResourceFolder + "/StorePhone.prefab");
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void InstallPhones()
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ResourceFolder + "/StorePhone.prefab");
        foreach (string path in new[] { "Assets/Scenes/MainGame.unity", "Assets/Scenes/GarageSale.unity", "Assets/Scenes/Mall.unity" })
        {
            Scene scene = SceneManager.GetSceneByPath(path);
            bool opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            else if (scene.isDirty) throw new InvalidOperationException("Save " + path + " before installing a phone.");
            try
            {
                EnemyAI first = null;
                bool alreadyInstalled = false;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.GetComponentInChildren<StorePhone>(true) != null) alreadyInstalled = true;
                    if (first == null) first = root.GetComponentInChildren<EnemyAI>(true);
                }
                if (alreadyInstalled) continue;
                if (first == null || first.deskReturn == null) throw new InvalidOperationException("A reachable desk marker is needed in " + path);
                Vector3 answer = first.deskReturn.position;
                Renderer counter = null;
                float best = 2 * 2;
                foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
                {
                    if (renderer.name.IndexOf("counter", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    float distance = (renderer.bounds.center - answer).sqrMagnitude;
                    if (distance >= best) continue;
                    best = distance;
                    counter = renderer;
                }
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
                instance.transform.position = counter != null
                    ? new Vector3(counter.bounds.center.x, counter.bounds.max.y + 0.01f, counter.bounds.center.z)
                    : answer + Vector3.up * 0.9f;
                instance.GetComponent<StorePhone>().answerPoint.position = answer;
                instance.transform.Find("Phone Stand").gameObject.SetActive(counter == null);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.GetComponent<StorePhone>().answerPoint);
                PrefabUtility.RecordPrefabInstancePropertyModifications(instance.transform.Find("Phone Stand").gameObject);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally { if (opened) EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
