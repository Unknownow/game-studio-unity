using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unknownow.StudioTools.Editor
{
    /// <summary>Shared helpers for the batch methods: argument lookup, project paths and exit codes.</summary>
    public static class StudioCli
    {
        public const int ExitOk = 0;
        public const int ExitFindings = 1;
        public const int ExitUsage = 2;

        /// <summary>The project folder: the parent of Assets.</summary>
        public static string ProjectRoot()
        {
            return Directory.GetParent(Application.dataPath).FullName;
        }

        /// <summary>The value after <paramref name="name"/> in <paramref name="args"/>, or null when it is missing.</summary>
        public static string GetArg(string[] args, string name)
        {
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == name)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        public static string QaPath(string projectRoot, params string[] parts)
        {
            string path = Path.Combine(projectRoot, "production", "qa");
            foreach (string part in parts)
            {
                path = Path.Combine(path, part);
            }
            return path;
        }

        public static void WriteText(string path, string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, text);
        }

        public static string Timestamp()
        {
            return DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        }

        /// <summary>Logs the reason and ends the editor with the exit code.</summary>
        public static void Exit(int code, string message)
        {
            if (code == ExitOk)
            {
                Debug.Log("[studio-tools] " + message);
            }
            else
            {
                Debug.LogError("[studio-tools] " + message);
            }
            EditorApplication.Exit(code);
        }
    }
}
