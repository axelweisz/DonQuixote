using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace DonQuixote.Editor
{
    // Run via DonQuixote menu in order: step 1 then step 2.
    // Step 1 writes assets and project settings; step 2 patches the open scene's camera.
    // Safe to re-run: step 1 skips asset creation if files already exist.
    public static class SetupUrp2D
    {
        private const string PcRpPath       = "Assets/Settings/PC_RPAsset.asset";
        private const string MobileRpPath   = "Assets/Settings/Mobile_RPAsset.asset";
        private const string PcRendererPath = "Assets/Settings/PC_Renderer2D.asset";
        private const string MobRendererPath= "Assets/Settings/Mobile_Renderer2D.asset";

        // ── Step 1 ──────────────────────────────────────────────────────────────

        [MenuItem("DonQuixote/1 - Switch to 2D URP Renderer")]
        public static void SwitchToRenderer2D()
        {
            bool changed = false;
            changed |= ConfigureRenderer2D(PcRpPath,     PcRendererPath,  "PC_Renderer2D");
            changed |= ConfigureRenderer2D(MobileRpPath, MobRendererPath, "Mobile_Renderer2D");

            if (changed)
            {
                SetTransparencySortMode();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[DonQuixote] Step 1 done — Renderer2D configured. " +
                          "Now run DonQuixote > 2 - Setup Camera.");
            }
            else
            {
                Debug.Log("[DonQuixote] Step 1: Renderer2D assets already in place. Nothing changed.");
            }
        }

        // ── Step 2 ──────────────────────────────────────────────────────────────

        [MenuItem("DonQuixote/2 - Setup Camera and Scene")]
        public static void SetupCamera()
        {
            var cameras = Object.FindObjectsByType<Camera>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);

            bool found = false;
            foreach (var cam in cameras)
            {
                if (!cam.CompareTag("MainCamera")) continue;
                found = true;

                Undo.RecordObject(cam, "Setup 2D Camera");
                cam.orthographic       = true;
                cam.orthographicSize   = 5f;
                cam.nearClipPlane      = -1f;
                cam.farClipPlane       = 1000f;
                cam.clearFlags         = CameraClearFlags.SolidColor;
                cam.backgroundColor    = Color.black;
                EditorUtility.SetDirty(cam);

                Debug.Log($"[DonQuixote] Camera '{cam.name}' → orthographic, size 5.");
            }

            if (!found)
                Debug.LogWarning("[DonQuixote] No MainCamera found in the open scene.");

            // Remove Directional Light — not needed for 2D URP
            var lights = Object.FindObjectsByType<Light>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    Undo.DestroyObjectImmediate(light.gameObject);
                    Debug.Log("[DonQuixote] Directional Light removed (not used in 2D URP).");
                    break;
                }
            }

            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[DonQuixote] Step 2 done — scene saved.");
        }

        // ── Helpers ─────────────────────────────────────────────────────────────

        private static bool ConfigureRenderer2D(string rpAssetPath, string renderer2DPath, string assetName)
        {
            var rpAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(rpAssetPath);
            if (rpAsset == null)
            {
                Debug.LogWarning($"[DonQuixote] Could not load {rpAssetPath} — skipping.");
                return false;
            }

            // Create Renderer2D data asset if it doesn't exist yet
            Renderer2DData renderer2D;
            if (File.Exists(Path.Combine(Application.dataPath, "..", renderer2DPath)))
            {
                renderer2D = AssetDatabase.LoadAssetAtPath<Renderer2DData>(renderer2DPath);
                Debug.Log($"[DonQuixote] {renderer2DPath} already exists — reusing.");
            }
            else
            {
                renderer2D = ScriptableObject.CreateInstance<Renderer2DData>();
                renderer2D.name = assetName;
                AssetDatabase.CreateAsset(renderer2D, renderer2DPath);
            }

            // Patch RPAsset via SerializedObject to avoid reflection hacks
            var so = new SerializedObject(rpAsset);
            so.FindProperty("m_RendererType").intValue = 2;          // 2 = Renderer2D

            var dataList = so.FindProperty("m_RendererDataList");
            dataList.ClearArray();
            dataList.InsertArrayElementAtIndex(0);
            dataList.GetArrayElementAtIndex(0).objectReferenceValue = renderer2D;

            so.FindProperty("m_DefaultRendererIndex").intValue = 0;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(rpAsset);

            Debug.Log($"[DonQuixote] {rpAssetPath} → Renderer2D assigned.");
            return true;
        }

        private static void SetTransparencySortMode()
        {
            // Orthographic sort mode is correct for a 2D side-scroller.
            // This is stored in GraphicsSettings via SerializedObject.
            var graphicsSettings = GraphicsSettings.GetGraphicsSettings();
            var so = new SerializedObject(graphicsSettings);
            // 0 = Default (respects camera projection), 3 = Orthographic
            // Default is fine — the orthographic camera drives the correct sort automatically.
            var sortMode = so.FindProperty("m_TransparencySortMode");
            if (sortMode != null)
            {
                sortMode.intValue = 3; // Orthographic
                so.ApplyModifiedProperties();
                Debug.Log("[DonQuixote] GraphicsSettings transparency sort mode → Orthographic.");
            }
        }
    }
}
