using System.IO;
using UnityEditor;
using UnityEngine;

public class VoxAssetPostprocessor : AssetPostprocessor {

    public static void ImportMagicaVoxelFile(string path) {
        var prefabPath = Path.ChangeExtension(path, ".prefab");
        var meshPath = Path.ChangeExtension(path, ".asset");
        var name = Path.GetFileNameWithoutExtension(path);

        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null) {
            mesh = new Mesh();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, meshPath);
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) {
            var gameObject = new GameObject(name);
            prefab = PrefabUtility.CreatePrefab(prefabPath, gameObject);
            GameObject.DestroyImmediate(gameObject);
        }

        var voxelData = VoxelImporter.ReadMagicaVoxelFile(path);
        VoxelImporter.VoxelsToUnity(prefab, mesh, voxelData);

        EditorUtility.SetDirty(mesh);
        EditorUtility.SetDirty(prefab);
    }

    static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets) {
            // create an asset using that new import
            if (Path.GetExtension(path) == ".vox") {
                ImportMagicaVoxelFile(path);
            }

        }
        AssetDatabase.SaveAssets();
    }
}
