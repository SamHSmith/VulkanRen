using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using System.Numerics;
using System.Linq;
using System.Threading.Tasks;
using Vulkan;
using System;
using System.Collections.Generic;

namespace Fabricor.VulkanRendering.VoxelRenderer
{


    public static class VoxelMeshFactory
    {

        public static MeshWrapper<VoxelVertex> GenerateMesh(VkDevice device, VkPhysicalDevice physicalDevice, bool optimize = false)
        {
            MeshWrapper<VoxelVertex> mesh = new MeshWrapper<VoxelVertex>();
            mesh.CreateMesh(() =>
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Mesh<VoxelVertex> meshlocal = _DoGenerateMesh(device, physicalDevice, optimize);
                Console.WriteLine($"Total time taken to make mesh: {((double)stopwatch.ElapsedTicks) / Stopwatch.Frequency * 1000} ms");
                return meshlocal;
            });
            return mesh;
        }
        private static Mesh<VoxelVertex> _DoGenerateMesh(VkDevice device, VkPhysicalDevice physicalDevice, bool optimize)
        {
            List<Face> faces = new List<Face>();
            Stopwatch stopwatch = Stopwatch.StartNew();
            Random random = new Random(42);
            ushort[,,] blocks = new ushort[VoxelRenderChunk.CHUNK_SIZE, VoxelRenderChunk.CHUNK_SIZE, VoxelRenderChunk.CHUNK_SIZE];
            for (int x2 = 0; x2 < VoxelRenderChunk.CHUNK_SIZE; x2++)
            {
                for (int z2 = 0; z2 < VoxelRenderChunk.CHUNK_SIZE; z2++)
                {
                    for (int y2 = 0; y2 < VoxelRenderChunk.CHUNK_SIZE; y2++)
                    {
                        blocks[x2, y2, z2] = (ushort)(random.Next(6) + 1);
                    }
                }
            }
            
            VoxelRenderChunk chunk = new VoxelRenderChunk(blocks);
            Span<ushort> span = chunk.Span;
            int x = 0, y = 0, z = 0;
            for (int i = 0; i < span.Length; i++)
            {
                if (span[i] != 0)
                    faces.AddRange(GenerateFaces(new Vector3(x, y, z), span[i]));

                z++;
                if (z >= VoxelRenderChunk.CHUNK_SIZE)
                {
                    z = 0;
                    y++;
                    if (y >= VoxelRenderChunk.CHUNK_SIZE)
                    {
                        y = 0;
                        x++;
                    }
                }
            }
            Face[] faceArr = faces.ToArray();
            if (optimize)
            {
                LoopFaces(ref faceArr, Vector3.UnitX);
                LoopFaces(ref faceArr, Vector3.UnitY);
                LoopFaces(ref faceArr, Vector3.UnitZ);
            }

            VoxelVertex[] vertices = new VoxelVertex[faceArr.Length * 4]; uint[] indicies = new uint[faceArr.Length * 6];
            for (uint i = 0; i < faceArr.Length; i++)
            {
                faceArr[i].Generate(ref vertices, ref indicies, i * 4, i * 6);
            }
            stopwatch.Stop();
            Console.WriteLine(((double)stopwatch.ElapsedTicks) / Stopwatch.Frequency * 1000);
            Console.WriteLine($"Vertex Count: {vertices.Length}, Index Count: {indicies.Length}");
            return new Mesh<VoxelVertex>(device, physicalDevice, vertices.ToArray(), indicies.ToArray());
        }
        private static void LoopFaces(ref Face[] outFaces, Vector3 axis)
        {
            RemoveFaces(ref outFaces);
            Face[] faces = (outFaces.Where((f) => Vector3.Abs(f.normal) == axis)).ToArray();
            faces = faces.OrderBy(f => f.Layer).ToArray();

            int maxLayer = 0;
            foreach (var f in faces)
            {
                if (f.Layer > maxLayer)
                    maxLayer = f.Layer;
            }
            int[] startPoints = new int[maxLayer + 1 + 1];
            for (int i = 1; i < startPoints.Length; i++)
            {
                if (i == maxLayer + 1)
                {
                    startPoints[i] = faces.Length;
                    continue;
                }
                for (int j = 0; j < faces.Length; j++)
                {
                    if (faces[j].Layer >= i)
                    {
                        startPoints[i] = j;
                        break;
                    }
                }
            }
            Memory<Face> mainMemory = faces.AsMemory();
            Parallel.For(0, maxLayer, (layer) =>
            {
                Span<Face> localArray = mainMemory.Span.Slice(startPoints[layer], startPoints[layer + 1] - startPoints[layer]);
                for (int i = 0; i < localArray.Length; i++)
                    for (int j = i + 1; j < localArray.Length; j++)
                    {
                        if (localArray[i].shouldBeRemoved || localArray[j].shouldBeRemoved)
                            continue;
                        if (localArray[i].centerPosition == localArray[j].centerPosition)
                        {
                            localArray[i].shouldBeRemoved = true;
                            localArray[j].shouldBeRemoved = true;
                        }
                    }
            });

            UpdateFaces(ref outFaces, faces);
            RemoveFaces(ref outFaces);
            faces = (outFaces.Where((f) => Vector3.Abs(f.normal) == axis)).ToArray();
            faces = faces.OrderBy(f => f.Layer).ToArray();

            maxLayer = 0;
            foreach (var f in faces)
            {
                if (f.Layer > maxLayer)
                    maxLayer = f.Layer;
            }
            startPoints = new int[maxLayer + 1 + 1];
            for (int i = 1; i < startPoints.Length; i++)
            {
                if (i == maxLayer + 1)
                {
                    startPoints[i] = faces.Length;
                    continue;
                }
                for (int j = 0; j < faces.Length; j++)
                {
                    if (faces[j].Layer >= i)
                    {
                        startPoints[i] = j;
                        break;
                    }
                }
            }

            mainMemory = faces.AsMemory();
            for (int i = 0; i < maxLayer + 1; i++)
            {
                Memory<Face> mergeFaces = mainMemory.Slice(startPoints[i], startPoints[i + 1] - startPoints[i]);
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


            UpdateFaces(ref outFaces, faces);
            RemoveFaces(ref outFaces);
        }

        private static void UpdateFaces(ref Face[] faceList, Memory<Face> data)
        {
            Span<Face> span = data.Span;
            for (int i = 0; i < data.Length; i++)
            {
                Face f = span[i];
                if (f.shouldBeRemoved || faceList[span[i].index].shouldBeRemoved)
                    f.shouldBeRemoved = true;
                faceList[span[i].index] = f;
            }
        }

        private static void RemoveFaces(ref Face[] faceList)
        {
            Face[] arr = new Face[faceList.Length];
            faceList.CopyTo(arr, 0);

            int index = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].shouldBeRemoved != true)
                {
                    faceList[index] = arr[i];
                    faceList[index].index = index;
                    index++;
                }
            }
            Array.Resize(ref faceList, index + 1);
        }
        private static void MergeFaces(Memory<Face> mem, Vector3 compareAxis, Vector3 otherAxis)
        {

            for (int i = 0; i < mem.Length; i++)
            {
                Span<Face> faces = mem.Span;
                for (int j = 0; j < faces.Length; j++)//inefficient loop but it makes stuff easier
                {
                    if (i == j)//This is self explanitory
                        continue;



                    if (faces[i].textureID != faces[j].textureID)
                        continue;//The faces have the same texture

                    if (faces[i].shouldBeRemoved || faces[j].shouldBeRemoved)
                        continue;//None of the faces are queued for destruction

                    if ((faces[i].position * otherAxis) != (faces[j].position * otherAxis))
                        continue;//If not aligned on other axis, skip.

                    if (((faces[i].position + faces[i].right + faces[i].up) * compareAxis) != (faces[j].position * compareAxis))
                        continue;//is face j next to face i?

                    if ((faces[i].right * compareAxis).LengthSquared() > 0)
                    {//Comparision is on the right axis
                        if (faces[i].up != faces[j].up)
                            continue;//Are the faces the same size?

                        faces[i].right += faces[j].right;
                        faces[j].shouldBeRemoved = true;
                    }
                    else
                    {//Comparision is on the up axis
                        if (faces[i].right != faces[j].right)
                            continue;//Are the faces the same size? And are they going the same direction?

                        faces[i].up += faces[j].up;
                        faces[j].shouldBeRemoved = true;
                    }
                }
            }
        }

        private static Face[] GenerateFaces(Vector3 position, ushort blockID)
        {
            Face[] faces = new Face[6];

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i] = new Face();
                faces[i].textureID = (ushort)(blockID);//TODO add custom textures for differnet sides with a block lookup
            }

            faces[0].position = position;
            faces[0].normal = -Vector3.UnitZ;
            faces[0].right = Vector3.UnitX;
            faces[0].up = Vector3.UnitY;

            faces[1].position = position + Vector3.UnitZ;
            faces[1].normal = -Vector3.UnitX;
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

            for (int i = 0; i < faces.Length; i++)
            {
                faces[i].UpdateCenterPosition();
            }
            return faces;
        }
    }

    struct Face
    {
        public int index;
        public bool shouldBeRemoved;
        public Vector3 position;//Bottom left of face

        public Vector3 centerPosition;
        public void UpdateCenterPosition()
        {
            centerPosition = position + (right / 2) + (up / 2);

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

        public void Generate(ref VoxelVertex[] vertices, ref uint[] indicies, uint vIndex, uint iIndex)
        {
            for (int i = 0; i < 4; i++)
            {
                vertices[vIndex + i].normal = normal;
                vertices[vIndex + i].textureId = textureID;
            }

            vertices[vIndex + 0].position = position;
            vertices[vIndex + 0].texcoords = Vector2.Zero;

            vertices[vIndex + 1].position = position + up;
            vertices[vIndex + 1].texcoords = new Vector2(0, up.Length());

            vertices[vIndex + 2].position = position + up + right;
            vertices[vIndex + 2].texcoords = new Vector2(right.Length(), up.Length());

            vertices[vIndex + 3].position = position + right;
            vertices[vIndex + 3].texcoords = new Vector2(right.Length(), 0);

            indicies[iIndex + 0] = vIndex + 0;
            indicies[iIndex + 1] = vIndex + 1;
            indicies[iIndex + 2] = vIndex + 2;

            indicies[iIndex + 3] = vIndex + 0;
            indicies[iIndex + 4] = vIndex + 2;
            indicies[iIndex + 5] = vIndex + 3;
        }
    }
}