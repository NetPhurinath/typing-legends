#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class WordbankTieredListAutoCreate
{
    private const string AssetPath = "Assets/Resources/Wordbanks/Ramayana_TieredList.asset";
    private const string AssetPath_1_3 = "Assets/Resources/Wordbanks/Ramayana_TieredList_1_3.asset";
    private const string AssetPath_4_6 = "Assets/Resources/Wordbanks/Ramayana_TieredList_4_6.asset";
    private const string AssetPath_7_10 = "Assets/Resources/Wordbanks/Ramayana_TieredList_7_10.asset";

    [MenuItem("Typing Legends/Wordbanks/Create Ramayana Tiered List (Easy/Medium/Hard)")]
    public static void CreateAsset()
    {
        var existing = AssetDatabase.LoadAssetAtPath<WordbankTieredList>(AssetPath);
        if (existing != null)
        {
            Selection.activeObject = existing;
            EditorGUIUtility.PingObject(existing);
            Debug.Log($"Already exists: {AssetPath}");
            return;
        }

        var asset = ScriptableObject.CreateInstance<WordbankTieredList>();

        // Easy (1-20)
        asset.easy.AddRange(new[]
        {
            "วานร", "อสุรา", "มัจฉา", "ปักษา", "คีรี", "นภา", "ราตรี", "กระบี่", "กุมภ์", "นาคา",
            "นารายณ์", "ลักษณ์", "สีดา", "บรรพต", "วายุ", "หาว", "พนา", "ชลธี", "จันทร", "อาทิตย์"
        });

        // Medium (21-40)
        asset.medium.AddRange(new[]
        {
            "พานรินทร์", "ยักษา", "อสุรี", "วายุบุตร", "พจนารถ", "รณรงค์", "พลับพลา", "ไสยา", "ศิโรเพฐน์", "สัประยุทธ์",
            "พนาลัย", "มรคา", "ภูวไนย", "เยาวมาลย์", "โยธา", "ไยไพ", "ลีลาศ", "ศุภฤกษ์", "เหมันต์", "ภพไตร"
        });

        // Hard (41-50)
        asset.hard.AddRange(new[]
        {
            "กัมปนาท", "ไกรสร", "มัชฌิมราตรี", "โกฏิ", "คัคนานต์", "ทศพักตร์", "ทิพยโสตนัยนา", "พรหมมาสตร์", "โตมร", "ศิลา"
        });

        AssetDatabase.CreateAsset(asset, AssetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
        Debug.Log($"Created: {AssetPath}");
    }

    [MenuItem("Typing Legends/Wordbanks/Create Grouped Ramayana Tiered Lists (1-3, 4-6, 7-10)")]
    public static void CreateGroupedAssets()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Wordbanks");

        var a = LoadOrCreateGrouped(AssetPath_1_3, "Ramayana_TieredList_1_3");
        var b = LoadOrCreateGrouped(AssetPath_4_6, "Ramayana_TieredList_4_6");
        var c = LoadOrCreateGrouped(AssetPath_7_10, "Ramayana_TieredList_7_10");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.objects = new Object[] { a, b, c };
        EditorGUIUtility.PingObject(a);
        Debug.Log("Grouped Ramayana tiered lists ready under Assets/Resources/Wordbanks");
    }

    [MenuItem("Typing Legends/Wordbanks/Select Grouped Ramayana Tiered Lists (1-3, 4-6, 7-10)")]
    public static void SelectGroupedAssets()
    {
        var a = AssetDatabase.LoadAssetAtPath<WordbankTieredList>(AssetPath_1_3);
        var b = AssetDatabase.LoadAssetAtPath<WordbankTieredList>(AssetPath_4_6);
        var c = AssetDatabase.LoadAssetAtPath<WordbankTieredList>(AssetPath_7_10);

        if (a == null || b == null || c == null)
        {
            Debug.LogWarning("Grouped assets not found (or not imported yet). Running create now.");
            CreateGroupedAssets();
            return;
        }

        Selection.objects = new Object[] { a, b, c };
        EditorGUIUtility.PingObject(a);
    }

    private static WordbankTieredList LoadOrCreateGrouped(string path, string name)
    {
        var existing = AssetDatabase.LoadAssetAtPath<WordbankTieredList>(path);
        if (existing != null) return existing;

        var asset = ScriptableObject.CreateInstance<WordbankTieredList>();
        asset.name = name;

        // Seed with the same starter list as the base Ramayana list.
        // You can customize each grouped asset later in the Inspector.
        asset.easy.AddRange(new[]
        {
            "วานร", "อสุรา", "มัจฉา", "ปักษา", "คีรี", "นภา", "ราตรี", "กระบี่", "กุมภ์", "นาคา",
            "นารายณ์", "ลักษณ์", "สีดา", "บรรพต", "วายุ", "หาว", "พนา", "ชลธี", "จันทร", "อาทิตย์"
        });

        asset.medium.AddRange(new[]
        {
            "พานรินทร์", "ยักษา", "อสุรี", "วายุบุตร", "พจนารถ", "รณรงค์", "พลับพลา", "ไสยา", "ศิโรเพฐน์", "สัประยุทธ์",
            "พนาลัย", "มรคา", "ภูวไนย", "เยาวมาลย์", "โยธา", "ไยไพ", "ลีลาศ", "ศุภฤกษ์", "เหมันต์", "ภพไตร"
        });

        asset.hard.AddRange(new[]
        {
            "กัมปนาท", "ไกรสร", "มัชฌิมราตรี", "โกฏิ", "คัคนานต์", "ทศพักตร์", "ทิพยโสตนัยนา", "พรหมมาสตร์", "โตมร", "ศิลา"
        });

        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        var parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
        var name = System.IO.Path.GetFileName(path);
        if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, name);
    }

    [MenuItem("Typing Legends/Wordbanks/Assign Ramayana Tiered List to All Wordbanks (Except Wordbank)")]
    public static void AssignToAllInOpenScenes()
    {
        var asset = AssetDatabase.LoadAssetAtPath<WordbankTieredList>(AssetPath);
        if (asset == null)
        {
            Debug.LogError($"Not found: {AssetPath}. Create it first via 'Create Ramayana Tiered List'.");
            return;
        }

        int applied = 0;
        int skipped = 0;

        var wordbanks = Object.FindObjectsByType<AdaptiveWordbankAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var wb in wordbanks)
        {
            if (wb == null) continue;

            // Keep Level 1 beginner-friendly (requested): don't override the simple Wordbank
            if (wb.GetType().Name == "Wordbank")
            {
                skipped++;
                continue;
            }

            var so = new SerializedObject(wb);
            var prop = so.FindProperty("tieredList");
            if (prop == null)
            {
                Debug.LogWarning($"Could not find 'tieredList' on {wb.name} ({wb.GetType().Name}).");
                continue;
            }

            if (prop.objectReferenceValue == asset)
            {
                skipped++;
                continue;
            }

            Undo.RecordObject(wb, "Assign Ramayana Tiered List");
            prop.objectReferenceValue = asset;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(wb);
            applied++;
        }

        Debug.Log($"Assigned Ramayana tiered list to {applied} Wordbank(s). Skipped {skipped}.");
    }
}
#endif
