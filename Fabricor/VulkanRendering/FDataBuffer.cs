using System.Runtime.InteropServices;
using static Vulkan.VulkanNative;
using Vulkan;
using System;

namespace Fabricor.VulkanRendering
{
    public unsafe class FDataBuffer<T> where T : unmanaged
    {
        private VkDevice device;

        private ulong size = 0;
        private void* spanStart=(void*)0;
        private int spanLength=0;
        private Span<T> Span{get{return new Span<T>(spanStart,spanLength);}}

        public VkBuffer Buffer { get; private set; }
        public VkDeviceMemory Memory { get; private set; }
        public FDataBuffer(VkDevice device, VkPhysicalDevice physicalDevice, int length, VkBufferUsageFlags usage, VkSharingMode sharingMode)
        {
            this.device = device;

            VkBufferCreateInfo createInfo = VkBufferCreateInfo.New();
            size = (ulong)(sizeof(T) * length);
            createInfo.size = size;
            createInfo.usage = usage;
            createInfo.sharingMode = sharingMode;

            VkBuffer buffer = VkBuffer.Null;
            Assert(vkCreateBuffer(device, &createInfo, null, &buffer));
            this.Buffer = buffer;

            VkMemoryRequirements memoryRequirements = new VkMemoryRequirements();
            vkGetBufferMemoryRequirements(device, this.Buffer, &memoryRequirements);

            VkPhysicalDeviceMemoryProperties memoryProperties = new VkPhysicalDeviceMemoryProperties();
            vkGetPhysicalDeviceMemoryProperties(physicalDevice, &memoryProperties);

            VkMemoryAllocateInfo allocateInfo = VkMemoryAllocateInfo.New();
            allocateInfo.allocationSize = memoryRequirements.size;
            allocateInfo.memoryTypeIndex = SelectMemoryType(memoryProperties, memoryRequirements.memoryTypeBits,
            VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent);

            VkDeviceMemory memory = new VkDeviceMemory();
            Assert(vkAllocateMemory(device, &allocateInfo, null, &memory));
            Memory=memory;

            Assert(vkBindBufferMemory(device, this.Buffer, memory, 0));

            spanLength=length;

            
        }

        private uint SelectMemoryType(Vulkan.VkPhysicalDeviceMemoryProperties memoryProperties, uint memoryTypeBits, VkMemoryPropertyFlags flags)
        {
            VkMemoryType[] types = GetMemoryTypes(memoryProperties);
            for (uint i = 0; i < types.Length; i++)
            {
                if ((memoryTypeBits & (1 << (int)i)) != 0 && ((uint)types[i].propertyFlags & (uint)flags) == (uint)flags)
                {
                    return i;
                }
            }
            throw new Exception("No Compatible Memory Found");
        }

        public Span<T> Map(){
            void* data = (void*)0;
            vkMapMemory(device, Memory, 0, size, 0, &data);
            
            spanStart=data;
            return Span;
        }

        /*You should assign your old span variable with this blank value to stop an exception from the debugger */
        public Span<T> UnMap(){
            vkUnmapMemory(device,Memory);
            return new Span<T>();
        }

        public void Free(){
            vkDestroyBuffer(device,Buffer,null);
            vkFreeMemory(device,Memory,null);
        }

        private VkMemoryType[] GetMemoryTypes(Vulkan.VkPhysicalDeviceMemoryProperties memoryProperties)
        {
            VkMemoryType[] types = new VkMemoryType[32];
            types[0] = memoryProperties.memoryTypes_0;
            types[1] = memoryProperties.memoryTypes_1;
            types[2] = memoryProperties.memoryTypes_2;
            types[3] = memoryProperties.memoryTypes_3;
            types[4] = memoryProperties.memoryTypes_4;
            types[5] = memoryProperties.memoryTypes_5;
            types[6] = memoryProperties.memoryTypes_6;
            types[7] = memoryProperties.memoryTypes_7;
            types[8] = memoryProperties.memoryTypes_8;
            types[9] = memoryProperties.memoryTypes_9;
            types[10] = memoryProperties.memoryTypes_10;
            types[11] = memoryProperties.memoryTypes_11;
            types[12] = memoryProperties.memoryTypes_12;
            types[13] = memoryProperties.memoryTypes_13;
            types[14] = memoryProperties.memoryTypes_14;
            types[15] = memoryProperties.memoryTypes_15;
            types[16] = memoryProperties.memoryTypes_16;
            types[17] = memoryProperties.memoryTypes_17;
            types[18] = memoryProperties.memoryTypes_18;
            types[19] = memoryProperties.memoryTypes_19;
            types[20] = memoryProperties.memoryTypes_20;
            types[21] = memoryProperties.memoryTypes_21;
            types[22] = memoryProperties.memoryTypes_22;
            types[23] = memoryProperties.memoryTypes_23;
            types[24] = memoryProperties.memoryTypes_24;
            types[25] = memoryProperties.memoryTypes_25;
            types[26] = memoryProperties.memoryTypes_26;
            types[27] = memoryProperties.memoryTypes_27;
            types[28] = memoryProperties.memoryTypes_28;
            types[29] = memoryProperties.memoryTypes_29;
            types[30] = memoryProperties.memoryTypes_30;
            types[31] = memoryProperties.memoryTypes_31;

            VkMemoryType[] types2 = new VkMemoryType[memoryProperties.memoryTypeCount];
            for (int i = 0; i < types2.Length; i++)
            {
                types2[i] = types[i];
            }
            return types2;
        }
        static void Assert(VkResult result)
        {
            if (result != VkResult.Success)
            {
                Console.Error.WriteLine($"Error: {result}");
                throw new System.Exception($"Error: {result}");
            }
        }

    }

}