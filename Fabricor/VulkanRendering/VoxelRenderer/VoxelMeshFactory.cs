using System.Numerics;
using System.Linq;
using Vulkan;
using static Vulkan.VulkanNative;
using System.Collections.Generic;

namespace Fabricor.VulkanRendering.VoxelRenderer
{
    public static class VoxelMeshFactory
    {

        public static VoxelMesh GenerateMesh(VkDevice device, VkPhysicalDevice physicalDevice)
        {
            List<VoxelVertex> vertices = new List<VoxelVertex>(); List<ushort> indicies = new List<ushort>();
            List<Face> faces = new List<Face>();

            faces.AddRange(GenerateFaces(new Vector3(0, 0, 0), 0));
            faces.AddRange(GenerateFaces(new Vector3(1, 0, 0), 0));
            faces.AddRange(GenerateFaces(new Vector3(1, 1, 0), 0));
            faces.AddRange(GenerateFaces(new Vector3(2, 0, 0), 0));
            faces.AddRange(GenerateFaces(new Vector3(2, 1, 0), 0));

            LoopFaces(ref faces, Vector3.UnitX);
            LoopFaces(ref faces, Vector3.UnitY);
            LoopFaces(ref faces, Vector3.UnitZ);

            for (int i = 0; i < faces.Count; i++)
            {
                faces[i].Generate(ref vertices, ref indicies);
            }
            return new VoxelMesh(device, physicalDevice, vertices.ToArray(), indicies.ToArray());
        }

        private static void LoopFaces(ref List<Face> outFaces, Vector3 axis)
        {
            List<Face> faces = new List<Face>(outFaces.Where((f) => Vector3.Abs(f.normal) == axis));

            for (int i = 0; i < faces.Count; i++)//remove inner faces
            {
                for (int j = i + 1; j < faces.Count; j++)
                {
                    if (faces[i].CenterPosition == faces[j].CenterPosition)
                    {
                        faces[i].shouldBeRemoved = true;
                        faces[j].shouldBeRemoved = true;
                    }
                }
            }
            outFaces.RemoveAll((f) => f.shouldBeRemoved);
            faces = new List<Face>(outFaces.Where((f) => Vector3.Abs(f.normal) == axis));
            int maxLayer = 0;
            foreach (var f in faces)
            {
                if (f.Layer > maxLayer)
                    maxLayer = f.Layer;
            }
            for (int i = 0; i < maxLayer+1; i++)
            {
                List<Face> mergeFaces = new List<Face>(faces.Where((f) => i == (int)(f.Layer)));
                if (axis == Vector3.UnitX)
                {
                    MergeFaces(mergeFaces, Vector3.UnitY, Vector3.UnitZ);
                    MergeFaces(mergeFaces, Vector3.UnitZ, Vector3.UnitY);
                }
                else if (axis == Vector3.UnitY)
                {
                    MergeFaces(mergeFaces, Vector3.UnitX, Vector3.UnitZ);
                    MergeFaces(mergeFaces, Vector3.UnitZ, Vector3.UnitX);
                }
                else if (axis == Vector3.UnitZ)
                {
                    MergeFaces(mergeFaces, Vector3.UnitX, Vector3.UnitY);
                    MergeFaces(mergeFaces, Vector3.UnitY, Vector3.UnitX);
                }
            }
            outFaces.RemoveAll((f) => f.shouldBeRemoved);
        }

        private static void MergeFaces(List<Face> faces, Vector3 compareAxis, Vector3 otherAxis)
        {
            for (int i = 0; i < faces.Count; i++)
            {
                for (int j = 0; j < faces.Count; j++)//inefficient loop but it makes stuff easier
                {
                    if (i == j)//This is self explanitory
                        continue;

                    if (faces[i].textureID != faces[j].textureID)
                        continue;//The faces have the same texture

                    if (faces[i].shouldBeRemoved || faces[j].shouldBeRemoved)
                        continue;//None of the faces are queued for destruction

                    if ((faces[i].position * otherAxis).LengthSquared() != (faces[j].position * otherAxis).LengthSquared())
                        continue;//If not aligned on other axis, skip.

                    if (((faces[i].position + faces[i].right + faces[j].up) * compareAxis).LengthSquared() != (faces[j].position * compareAxis).LengthSquared())
                        continue;//is face j next to face i?

                    if ((faces[i].right * compareAxis).LengthSquared() > 0)
                    {//Comparision is on the right axis
                        if (faces[i].up.LengthSquared() != faces[j].up.LengthSquared())
                            continue;//Are the faces the same size?

                        faces[i].right += faces[j].right;
                        faces[j].shouldBeRemoved = true;
                    }
                    else
                    {//Comparision is on the up axis
                        if (faces[i].right.LengthSquared() != faces[j].right.LengthSquared())
                            continue;//Are the faces the same size?

                        faces[i].up += faces[j].up;
                        faces[j].shouldBeRemoved = true;
                    }
                }
            }
        }

        private static Face[] GenerateFaces(Vector3 position, uint blockID)
        {
            Face[] faces = new Face[6];

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = new Face();
                faces[i].textureID = blockID;//TODO add custom textures for differnet sides with a block lookup
            }

            faces[0].position = position;
            faces[0].normal = -Vector3.UnitZ;
            faces[0].right = Vector3.UnitX;
            faces[0].up = Vector3.UnitY;

            faces[1].position = position + Vector3.UnitZ;
            faces[1].normal = Vector3.UnitX;
            faces[1].right = -Vector3.UnitZ;
            faces[1].up = Vector3.UnitY;

            faces[2].position = position + Vector3.UnitZ + Vector3.UnitX;
            faces[2].normal = Vector3.UnitZ;
            faces[2].right = -Vector3.UnitX;
            faces[2].up = Vector3.UnitY;

            faces[3].position = position + Vector3.UnitX;
            faces[3].normal = Vector3.UnitX;
            faces[3].right = Vector3.UnitZ;
            faces[3].up = Vector3.UnitY;

            faces[4].position = position + Vector3.UnitY;
            faces[4].normal = Vector3.UnitY;
            faces[4].right = Vector3.UnitX;
            faces[4].up = Vector3.UnitZ;

            faces[5].position = position + Vector3.UnitZ;
            faces[5].normal = -Vector3.UnitY;
            faces[5].right = Vector3.UnitX;
            faces[5].up = -Vector3.UnitZ;

            return faces;
        }
    }

    class Face
    {
        public bool shouldBeRemoved = false;
        public Vector3 position;//Bottom left of face
        public Vector3 CenterPosition
        {
            get
            {
                return position + (right / 2) + (up / 2);
            }
        }
        public int Layer
        {
            get
            {
                return (int)(Vector3.Abs(normal) * position).Length();
            }
        }
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