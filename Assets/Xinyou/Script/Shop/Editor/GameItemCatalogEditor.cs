#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameItemCatalog))]
public class GameItemCatalogEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "在此列表中配置所有商店与升级道具。\n" +
            "• 类型选「商店-*」→ 进入商店池\n" +
            "• 类型选「升级 - 属性强化」→ 进入升级池\n" +
            "• Icon 直接拖入图片即可显示",
            MessageType.Info);

        var catalog = (GameItemCatalog)target;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("填入默认全部道具", GUILayout.Height(28f)))
        {
            Undo.RecordObject(catalog, "Fill Default Items");
            catalog.SetEntries(GameItemCatalog.CreateDefaultEntries());
            EditorUtility.SetDirty(catalog);
        }

        if (GUILayout.Button("仅商店道具", GUILayout.Height(28f)))
        {
            Undo.RecordObject(catalog, "Fill Shop Items");
            var all = GameItemCatalog.CreateDefaultEntries();
            all.RemoveAll(entry => entry.kind == GameItemKind.LevelUpStat);
            catalog.SetEntries(all);
            EditorUtility.SetDirty(catalog);
        }

        if (GUILayout.Button("仅升级道具", GUILayout.Height(28f)))
        {
            Undo.RecordObject(catalog, "Fill Level Up Items");
            var all = GameItemCatalog.CreateDefaultEntries();
            all.RemoveAll(entry => entry.kind != GameItemKind.LevelUpStat);
            catalog.SetEntries(all);
            EditorUtility.SetDirty(catalog);
        }
        EditorGUILayout.EndHorizontal();
    }

    [MenuItem("Assets/Create/GachaSurvivor/Game Item Catalog (With Defaults)", false, 0)]
    static void CreateCatalogWithDefaults()
    {
        var catalog = ScriptableObject.CreateInstance<GameItemCatalog>();
        catalog.SetEntries(GameItemCatalog.CreateDefaultEntries());

        EnsureAssetFolder("Assets/Xinyou/Data");

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Xinyou/Data/GameItemCatalog.asset");
        AssetDatabase.CreateAsset(catalog, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = catalog;
    }

    static void EnsureAssetFolder(string folderPath)
    {
        if (AssetDatabase.IsValidFolder(folderPath))
            return;

        const string assetsRoot = "Assets";
        if (!folderPath.StartsWith(assetsRoot + "/") && folderPath != assetsRoot)
            return;

        string[] parts = folderPath.Substring(assetsRoot.Length).TrimStart('/').Split('/');
        string current = assetsRoot;

        for (int i = 0; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
