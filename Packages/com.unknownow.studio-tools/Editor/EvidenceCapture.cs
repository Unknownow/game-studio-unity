using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unknownow.StudioTools.Editor
{
    /// <summary>
    /// Saves a screenshot of a scene as evidence for a story. It renders a camera, so it needs a GPU:
    /// never run it with -nographics.
    /// </summary>
    public static class EvidenceCapture
    {
        public const int DefaultWidth = 1920;
        public const int DefaultHeight = 1080;

        static readonly Regex SlugPattern = new Regex("^[a-z0-9][a-z0-9._-]*$");

        public static bool IsValidSlug(string slug)
        {
            return !string.IsNullOrEmpty(slug) && SlugPattern.IsMatch(slug);
        }

        /// <summary>The PNG path: production/qa/evidence/&lt;slug&gt;/&lt;slug&gt;-yyyyMMdd-HHmmss.png under the project.</summary>
        public static string OutputPath(string projectRoot, string slug, DateTime now)
        {
            string file = slug + "-" + now.ToString("yyyyMMdd-HHmmss") + ".png";
            return StudioCli.QaPath(projectRoot, "evidence", slug, file);
        }

        /// <summary>Renders the camera into a PNG file. Returns false when there is no camera.</summary>
        public static bool RenderToFile(Camera camera, int width, int height, string path)
        {
            if (camera == null)
            {
                return false;
            }
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            var target = new RenderTexture(width, height, 24);
            var previousActive = RenderTexture.active;
            var previousTarget = camera.targetTexture;
            try
            {
                camera.targetTexture = target;
                camera.Render();
                RenderTexture.active = target;
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, texture.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(texture);
            }
            return true;
        }

        static Camera FindCamera()
        {
            Camera main = Camera.main;
            return main != null ? main : UnityEngine.Object.FindFirstObjectByType<Camera>();
        }

        /// <summary>-story &lt;slug&gt; -scene &lt;path&gt; [-width n] [-height n]. Exit 0 captured, 2 bad argument or no camera.</summary>
        public static void CaptureBatch()
        {
            string[] args = Environment.GetCommandLineArgs();
            string slug = StudioCli.GetArg(args, "-story");
            string scene = StudioCli.GetArg(args, "-scene");
            if (!IsValidSlug(slug))
            {
                StudioCli.Exit(StudioCli.ExitUsage, "-story needs a slug matching ^[a-z0-9][a-z0-9._-]*$, got: " + (slug ?? "(missing)"));
                return;
            }
            if (string.IsNullOrEmpty(scene) || !File.Exists(Path.Combine(StudioCli.ProjectRoot(), scene)))
            {
                StudioCli.Exit(StudioCli.ExitUsage, "-scene needs a scene path inside the project, got: " + (scene ?? "(missing)"));
                return;
            }
            if (!TryParseSize(args, "-width", DefaultWidth, out int width) || !TryParseSize(args, "-height", DefaultHeight, out int height))
            {
                StudioCli.Exit(StudioCli.ExitUsage, "-width and -height must be whole numbers above 0");
                return;
            }
            EditorSceneManager.OpenScene(scene, OpenSceneMode.Single);
            string path = OutputPath(StudioCli.ProjectRoot(), slug, DateTime.Now);
            if (!RenderToFile(FindCamera(), width, height, path))
            {
                StudioCli.Exit(StudioCli.ExitUsage, "no camera in scene " + scene);
                return;
            }
            StudioCli.Exit(StudioCli.ExitOk, "evidence saved to " + path);
        }

        public static bool TryParseSize(string[] args, string name, int fallback, out int value)
        {
            string text = StudioCli.GetArg(args, name);
            if (text == null)
            {
                value = fallback;
                return true;
            }
            return int.TryParse(text, out value) && value > 0;
        }

        [MenuItem("Tools/Game Studio/Capture Evidence")]
        static void CaptureFromMenu()
        {
            string slug = Regex.Replace(EditorSceneManager.GetActiveScene().name.ToLowerInvariant(), "[^a-z0-9._-]+", "-").Trim('-');
            if (!IsValidSlug(slug))
            {
                slug = "scene";
            }
            string path = OutputPath(StudioCli.ProjectRoot(), slug, DateTime.Now);
            if (EditorApplication.isPlaying)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                ScreenCapture.CaptureScreenshot(path);
                EditorUtility.DisplayDialog("Capture Evidence", "Saving the Game view to:\n" + path, "OK");
                return;
            }
            bool ok = RenderToFile(FindCamera(), DefaultWidth, DefaultHeight, path);
            EditorUtility.DisplayDialog("Capture Evidence", ok ? "Saved:\n" + path : "No camera in the open scene.", "OK");
        }
    }
}
