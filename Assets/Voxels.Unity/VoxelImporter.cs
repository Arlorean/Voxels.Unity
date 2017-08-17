using System;
using System.IO;
using System.Linq;
using UnityEngine;
using Voxels;

using Color = Voxels.Color;

public static class VoxelImporter {
    public static VoxelData ReadVoxFile(string path) {
        using (var stream = File.OpenRead(path)) {
            return VoxFile.Read(stream);
        }
    }

    public static void VoxelsToUnity(GameObject go, Mesh mesh, VoxelData voxelData, MeshSettings settings) {
        // Recenter the .vox model so the bottom is at the origin
        var matrix = Matrix4x4.Translate(new Vector3(-voxelData.size.X*0.5f, 0, -voxelData.size.Y*0.5f));

        // Convert a Voxel mesh to a Unity mesh
        var meshBuilder = new MeshBuilder(voxelData, settings);
        mesh.Clear();
        mesh.vertices = meshBuilder.Vertices.Select(v => matrix.MultiplyPoint(new Vector3(v.X, v.Z, v.Y))).ToArray();
        mesh.normals = meshBuilder.Normals.Select(n => matrix.MultiplyVector(new Vector3(n.X, n.Z, n.Y))).ToArray();
        mesh.triangles = meshBuilder.Faces.Select(f => (int)f).ToArray();

        // Combine occlusion with vertex colors
        var colors = meshBuilder.Colors;
        var occlusion = meshBuilder.Occlusion;
        mesh.colors32 = Enumerable.Range(0, meshBuilder.Colors.Length)
            .Select(i => AmbientOcclusion.CombineColorOcclusion(colors[i], occlusion[i]))
            .Select(c => new Color32(c.R, c.G, c.B, c.A)).ToArray();

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
        meshRenderer.sharedMaterial = Resources.Load<Material>(settings.FloorShadow ? "StandardTransparent" : "Standard");
    }
}
