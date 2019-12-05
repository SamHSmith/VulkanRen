using System.Threading;
using System.Threading.Tasks;
using System;
using Vulkan;
namespace Fabricor.VulkanRendering
{
    public class MeshWrapper<T> where T : unmanaged
    {
        public bool IsReady
        {
            get
            {
                return Mesh != null;
            }
        }
        private bool isBeingUsed=false;
        private bool isBeingEdited=false;
        public Mesh<T> Mesh
        {
            get
            {
                if (meshtype == Meshtype.None)
                {
                    if (slowMesh != null)
                    {
                        mesh = slowMesh;
                        meshtype = Meshtype.Slow;
                    }
                    else if (quickMesh != null)
                    {
                        mesh = quickMesh;
                        meshtype = Meshtype.Fast;
                    }
                    else
                    {
                        return null;
                    }
                }
                if (meshtype == Meshtype.Fast)
                {
                    if (slowMesh != null)
                    {
                        mesh = slowMesh;
                        meshtype = Meshtype.Slow;
                    }
                }
                return mesh;
            }
        }
        private Mesh<T> mesh, quickMesh, slowMesh;//slow is the optimized one
        private Meshtype meshtype = Meshtype.None;
        public async void CreateMesh(Func<Mesh<T>> slowMeshFunc, Func<Mesh<T>> fastMeshFunc)
        {
            quickMesh = await Task<Mesh<T>>.Run(fastMeshFunc);
            slowMesh = await Task<Mesh<T>>.Run(slowMeshFunc);
        }

        public async void UpdateMesh(Func<Mesh<T>> slowMeshFunc, Func<Mesh<T>> fastMeshFunc)
        {
            Mesh<T> quickMesh = await Task<Mesh<T>>.Run(fastMeshFunc);
            Mesh<T> oldquick = this.quickMesh;
            Mesh<T> oldslow = this.slowMesh;

            isBeingEdited=true;
            while(isBeingUsed){}
            this.quickMesh = quickMesh;
            this.slowMesh = null;
            this.meshtype = Meshtype.None;//Now the main loop will swap out the mesh for the quick one.
            isBeingEdited=false;

            Mesh<T> slowMesh = await Task<Mesh<T>>.Run(slowMeshFunc);
            this.slowMesh=slowMesh;

            oldquick.Free();
            oldslow.Free();
        }

        public void Use(){
            while(isBeingEdited){}
            isBeingUsed=true;
        }
        public void StopUse(){
            isBeingUsed=false;
        }

        public void Free()
        {
            mesh.Free();
            quickMesh.Free();
            slowMesh.Free();
        }
    }
    enum Meshtype
    {
        None, Fast, Slow
    }
}