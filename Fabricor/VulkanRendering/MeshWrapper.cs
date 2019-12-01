using System.Threading.Tasks;
using System;
using Vulkan;
namespace Fabricor.VulkanRendering{
    public class MeshWrapper<T> where T : unmanaged{
        public bool IsReady{get;private set;}=false;
        public Mesh<T> Mesh{get;private set;}
        public async void CreateMesh(Func<Mesh<T>> func){
            Mesh=await Task<Mesh<T>>.Run(func);
            IsReady=true;
        }

        public void Free(){
            Mesh.Free();
        }
    }
}