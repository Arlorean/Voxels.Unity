using System.Collections;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Inspector for .Vox assets
/// </summary>
[CustomEditor(typeof(DefaultAsset))]
public class VoxEditor : Editor {
    public bool testbool;

    public override void OnInspectorGUI() {
        // .vox files are imported as a DefaultAsset.
        // Need to determine that this default asset is an .vox file
        var path = AssetDatabase.GetAssetPath(target);

        if (path.EndsWith(".vox")) {
            VoxInspectorGUI(path);
        }
        else {
            base.OnInspectorGUI();
        }
    }

    void VoxInspectorGUI(string path) {
        var voxelData = VoxelImporter.ReadVoxFile(path);

        // Read only
        EditorGUILayout.Vector3Field("Size", new Vector3(voxelData.size.X, voxelData.size.Y, voxelData.size.Z));
        EditorGUILayout.IntField("Count", voxelData.Count);

        // Editable
        GUI.enabled = true;
        if (GUILayout.Button("Create Prefab")) {
            VoxAssetPostprocessor.ImportVoxFile(path);
        }
    }
}
