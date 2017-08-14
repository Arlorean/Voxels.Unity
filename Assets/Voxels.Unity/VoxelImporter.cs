using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Voxels;

public static class VoxelImporter {
    static MeshSettings settings = new MeshSettings() {
        AmbientOcclusion = true,
    };

    public static VoxelData ReadMagicaVoxelFile(string path) {
        using (var stream = File.OpenRead(path)) {
            return MagicaVoxel.Read(stream);
        }
    }

    public static void VoxelsToUnity(GameObject go, Mesh mesh, VoxelData voxelData) {
        // Recenter the .vox model so the bottom is at the origin
        var matrix = Matrix4x4.Translate(new Vector3(-voxelData.size.X*0.5f, 0, -voxelData.size.Y*0.5f));

        // Convert a Voxel mesh to a Unity mesh
        var meshBuilder = new MeshBuilder(voxelData, settings);
        mesh.Clear();
        mesh.vertices = meshBuilder.Vertices.Select(v => matrix.MultiplyPoint(new Vector3(v.X, v.Z, v.Y))).ToArray();
        mesh.normals = meshBuilder.Normals.Select(n => matrix.MultiplyVector(new Vector3(n.X, n.Z, n.Y))).ToArray();
        mesh.colors32 = meshBuilder.Colors.Select(c => new Color32(c.R, c.G, c.B, c.A)).ToArray();
        mesh.triangles = meshBuilder.Faces.Select(f => (int)f).ToArray();

        // Set mesh filter mesh
        var meshFilter = go.GetComponent<MeshFilter>();
        if (meshFilter == null) {
            meshFilter = go.AddComponent<MeshFilter>();
        }
        meshFilter.sharedMesh = mesh;

        // Set mesh render material
        var meshRenderer = go.GetComponent<MeshRenderer>();
        if (meshRenderer == null) {
            meshRenderer = go.AddComponent<MeshRenderer>();
        }
        meshRenderer.sharedMaterial = Resources.Load<Material>("Standard");
    }
}
