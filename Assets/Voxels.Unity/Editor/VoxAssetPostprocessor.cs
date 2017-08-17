using System.IO;
using UnityEditor;
using UnityEngine;
using Voxels;

public class VoxAssetPostprocessor : AssetPostprocessor {

    public static void ImportVoxFile(string path) {
        var dir = Path.GetDirectoryName(path);
        var name = Path.GetFileNameWithoutExtension(path);

        var meshDir = dir + "/Meshes/";
        var prefabDir = dir + "/Prefabs/";
        Directory.CreateDirectory(meshDir);
        Directory.CreateDirectory(prefabDir);

        var prefabPath = prefabDir+name + ".prefab";
        var meshPath = meshDir+name + ".asset";
        var shadowPath = meshDir+name + ".shadow.asset";

        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if (mesh == null) {
            mesh = new Mesh();
            mesh.name = name;
            AssetDatabase.CreateAsset(mesh, meshPath);
        }

        var shadow = AssetDatabase.LoadAssetAtPath<Mesh>(shadowPath);
        if (shadow == null) {
            shadow = new Mesh();
            shadow.name = name+".shadow";
            AssetDatabase.CreateAsset(shadow, shadowPath);
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null || !prefab.transform.Find("shadow")) {
            var gameObject = new GameObject(name);
            var prefabShadow = new GameObject("shadow");
            prefabShadow.transform.parent = gameObject.transform;
            prefab = PrefabUtility.CreatePrefab(prefabPath, gameObject);
            GameObject.DestroyImmediate(gameObject);
            GameObject.DestroyImmediate(prefabShadow);
        }

        var voxelData = VoxelImporter.ReadVoxFile(path);
        VoxelImporter.VoxelsToUnity(prefab, mesh, voxelData, new MeshSettings { FrontFaces = true, BackFaces = true });

        var shadowObject = prefab.transform.Find("shadow").gameObject;
        VoxelImporter.VoxelsToUnity(shadowObject, shadow, voxelData, new MeshSettings { FloorShadow = true });

        EditorUtility.SetDirty(mesh);
        EditorUtility.SetDirty(shadow);
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
                ImportVoxFile(path);
            }

        }
        AssetDatabase.SaveAssets();
    }
}
