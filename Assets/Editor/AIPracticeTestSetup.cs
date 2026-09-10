using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public static class AIPracticeTestSetup
{
    public const string ScenePath = "Assets/Resources/Scenes/AIPracticeTest.unity";

    [MenuItem("Tools/Typing Legends/Create AI Practice Test Scene")]
    public static void CreateScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
        {
            Debug.Log("AI practice test scene already exists: " + ScenePath);
            return;
        }
        Scene previous = SceneManager.GetActiveScene();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        try
        {
            var camera = new GameObject("Main Camera").AddComponent<Camera>();
            camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.06f, 0.09f, 0.15f);
            camera.transform.position = new Vector3(0, 0, -10);
            camera.orthographic = true;
            var manager = new GameObject("AI Manager").AddComponent<AIPracticeClient>();
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var buttonObject = new GameObject("Create AI Practice Button", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
            buttonObject.transform.SetParent(canvasObject.transform, false);
            buttonObject.GetComponent<RectTransform>().sizeDelta = new Vector2(660, 130);
            buttonObject.GetComponent<UnityEngine.UI.Image>().color = new Color(0.16f, 0.36f, 0.72f);
            var button = buttonObject.GetComponent<UnityEngine.UI.Button>();
            button.targetGraphic = buttonObject.GetComponent<UnityEngine.UI.Image>();
            UnityEventTools.AddPersistentListener(button.onClick, manager.RequestPracticeWords);

            var textObject = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(20, 10);
            rect.offsetMax = new Vector2(-20, -10);
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/NotoSerifThai-VariableFont_wdth,wght SDF.asset");
            label.text = "สร้างชุดฝึกด้วย AI";
            label.fontSize = 48;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            var events = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
#if ENABLE_INPUT_SYSTEM
            events.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            events.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
#endif
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("Created AI practice test scene: " + ScenePath);
        }
        finally
        {
            if (previous.IsValid()) SceneManager.SetActiveScene(previous);
            EditorSceneManager.CloseScene(scene, true);
        }
    }
}
