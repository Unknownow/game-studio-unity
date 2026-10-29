using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Unknownow.StudioTools.Editor;

namespace Unknownow.StudioTools.Tests
{
    public class MissingReferenceAuditTests
    {
        const string TempFolder = "Assets/_StudioToolsTestTemp";

        const string PrefabHeader = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n";

        const string GameObjectAndTransform =
            "--- !u!1 &1001\nGameObject:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  serializedVersion: 6\n  m_Component:\n  - component: {fileID: 1002}\n  - component: {fileID: 1003}\n  m_Layer: 0\n  m_Name: Probe\n  m_TagString: Untagged\n  m_Icon: {fileID: 0}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n" +
            "--- !u!4 &1002\nTransform:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 1001}\n  serializedVersion: 2\n  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}\n  m_LocalPosition: {x: 0, y: 0, z: 0}\n  m_LocalScale: {x: 1, y: 1, z: 1}\n  m_ConstrainProxyScale: 0\n  m_Children: []\n  m_Father: {fileID: 0}\n  m_LocalEulerAnglesHint: {x: 0, y: 0, z: 0}\n";

        // A component whose script does not exist.
        const string MissingScriptComponent =
            "--- !u!114 &1003\nMonoBehaviour:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 1001}\n  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {fileID: 11500000, guid: 0123456789abcdef0123456789abcdef, type: 3}\n  m_Name: \n  m_EditorClassIdentifier: \n";

        // A MeshFilter whose mesh points to an asset that does not exist.
        const string DanglingMeshComponent =
            "--- !u!33 &1003\nMeshFilter:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 1001}\n  m_Mesh: {fileID: 4300000, guid: 0123456789abcdef0123456789abcdef, type: 3}\n";

        // A MeshFilter with no mesh set: an empty reference, not a dangling one.
        const string EmptyMeshComponent =
            "--- !u!33 &1003\nMeshFilter:\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_GameObject: {fileID: 1001}\n  m_Mesh: {fileID: 0}\n";

        [SetUp]
        public void SetUp()
        {
            Directory.CreateDirectory(TempFolder);
        }

        [TearDown]
        public void TearDown()
        {
            AssetDatabase.DeleteAsset(TempFolder);
            AssetDatabase.Refresh();
        }

        static string WritePrefab(string name, string component)
        {
            string path = TempFolder + "/" + name + ".prefab";
            File.WriteAllText(path, PrefabHeader + GameObjectAndTransform + component);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            return path;
        }

        [Test]
        public void Audit_ReportsAMissingScript()
        {
            string path = WritePrefab("MissingScript", MissingScriptComponent);
            var report = MissingReferenceAudit.AuditAssets(new[] { path });
            Assert.AreEqual(1, report.missingScripts.Count);
            Assert.AreEqual(path, report.missingScripts[0].asset);
            Assert.AreEqual("MissingScript", report.missingScripts[0].@object);
        }

        [Test]
        public void Audit_ReportsADanglingReference()
        {
            string path = WritePrefab("Dangling", DanglingMeshComponent);
            var report = MissingReferenceAudit.AuditAssets(new[] { path });
            Assert.AreEqual(0, report.missingScripts.Count);
            Assert.AreEqual(1, report.missingReferences.Count);
            Assert.AreEqual("MeshFilter", report.missingReferences[0].component);
            Assert.AreEqual("m_Mesh", report.missingReferences[0].property);
        }

        [Test]
        public void Audit_CleanPrefab_GivesEmptyLists()
        {
            string path = WritePrefab("Clean", EmptyMeshComponent);
            var report = MissingReferenceAudit.AuditAssets(new[] { path });
            Assert.AreEqual(0, report.Total);
        }

        [Test]
        public void ToJson_HasTheDocumentedKeys()
        {
            string path = WritePrefab("MissingScript2", MissingScriptComponent);
            string json = MissingReferenceAudit.ToJson(MissingReferenceAudit.AuditAssets(new[] { path }));
            StringAssert.Contains("\"generated\"", json);
            StringAssert.Contains("\"missingScripts\"", json);
            StringAssert.Contains("\"missingReferences\"", json);
            StringAssert.Contains("\"object\"", json);
        }
    }
}
