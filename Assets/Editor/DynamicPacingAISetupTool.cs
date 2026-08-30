using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor Tool: Auto-setup DynamicPacingAI on all Level scenes
/// 
/// วิธีใช้:
/// 1. ไปที่ Tools > Typing Legends > Setup DynamicPacingAI
/// 2. คลิก "Setup Current Scene" หรือ "Setup All Levels"
/// 
/// มันจะทำให้:
/// - หา GameObject ที่มี Typer component
/// - เพิ่ม DynamicPacingAI component ถ้ายังไม่มี
/// - Auto-assign references
/// - ตั้งค่า default parameters
/// </summary>
public class DynamicPacingAISetupTool : EditorWindow
{
    [MenuItem("Tools/Typing Legends/Setup DynamicPacingAI/Setup Current Scene")]
    public static void SetupCurrentScene()
    {
        // หา Typer ใน scene ปัจจุบัน
        Typer typer = Object.FindFirstObjectByType<Typer>();
        if (typer == null)
        {
            EditorUtility.DisplayDialog("Error", "ไม่เจอ Typer component ใน scene ปัจจุบัน", "OK");
            return;
        }

        SetupDynamicPacingAI(typer.gameObject);
        EditorUtility.DisplayDialog("Success", $"Setup DynamicPacingAI บน {typer.gameObject.name}", "OK");
    }

    [MenuItem("Tools/Typing Legends/Setup DynamicPacingAI/Setup All Level Scenes")]
    public static void SetupAllLevelScenes()
    {
        EditorUtility.DisplayDialog("Warning", "This feature has been disabled to prevent scene corruption.\n\nPlease setup DynamicPacingAI manually:\n1. Open each level scene\n2. Use 'Setup Current Scene' option\n3. Save the scene", "OK");
        return;
        
        /*
        // DISABLED - Was corrupting scenes. Do not use without proper scene serialization handling.
        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Resources/Scenes" });
        int setupCount = 0;

        foreach (string guid in sceneGuids)
        {
            string scenePath = AssetDatabase.GUIDToAssetPath(guid);
            
            // Skip non-level scenes
            if (scenePath.Contains("MainMenu") || scenePath.Contains("Options") || 
                scenePath.Contains("LevelSelection") || scenePath.Contains("Music"))
                continue;

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            
            Typer typer = Object.FindFirstObjectByType<Typer>();
            if (typer != null)
            {
                SetupDynamicPacingAI(typer.gameObject);
                setupCount++;
                
                // Save scene
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }
        }

        EditorUtility.DisplayDialog("Success", $"Setup complete! ตั้งค่า DynamicPacingAI บน {setupCount} scenes", "OK");
        */
    }

    private static void SetupDynamicPacingAI(GameObject targetGameObject)
    {
        // ตรวจเช็ค: มี DynamicPacingAI อยู่แล้วหรือไม่
        DynamicPacingAI existingAI = targetGameObject.GetComponent<DynamicPacingAI>();
        if (existingAI != null)
        {
            Debug.Log($"DynamicPacingAI already exists on {targetGameObject.name}");
            return;
        }

        // เพิ่ม DynamicPacingAI component
        DynamicPacingAI pacingAI = targetGameObject.AddComponent<DynamicPacingAI>();
        Debug.Log($"Added DynamicPacingAI to {targetGameObject.name}");

        // ตั้งค่า default parameters
        pacingAI.pressureWpmThreshold = 30f;
        pacingAI.pressureAccuracyThreshold = 0.85f;
        pacingAI.recoveryMistakeThreshold = 0.3f;
        pacingAI.burstRequiredStreak = 3;
        
        pacingAI.timerReducePercentPressure = 0.2f;
        pacingAI.timerIncreasePercentRecovery = 0.25f;
        
        // Editor reference assignment
        Typer typer = targetGameObject.GetComponent<Typer>();
        Typer typerInScene = Object.FindFirstObjectByType<Typer>();
        if (typer != null)
        {
            // Reflect to set private field
            var field = typeof(DynamicPacingAI).GetField("typer", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
                field.SetValue(pacingAI, typer);
        }

        Debug.Log($"DynamicPacingAI setup complete with default parameters");
        EditorUtility.SetDirty(targetGameObject);
    }
}
