using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public class SOItemCreatorTests
{
    private SOItemCreator window;
    private string folder;
    private Scene previewScene;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        string name = "__SOItemCreatorTests_" + Guid.NewGuid().ToString("N");
        AssetDatabase.CreateFolder("Assets", name);
        folder = "Assets/" + name;
        previewScene = EditorSceneManager.NewPreviewScene();
        window = ScriptableObject.CreateInstance<SOItemCreator>();
        window.targetFolder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(folder);
        window.generatePreviewSprites = false;
    }

    [TearDown]
    public void TearDown()
    {
        if (window != null) Object.DestroyImmediate(window);
        if (previewScene.IsValid()) EditorSceneManager.ClosePreviewScene(previewScene);
        if (!string.IsNullOrEmpty(folder)) AssetDatabase.DeleteAsset(folder);
    }

    private object Invoke(string name, params object[] arguments)
    {
        try { return typeof(SOItemCreator).GetMethod(name, Private).Invoke(window, arguments); }
        catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
    }

    private void Set(string name, object value) { typeof(SOItemCreator).GetField(name, Private).SetValue(window, value); }
    private T Get<T>(string name) { return (T)typeof(SOItemCreator).GetField(name, Private).GetValue(window); }

    private GameObject Prefab(string name, bool addItem = false)
    {
        GameObject source = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
        SceneManager.MoveGameObjectToScene(source, previewScene);
        source.hideFlags = HideFlags.None;
        if (addItem) source.AddComponent<Item>();
        try { return PrefabUtility.SaveAsPrefabAsset(source, folder + "/" + name + ".prefab"); }
        finally { Object.DestroyImmediate(source); }
    }

    private void RunBatch()
    {
        Invoke("BeginBatch");
        for (int tick = 0; tick < 100 && Get<bool>("running"); tick++) Invoke("ProcessNext");
        Assert.That(Get<bool>("running"), Is.False, "Non-preview batches should finish without waiting.");
        Assert.That(Get<int>("failed"), Is.Zero, Get<string>("summary"));
    }

    private GameObject MeshPrefab(string name, bool meshOnRoot = true)
    {
        GameObject source = EditorUtility.CreateGameObjectWithHideFlags(name, HideFlags.HideAndDontSave);
        SceneManager.MoveGameObjectToScene(source, previewScene);
        source.hideFlags = HideFlags.None;
        source.transform.SetPositionAndRotation(new Vector3(4, 5, 6), Quaternion.Euler(13, 37, 9));
        source.transform.localScale = new Vector3(-2, 3, 0.5f);
        source.AddComponent<Item>();
        BoxCollider box = source.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.center = new Vector3(3, 5, 7);
        box.size = new Vector3(6, 7, 8);
        GameObject model = source;
        if (!meshOnRoot)
        {
            model = new GameObject("Model");
            model.transform.SetParent(source.transform, false);
            model.transform.localPosition = new Vector3(2, -3, 5);
            model.transform.localScale = new Vector3(2, 4, 6);
        }
        model.AddComponent<MeshFilter>().sharedMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        model.AddComponent<MeshRenderer>().sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
        try { return PrefabUtility.SaveAsPrefabAsset(source, folder + "/" + name + ".prefab"); }
        finally { Object.DestroyImmediate(source); }
    }

    private static void CloseTo(Vector3 actual, Vector3 expected)
    {
        Assert.That(Vector3.Distance(actual, expected), Is.LessThan(0.0001f));
    }

    [Test]
    public void MovesRootMeshToChildAndKeepsItemAndColliderOnRoot()
    {
        GameObject prefab = MeshPrefab("Root mesh");
        Mesh mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        Material material = prefab.GetComponent<MeshRenderer>().sharedMaterial;
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        Assert.That(prefab.GetComponent<MeshFilter>(), Is.Null);
        Assert.That(prefab.GetComponent<MeshRenderer>(), Is.Null);
        Assert.That(prefab.GetComponent<Item>(), Is.Not.Null);
        Assert.That(prefab.GetComponents<BoxCollider>().Length, Is.EqualTo(1));
        Assert.That(prefab.GetComponent<BoxCollider>().isTrigger, Is.True);
        Assert.That(prefab.transform.childCount, Is.EqualTo(1));
        GameObject child = prefab.transform.GetChild(0).gameObject;
        Assert.That(child.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(mesh));
        Assert.That(child.GetComponent<MeshRenderer>().sharedMaterial, Is.SameAs(material));
        Assert.That(child.GetComponent<Item>(), Is.Null);
        Assert.That(child.GetComponent<BoxCollider>(), Is.Null);
        CloseTo(child.transform.localPosition, Vector3.zero);
        CloseTo(prefab.GetComponent<BoxCollider>().center, Vector3.zero);
        CloseTo(prefab.GetComponent<BoxCollider>().size, Vector3.one);
        RunBatch();
        Assert.That(Get<int>("skipped"), Is.EqualTo(1));
        Assert.That(prefab.transform.childCount, Is.EqualTo(1));
    }

    [Test]
    public void BottomCenterAlignmentPreservesRootTransformAndIsIdempotent()
    {
        GameObject prefab = MeshPrefab("Pivot", false);
        Vector3 position = prefab.transform.position;
        Quaternion rotation = prefab.transform.rotation;
        Vector3 scale = prefab.transform.localScale;
        Set("alignBottomCenter", true);
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        Assert.That(BoxColliderEditorUtils.TryGetLocalBounds(prefab, out Bounds bounds), Is.True);
        CloseTo(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), Vector3.zero);
        CloseTo(prefab.GetComponent<BoxCollider>().center, bounds.center);
        CloseTo(prefab.GetComponent<BoxCollider>().size, bounds.size);
        CloseTo(prefab.transform.position, position);
        CloseTo(prefab.transform.localScale, scale);
        Assert.That(Quaternion.Angle(prefab.transform.rotation, rotation), Is.LessThan(0.001f));
        Vector3 modelPosition = prefab.transform.GetChild(0).localPosition;
        RunBatch();
        Assert.That(prefab.transform.childCount, Is.EqualTo(1));
        CloseTo(prefab.transform.GetChild(0).localPosition, modelPosition);
    }

    [Test]
    public void ColliderFitHandlesRotatedMirroredRootsAndInactiveChildren()
    {
        GameObject prefab = MeshPrefab("Fit", false);
        GameObject contents = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
        try
        {
            contents.transform.GetChild(0).gameObject.SetActive(false);
            BoxCollider box = contents.GetComponent<BoxCollider>();
            Assert.That(BoxColliderEditorUtils.FitToChildren(box, false), Is.True);
            CloseTo(box.center, new Vector3(2, -3, 5));
            CloseTo(box.size, new Vector3(2, 4, 6));
        }
        finally { PrefabUtility.UnloadPrefabContents(contents); }
    }

    [Test]
    public void StructuralRepairSupportsUndoRedo()
    {
        GameObject prefab = MeshPrefab("Undo structure");
        window.prefabsToConvert.Add(prefab);
        Set("alignBottomCenter", true);
        RunBatch();
        Assert.That(prefab.GetComponent<MeshFilter>(), Is.Null);
        Assert.That(prefab.transform.childCount, Is.EqualTo(1));
        Undo.PerformUndo();
        Assert.That(prefab.GetComponent<MeshFilter>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<MeshRenderer>(), Is.Not.Null);
        Assert.That(prefab.transform.childCount, Is.Zero);
        CloseTo(prefab.GetComponent<BoxCollider>().center, new Vector3(3, 5, 7));
        CloseTo(prefab.GetComponent<BoxCollider>().size, new Vector3(6, 7, 8));
        Undo.PerformRedo();
        Assert.That(prefab.GetComponent<MeshFilter>(), Is.Null);
        Assert.That(prefab.transform.childCount, Is.EqualTo(1));
        Assert.That(prefab.GetComponent<Item>().itemData, Is.Not.Null);
        Assert.That(BoxColliderEditorUtils.TryGetLocalBounds(prefab, out Bounds bounds), Is.True);
        CloseTo(new Vector3(bounds.center.x, bounds.min.y, bounds.center.z), Vector3.zero);
    }

    private object Entry(GameObject prefab)
    {
        window.prefabsToConvert.Clear();
        window.prefabsToConvert.Add(prefab);
        Invoke("RefreshReview");
        return Get<IList>("review")[0];
    }

    [Test]
    public void FolderSearchDefaultsToItemsAndCanIncludeOtherPrefabs()
    {
        GameObject item = Prefab("Has Item", true);
        GameObject other = Prefab("No Item");
        Set("searchFolder", window.targetFolder);
        Assert.That(Get<bool>("onlyItemPrefabs"), Is.True);
        Invoke("FindUnassignedItemPrefabs");
        Assert.That(window.prefabsToConvert, Is.EquivalentTo(new[] { item }));
        Set("onlyItemPrefabs", false);
        Invoke("FindUnassignedItemPrefabs");
        Assert.That(window.prefabsToConvert, Is.EquivalentTo(new[] { item, other }));
        Assert.That(other.GetComponent<Item>(), Is.Null, "Searching must not modify prefabs.");
    }

    [Test]
    public void SearchWithoutFolderAlwaysRequiresItemEvenAfterFolderFilterIsDisabled()
    {
        GameObject item = Prefab("Has Item", true);
        GameObject other = Prefab("No Item");
        Set("onlyItemPrefabs", false);
        Invoke("FindUnassignedItemPrefabs");
        Assert.That(window.prefabsToConvert, Does.Contain(item));
        Assert.That(window.prefabsToConvert.Contains(other), Is.False);
        foreach (GameObject found in window.prefabsToConvert)
            Assert.That(found.GetComponent<Item>(), Is.Not.Null);
    }

    [Test]
    public void AddsMissingRootComponentAndRerunReusesTheSameAsset()
    {
        GameObject prefab = Prefab("New item");
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        Item component = prefab.GetComponent<Item>();
        Assert.That(component, Is.Not.Null);
        ItemData data = component.itemData;
        Assert.That(data, Is.Not.Null);
        Assert.That(data.itemPrefab, Is.EqualTo(prefab));
        Assert.That(data.itemName, Is.EqualTo("New item"));
        string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(data));
        Assert.That(Get<int>("componentsAdded"), Is.EqualTo(1));
        RunBatch();
        Assert.That(Get<int>("created"), Is.Zero);
        Assert.That(Get<int>("skipped"), Is.EqualTo(1));
        Assert.That(AssetDatabase.FindAssets("t:ItemData", new[] { folder }).Length, Is.EqualTo(1));
        Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(component.itemData)), Is.EqualTo(guid));
    }

    [Test]
    public void UpdatePreservesAuthoredFieldsAndCustomSprite()
    {
        GameObject prefab = Prefab("Custom");
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        ItemData data = prefab.GetComponent<Item>().itemData;
        data.itemName = "Hand-written name";
        data.itemOffset = new Vector3(1, 2, 3);
        data.itemScale = new Vector3(4, 5, 6);
        data.editScale = true;
        data.itemSpriteColor = Color.magenta;
        data.swappableItems.Add(data);
        Texture2D texture = new Texture2D(8, 8);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), Vector2.zero);
        try
        {
            data.itemSprite = sprite;
            Set("mode", SOItemCreator.CreationMode.CreateOrUpdate);
            window.generatePreviewSprites = true;
            RunBatch();
            Assert.That(prefab.GetComponent<Item>().itemData, Is.SameAs(data));
            Assert.That(data.itemName, Is.EqualTo("Hand-written name"));
            Assert.That(data.itemOffset, Is.EqualTo(new Vector3(1, 2, 3)));
            Assert.That(data.itemScale, Is.EqualTo(new Vector3(4, 5, 6)));
            Assert.That(data.editScale, Is.True);
            Assert.That(data.itemSpriteColor, Is.EqualTo(Color.magenta));
            Assert.That(data.itemSprite, Is.SameAs(sprite));
            Assert.That(data.swappableItems, Is.EqualTo(new[] { data }));
            Assert.That(AssetDatabase.FindAssets("t:ItemData", new[] { folder }).Length, Is.EqualTo(1));
        }
        finally { data.itemSprite = null; Object.DestroyImmediate(sprite); Object.DestroyImmediate(texture); }
    }

    [Test]
    public void RecoversAnUnlinkedAssetWithoutMakingADuplicate()
    {
        GameObject prefab = Prefab("Orphan");
        ItemData data = ScriptableObject.CreateInstance<ItemData>();
        data.itemPrefab = prefab;
        data.itemName = "Keep this name";
        AssetDatabase.CreateAsset(data, folder + "/Existing.asset");
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        Assert.That(prefab.GetComponent<Item>().itemData, Is.SameAs(data));
        Assert.That(data.itemName, Is.EqualTo("Keep this name"));
        Assert.That(Get<int>("created"), Is.Zero);
        Assert.That(Get<int>("updated"), Is.EqualTo(1));
    }

    [Test]
    public void DuplicateAndInvalidEntriesAreSkipped()
    {
        GameObject prefab = Prefab("Valid");
        window.prefabsToConvert.Add(prefab);
        window.prefabsToConvert.Add(null);
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        Assert.That(Get<int>("created"), Is.EqualTo(1));
        Assert.That(Get<int>("skipped"), Is.EqualTo(2));
    }

    [Test]
    public void VariantGetsItsOwnDataAndLeavesBaseDataUntouched()
    {
        GameObject original = Prefab("Base");
        window.prefabsToConvert.Add(original);
        RunBatch();
        ItemData baseData = original.GetComponent<Item>().itemData;
        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(original, previewScene);
        GameObject variant;
        try { variant = PrefabUtility.SaveAsPrefabAsset(instance, folder + "/Variant.prefab"); }
        finally { Object.DestroyImmediate(instance); }
        Assert.That(PrefabUtility.GetPrefabAssetType(variant), Is.EqualTo(PrefabAssetType.Variant));
        window.prefabsToConvert.Clear();
        window.prefabsToConvert.Add(variant);
        RunBatch();
        ItemData variantData = variant.GetComponent<Item>().itemData;
        Assert.That(variantData, Is.Not.EqualTo(baseData));
        Assert.That(variantData.itemPrefab, Is.EqualTo(variant));
        Assert.That(baseData.itemPrefab, Is.EqualTo(original));
        AssetDatabase.ImportAsset(folder + "/Variant.prefab", ImportAssetOptions.ForceUpdate);
        Assert.That(AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/Variant.prefab").GetComponent<Item>().itemData, Is.EqualTo(variantData));
    }

    [Test]
    public void CancelLeavesRemainingPrefabsUntouched()
    {
        GameObject first = Prefab("First");
        GameObject second = Prefab("Second");
        window.prefabsToConvert.Add(first);
        window.prefabsToConvert.Add(second);
        Invoke("BeginBatch");
        Invoke("ProcessNext");
        Invoke("CancelBatch");
        Assert.That(Get<bool>("running"), Is.False);
        Assert.That(first.GetComponent<Item>(), Is.Not.Null);
        Assert.That(second.GetComponent<Item>(), Is.Null);
        Assert.That(Get<string>("summary"), Does.Contain("1 remaining"));
    }

    [Test]
    public void UndoRestoresTheMissingComponentAndRedoRestoresItsLink()
    {
        GameObject prefab = Prefab("Undo");
        window.prefabsToConvert.Add(prefab);
        RunBatch();
        Assert.That(prefab.GetComponent<Item>().itemData, Is.Not.Null);
        Undo.PerformUndo();
        Assert.That(prefab.GetComponent<Item>(), Is.Null);
        Undo.PerformRedo();
        Assert.That(prefab.GetComponent<Item>(), Is.Not.Null);
        Assert.That(prefab.GetComponent<Item>().itemData, Is.Not.Null);
        Assert.That(AssetDatabase.Contains(prefab.GetComponent<Item>().itemData), Is.True);
    }

    [Test]
    public void GeneratedIconsAreSpritesAndRegenerationReusesTheirPath()
    {
        GameObject prefab = Prefab("Icon");
        Texture2D texture = new Texture2D(8, 8);
        try
        {
            Set("outputFolder", folder);
            RenderTexture before = RenderTexture.active;
            Invoke("ProcessEntry", Entry(prefab), texture);
            ItemData data = prefab.GetComponent<Item>().itemData;
            Assert.That(data.itemSprite, Is.Not.Null);
            string iconPath = AssetDatabase.GetAssetPath(data.itemSprite);
            Assert.That(iconPath, Does.EndWith(".png"));
            Assert.That(RenderTexture.active, Is.EqualTo(before));
            Set("mode", SOItemCreator.CreationMode.CreateOrUpdate);
            Invoke("ProcessEntry", Entry(prefab), texture);
            Assert.That(AssetDatabase.GetAssetPath(data.itemSprite), Is.EqualTo(iconPath));
            Assert.That(Directory.GetFiles(folder, "*.png").Length, Is.EqualTo(1));
        }
        finally { Object.DestroyImmediate(texture); }
    }

    [Test]
    public void PreviewWaitYieldsInsteadOfBlockingAndCanBeCancelled()
    {
        GameObject prefab = Prefab("Waiting preview");
        window.prefabsToConvert.Add(prefab);
        window.generatePreviewSprites = true;
        Invoke("BeginBatch");
        Invoke("ProcessNext");
        // A preview may already be cached; otherwise nothing is written while it is pending.
        if (Get<bool>("running")) Assert.That(prefab.GetComponent<Item>(), Is.Null);
        Invoke("CancelBatch");
        Assert.That(Get<bool>("running"), Is.False);
    }

    [Test]
    public void RejectedAssetWriteLeavesNoBrokenPrefabLink()
    {
        GameObject prefab = Prefab("Rejected write");
        object entry = Entry(prefab);
        Set("outputFolder", folder + "/MissingFolder");
        bool previous = LogAssert.ignoreFailingMessages;
        try
        {
            LogAssert.ignoreFailingMessages = true;
            Assert.Catch<Exception>(() => Invoke("ProcessEntry", entry, null));
            Assert.That(prefab.GetComponent<Item>(), Is.Null);
            Assert.That(AssetDatabase.FindAssets("t:ItemData", new[] { folder }).Length, Is.Zero);
        }
        finally { LogAssert.ignoreFailingMessages = previous; }
    }

    [Test]
    public void FailingItemDoesNotStopTheRestOfTheBatch()
    {
        GameObject first = Prefab("Cannot write");
        GameObject second = Prefab("Still succeeds");
        window.prefabsToConvert.Add(first);
        window.prefabsToConvert.Add(second);
        Invoke("BeginBatch");
        string path = AssetDatabase.GetAssetPath(first);
        FileAttributes original = File.GetAttributes(path);
        try
        {
            File.SetAttributes(path, original | FileAttributes.ReadOnly);
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex("InvalidOperationException: Prefab is read-only"));
            Invoke("ProcessNext");
            Invoke("ProcessNext");
            Assert.That(Get<int>("failed"), Is.EqualTo(1));
            Assert.That(Get<int>("created"), Is.EqualTo(1));
            Assert.That(first.GetComponent<Item>(), Is.Null);
            Assert.That(second.GetComponent<Item>(), Is.Not.Null);
        }
        finally { File.SetAttributes(path, original); }
    }
}
