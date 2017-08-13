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
        // Rotate the .vox model so it's facing the default camera correctly
        var recenter = Matrix4x4.Translate(new Vector3(voxelData.size.X*0.5f, 0, -voxelData.size.Y*0.5f));
        var rotZ180 = new Matrix4x4() { m00 = -1, m11 = -1, m22 = 1, m33 = 1, };
        var rotX270 = new Matrix4x4() { m00 = 1, m12 = -1, m21 = 1, m33 = 1, };
        var matrix = recenter * rotZ180 * rotX270;

        // Convert a Voxel mesh to a Unity mesh
        var meshBuilder = new MeshBuilder(voxelData, settings);
        mesh.Clear();
        mesh.vertices = meshBuilder.Vertices.Select(v => matrix.MultiplyPoint(new Vector3(v.X, v.Y, v.Z))).ToArray();
        mesh.normals = meshBuilder.Normals.Select(n => matrix.MultiplyVector(new Vector3(n.X, n.Y, n.Z))).ToArray();
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
