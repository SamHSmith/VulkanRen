using System.Numerics;
using System;
using Vulkan;
using static Vulkan.VulkanNative;
using System.Collections.Generic;

namespace Fabricor.VulkanRendering.VoxelRenderer
{
    public static class VoxelMeshFactory
    {

        public static VoxelMesh GenerateMesh(VkDevice device, VkPhysicalDevice physicalDevice)
        {
            List<VoxelVertex> vertices=new List<VoxelVertex>(); List<ushort> indicies=new List<ushort>();

            Face f = new Face();
            f.right=Vector3.UnitX;
            f.up=Vector3.UnitY;
            f.Generate(ref vertices,ref indicies);

            f.position=-(Vector3.UnitX*0.5f)+(Vector3.UnitZ*0.4f);
            f.textureID=1;
            f.Generate(ref vertices,ref indicies);

            return new VoxelMesh(device,physicalDevice,vertices.ToArray(),indicies.ToArray());
        }
    }

    struct Face
    {
        public Vector3 position;//Bottom left of face
        public Vector3 right, up;
        public Vector3 normal;
        public uint textureID;

        public void Generate(ref List<VoxelVertex> vertices, ref List<ushort> indicies)
        {
            VoxelVertex[] verts = new VoxelVertex[4];
            for (int i = 0; i < verts.Length; i++)
            {
                verts[i].normal = normal;
                verts[i].textureId = textureID;
            }

            verts[0].position = position;
            verts[0].texcoords = Vector2.Zero;

            verts[1].position = position + up;
            verts[1].texcoords = new Vector2(0, up.Length());

            verts[2].position = position + up + right;
            verts[2].texcoords = new Vector2(right.Length(), up.Length());

            verts[3].position = position + right;
            verts[3].texcoords = new Vector2(right.Length(), 0);

            ushort o = (ushort)vertices.Count;//First index of this quad
            vertices.AddRange(verts);

            ushort[] inds = new ushort[] { 0, 1, 2, 0, 2, 3 };
            for (int i = 0; i < inds.Length; i++)
            {
                inds[i] += o;
            }
            indicies.AddRange(inds);
        }
    }
}