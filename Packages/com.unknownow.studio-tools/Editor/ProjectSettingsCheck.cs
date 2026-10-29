using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Unknownow.StudioTools.Editor
{
    /// <summary>
    /// Checks the project settings the studio relies on and writes production/qa/audit/project-settings.json.
    /// Exit 0 when every check passes, 1 otherwise.
    /// </summary>
    public static class ProjectSettingsCheck
    {
        [Serializable]
        public class Check
        {
            public string id;
            public bool pass;
            public string actual;
        }

        [Serializable]
        public class Report
        {
            public string generated;
            public List<Check> checks = new List<Check>();

            public bool AllPass
            {
                get
                {
                    foreach (Check check in checks)
                    {
                        if (!check.pass)
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        /// <summary>Reads activeInputHandler from the text of ProjectSettings/ProjectSettings.asset. Returns -1 when it is absent.</summary>
        public static int ParseActiveInputHandler(string settingsText)
        {
            Match match = Regex.Match(settingsText ?? "", @"^\s*activeInputHandler:\s*(\d+)", RegexOptions.Multiline);
            return match.Success ? int.Parse(match.Groups[1].Value) : -1;
        }

        /// <summary>Builds the report from the three settings. Input handler 1 (new) and 2 (both) pass; 0 (old) fails.</summary>
        public static Report Evaluate(SerializationMode serialization, string versionControlMode, int activeInputHandler)
        {
            var report = new Report { generated = StudioCli.Timestamp() };
            report.checks.Add(new Check { id = "force-text", pass = serialization == SerializationMode.ForceText, actual = serialization.ToString() });
            report.checks.Add(new Check { id = "visible-meta", pass = versionControlMode == "Visible Meta Files", actual = versionControlMode ?? "" });
            report.checks.Add(new Check { id = "input-system", pass = activeInputHandler == 1 || activeInputHandler == 2, actual = activeInputHandler.ToString() });
            return report;
        }

        public static string ToJson(Report report)
        {
            return JsonUtility.ToJson(report, true);
        }

        public static Report EvaluateProject()
        {
            string settingsPath = Path.Combine(StudioCli.ProjectRoot(), "ProjectSettings", "ProjectSettings.asset");
            int handler = File.Exists(settingsPath) ? ParseActiveInputHandler(File.ReadAllText(settingsPath)) : -1;
            return Evaluate(EditorSettings.serializationMode, VersionControlSettings.mode, handler);
        }

        static string Write(Report report)
        {
            string path = StudioCli.QaPath(StudioCli.ProjectRoot(), "audit", "project-settings.json");
            StudioCli.WriteText(path, ToJson(report) + "\n");
            return path;
        }

        static string Summary(Report report)
        {
            var failed = new List<string>();
            foreach (Check check in report.checks)
            {
                if (!check.pass)
                {
                    failed.Add(check.id + " (" + check.actual + ")");
                }
            }
            return failed.Count == 0 ? "all checks pass" : "failed: " + string.Join(", ", failed);
        }

        public static void RunBatch()
        {
            Report report = EvaluateProject();
            string path = Write(report);
            StudioCli.Exit(report.AllPass ? StudioCli.ExitOk : StudioCli.ExitFindings, Summary(report) + "; report: " + path);
        }

        [MenuItem("Tools/Game Studio/Project Settings Check")]
        static void RunFromMenu()
        {
            Report report = EvaluateProject();
            string path = Write(report);
            EditorUtility.DisplayDialog("Project Settings Check", Summary(report) + "\nReport: " + path, "OK");
        }
    }
}
