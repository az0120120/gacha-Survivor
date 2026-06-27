#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyCatalog))]
public class EnemyCatalogEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(8f);
        EditorGUILayout.HelpBox(
            "在此配置三种怪物的图片、子弹图片与数值。\n" +
            "• Sprite：怪物外观\n" +
            "• Projectile Sprite：远程怪子弹（仅远程有效）\n" +
            "• 将资源拖到 Wave Spawner 的 Enemy Catalog 字段生效",
            MessageType.Info);

        var catalog = (EnemyCatalog)target;

        if (GUILayout.Button("填入默认三种怪物配置", GUILayout.Height(28f)))
        {
            Undo.RecordObject(catalog, "Fill Default Enemy Definitions");
            catalog.SetArchetypes(EnemyCatalog.CreateDefaultDefinitions());
            EditorUtility.SetDirty(catalog);
        }
    }

    [MenuItem("Assets/Create/GachaSurvivor/Enemy Catalog (With Defaults)", false, 1)]
    static void CreateCatalogWithDefaults()
    {
        var catalog = ScriptableObject.CreateInstance<EnemyCatalog>();
        catalog.SetArchetypes(EnemyCatalog.CreateDefaultDefinitions());

        EnsureAssetFolder("Assets/Xinyou/Data");

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Xinyou/Data/EnemyCatalog.asset");
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
