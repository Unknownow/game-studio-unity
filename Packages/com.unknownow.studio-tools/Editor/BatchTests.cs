using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Unknownow.StudioTools.Editor
{
    /// <summary>
    /// Runs the EditMode and PlayMode tests and writes production/qa/test-results/editmode.xml and playmode.xml.
    /// Start it without -quit: it ends the editor itself when the last run finishes.
    /// Exit 0 when every test passes, 1 on any failure, 2 on a bad or missing -testPlatform.
    /// </summary>
    public static class BatchTests
    {
        const string QueueKey = "Unknownow.StudioTools.BatchTests.Queue";
        const string CurrentKey = "Unknownow.StudioTools.BatchTests.Current";
        const string FailedKey = "Unknownow.StudioTools.BatchTests.Failed";
        const string ActiveKey = "Unknownow.StudioTools.BatchTests.Active";
        const string PassedKey = "Unknownow.StudioTools.BatchTests.Passed";
        const string FailCountKey = "Unknownow.StudioTools.BatchTests.FailCount";

        static TestRunnerApi _api;
        static bool _attached;

        /// <summary>Parses -testPlatform EditMode|PlayMode|All into the list of platforms to run.</summary>
        public static bool TryParsePlatforms(string[] args, out List<string> platforms, out string error)
        {
            platforms = new List<string>();
            error = null;
            string value = StudioCli.GetArg(args, "-testPlatform");
            switch (value)
            {
                case "EditMode":
                    platforms.Add("EditMode");
                    return true;
                case "PlayMode":
                    platforms.Add("PlayMode");
                    return true;
                case "All":
                    platforms.Add("EditMode");
                    platforms.Add("PlayMode");
                    return true;
                case null:
                    error = "-testPlatform is required: EditMode, PlayMode or All";
                    return false;
                default:
                    error = "-testPlatform must be EditMode, PlayMode or All, got: " + value;
                    return false;
            }
        }

        public static string ResultFileName(string platform)
        {
            return platform == "PlayMode" ? "playmode.xml" : "editmode.xml";
        }

        public static void Run()
        {
            if (!TryParsePlatforms(Environment.GetCommandLineArgs(), out List<string> platforms, out string error))
            {
                StudioCli.Exit(StudioCli.ExitUsage, error);
                return;
            }
            SessionState.SetString(QueueKey, string.Join(",", platforms));
            SessionState.SetString(CurrentKey, "");
            SessionState.SetBool(FailedKey, false);
            SessionState.SetInt(PassedKey, 0);
            SessionState.SetInt(FailCountKey, 0);
            SessionState.SetBool(ActiveKey, true);
            Attach();
            StartNext();
        }

        [MenuItem("Tools/Game Studio/Run All Tests")]
        static void RunFromMenu()
        {
            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _api.Execute(new ExecutionSettings(new Filter { testMode = TestMode.EditMode }, new Filter { testMode = TestMode.PlayMode }));
        }

        /// <summary>Registers the callbacks. A PlayMode run reloads the domain, so this is also called again on load.</summary>
        internal static void Attach()
        {
            if (_attached)
            {
                return;
            }
            _attached = true;
            _api = ScriptableObject.CreateInstance<TestRunnerApi>();
            _api.RegisterCallbacks(new Callbacks());
        }

        static void StartNext()
        {
            string queue = SessionState.GetString(QueueKey, "");
            if (queue.Length == 0)
            {
                bool failed = SessionState.GetBool(FailedKey, false);
                SessionState.SetBool(ActiveKey, false);
                DeleteInitTestScenes();
                // One summary line with the totals for every platform: tools that read the log look for "N passed".
                StudioCli.Exit(failed ? StudioCli.ExitFindings : StudioCli.ExitOk,
                    SessionState.GetInt(PassedKey, 0) + " passed, " + SessionState.GetInt(FailCountKey, 0) + " failed");
                return;
            }
            int comma = queue.IndexOf(',');
            string platform = comma < 0 ? queue : queue.Substring(0, comma);
            SessionState.SetString(QueueKey, comma < 0 ? "" : queue.Substring(comma + 1));
            SessionState.SetString(CurrentKey, platform);
            var mode = platform == "PlayMode" ? TestMode.PlayMode : TestMode.EditMode;
            _api.Execute(new ExecutionSettings(new Filter { testMode = mode }));
        }

        static readonly System.Text.RegularExpressions.Regex InitTestScenePattern = new System.Text.RegularExpressions.Regex(
            "^InitTestScene[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$");

        /// <summary>True for the bootstrap scenes the Test Framework leaves directly in Assets: InitTestScene&lt;guid&gt;.unity.</summary>
        public static bool IsInitTestScene(string assetPath)
        {
            return System.IO.Path.GetDirectoryName(assetPath).Replace('\\', '/') == "Assets"
                && System.IO.Path.GetExtension(assetPath) == ".unity"
                && InitTestScenePattern.IsMatch(System.IO.Path.GetFileNameWithoutExtension(assetPath));
        }

        /// <summary>The PlayMode run leaves its bootstrap scenes in Assets when the editor exits first; remove them.</summary>
        static void DeleteInitTestScenes()
        {
            foreach (string guid in AssetDatabase.FindAssets("InitTestScene t:Scene", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (IsInitTestScene(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        static void Finished(ITestResultAdaptor result)
        {
            string platform = SessionState.GetString(CurrentKey, "EditMode");
            string path = StudioCli.QaPath(StudioCli.ProjectRoot(), "test-results", ResultFileName(platform));
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
            TestRunnerApi.SaveResultToFile(result, path);
            if (result.FailCount > 0 || result.TestStatus == TestStatus.Failed)
            {
                SessionState.SetBool(FailedKey, true);
            }
            SessionState.SetInt(PassedKey, SessionState.GetInt(PassedKey, 0) + result.PassCount);
            SessionState.SetInt(FailCountKey, SessionState.GetInt(FailCountKey, 0) + result.FailCount);
            Debug.Log("[studio-tools] " + platform + ": passed=" + result.PassCount + ", failed=" + result.FailCount);
            EditorApplication.delayCall += StartNext;
        }

        sealed class Callbacks : ICallbacks
        {
            public void RunStarted(ITestAdaptor testsToRun) { }
            public void RunFinished(ITestResultAdaptor result) { Finished(result); }
            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }

        [InitializeOnLoadMethod]
        static void Resume()
        {
            if (SessionState.GetBool(ActiveKey, false))
            {
                Attach();
            }
        }
    }
}
