using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unknownow.StudioTools.Editor
{
    /// <summary>
    /// Scans every scene and prefab under Assets/ for missing scripts and dangling object references and writes
    /// production/qa/audit/missing-references.json. Exit 0 when clean, 1 when anything is found.
    /// </summary>
    public static class MissingReferenceAudit
    {
        [Serializable]
        public class MissingScript
        {
            public string asset;
            public string @object;
        }

        [Serializable]
        public class MissingReference
        {
            public string asset;
            public string @object;
            public string component;
            public string property;
        }

        [Serializable]
        public class Report
        {
            public string generated;
            public List<MissingScript> missingScripts = new List<MissingScript>();
            public List<MissingReference> missingReferences = new List<MissingReference>();

            public int Total { get { return missingScripts.Count + missingReferences.Count; } }
        }

        public static string ToJson(Report report)
        {
            return JsonUtility.ToJson(report, true);
        }

        /// <summary>Audits one GameObject hierarchy. <paramref name="asset"/> is the asset path it came from.</summary>
        public static void AuditHierarchy(GameObject root, string asset, Report report)
        {
            foreach (Component component in root.GetComponentsInChildren<Component>(true))
            {
                if (component == null)
                {
                    continue;
                }
                var serialized = new SerializedObject(component);
                SerializedProperty property = serialized.GetIterator();
                while (property.NextVisible(true))
                {
                    if (property.propertyType == SerializedPropertyType.ObjectReference
                        && property.objectReferenceValue == null
                        && property.objectReferenceInstanceIDValue != 0)
                    {
                        report.missingReferences.Add(new MissingReference
                        {
                            asset = asset,
                            @object = PathOf(component.transform),
                            component = component.GetType().Name,
                            property = property.propertyPath
                        });
                    }
                }
            }
            foreach (Transform transform in root.GetComponentsInChildren<Transform>(true))
            {
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                for (int i = 0; i < count; i++)
                {
                    report.missingScripts.Add(new MissingScript { asset = asset, @object = PathOf(transform) });
                }
            }
        }

        static string PathOf(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        /// <summary>Audits the given assets (scene and prefab paths). Opens each scene, so the caller restores the open scenes.</summary>
        public static Report AuditAssets(IEnumerable<string> assetPaths)
        {
            var report = new Report { generated = StudioCli.Timestamp() };
            foreach (string path in assetPaths)
            {
                if (path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    GameObject root = PrefabUtility.LoadPrefabContents(path);
                    try
                    {
                        AuditHierarchy(root, path, report);
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
                else if (path.EndsWith(".unity", StringComparison.OrdinalIgnoreCase))
                {
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                    foreach (GameObject root in scene.GetRootGameObjects())
                    {
                        AuditHierarchy(root, path, report);
                    }
                }
            }
            return report;
        }

        /// <summary>The scene and prefab paths under Assets/.</summary>
        public static List<string> FindAssets()
        {
            var paths = new List<string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Scene t:Prefab", new[] { "Assets" }))
            {
                paths.Add(AssetDatabase.GUIDToAssetPath(guid));
            }
            paths.Sort(StringComparer.Ordinal);
            return paths;
        }

        public static string WriteReport(Report report)
        {
            string path = StudioCli.QaPath(StudioCli.ProjectRoot(), "audit", "missing-references.json");
            StudioCli.WriteText(path, ToJson(report) + "\n");
            return path;
        }

        public static void RunBatch()
        {
            Report report = AuditAssets(FindAssets());
            string path = WriteReport(report);
            StudioCli.Exit(report.Total == 0 ? StudioCli.ExitOk : StudioCli.ExitFindings,
                report.missingScripts.Count + " missing scripts, " + report.missingReferences.Count + " missing references; report: " + path);
        }

        [MenuItem("Tools/Game Studio/Missing Reference Audit")]
        static void RunFromMenu()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }
            SceneSetup[] setup = EditorSceneManager.GetSceneManagerSetup();
            Report report;
            try
            {
                report = AuditAssets(FindAssets());
            }
            finally
            {
                if (setup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(setup);
                }
            }
            string path = WriteReport(report);
            EditorUtility.DisplayDialog("Missing Reference Audit",
                report.missingScripts.Count + " missing scripts, " + report.missingReferences.Count + " missing references.\nReport: " + path, "OK");
        }
    }
}
