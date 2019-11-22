using System.Net.Mime;
using System.Numerics;
using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using System.Threading;
using GLFW;
using static GLFW.Glfw;
using Vulkan;
using static Vulkan.VulkanNative;
using Fabricor.VulkanRendering;

namespace Fabricor.VulkanRendering
{
    public unsafe class FGraphicsPipeline
    {

        public VkPipeline pipeline;
        public VkPipelineLayout pipelineLayout;
        public VkDescriptorPool descriptorPool;
        public VkDescriptorSetLayout desclayout;
        public VkDescriptorSet[] descriptorSets;
        public uint swapchainImageCount;
        public FGraphicsPipeline(VkDevice device, VkPipelineCache pipelineCache, VkRenderPass renderPass,
        string shaderPath,uint swapchainImageCount,VkImageView texView)
        {
            this.swapchainImageCount=swapchainImageCount;

            desclayout=CreateDescriptorLayout(device);
            descriptorPool=CreateDescriptorPool(device,swapchainImageCount);
            descriptorSets=AllocateDescriptorSets(device,desclayout, descriptorPool,
            swapchainImageCount);

            VkSampler sampler = CreateSampler(device);

            for (int i = 0; i < swapchainImageCount; i++)
            {
                VkDescriptorImageInfo imageInfo = new VkDescriptorImageInfo();
                imageInfo.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;
                imageInfo.imageView = texView;
                imageInfo.sampler = sampler;

                VkWriteDescriptorSet[] writes = new VkWriteDescriptorSet[1];
                writes[0].dstSet = descriptorSets[i];
                writes[0].dstBinding = 0;
                writes[0].dstArrayElement = 0;
                writes[0].descriptorType = VkDescriptorType.CombinedImageSampler;
                writes[0].descriptorCount = 1;
                writes[0].pImageInfo = &imageInfo;
                fixed (VkWriteDescriptorSet* ptr = writes)
                    vkUpdateDescriptorSets(device, (uint)writes.Length, ptr, 0, null);
            }

            VkShaderModule vs = LoadShader(device, $"{shaderPath}.vert.spv");
            VkShaderModule fs = LoadShader(device, $"{shaderPath}.frag.spv");


            VkGraphicsPipelineCreateInfo pCreateInfo = VkGraphicsPipelineCreateInfo.New();
            pCreateInfo.flags = VkPipelineCreateFlags.DisableOptimization;

            VkPipelineShaderStageCreateInfo[] shaderStages = new VkPipelineShaderStageCreateInfo[2];
            shaderStages[0] = VkPipelineShaderStageCreateInfo.New();
            shaderStages[0].stage = VkShaderStageFlags.Vertex;
            shaderStages[0].module = vs;

            byte[] vsFuncName = Encoding.UTF8.GetBytes("main" + char.MinValue);
            fixed (byte* ptr = &(vsFuncName[0]))
                shaderStages[0].pName = ptr;

            shaderStages[1] = VkPipelineShaderStageCreateInfo.New();
            shaderStages[1].stage = VkShaderStageFlags.Fragment;
            shaderStages[1].module = fs;
            byte[] fsFuncName = Encoding.UTF8.GetBytes("main" + char.MinValue);
            fixed (byte* ptr = &(fsFuncName[0]))
                shaderStages[1].pName = ptr;

            fixed (VkPipelineShaderStageCreateInfo* ptr = shaderStages)
                pCreateInfo.pStages = ptr;
            pCreateInfo.stageCount = 2;

            VkVertexInputBindingDescription[] bindings = new VkVertexInputBindingDescription[4];
            bindings[0] = new VkVertexInputBindingDescription();
            bindings[0].binding = 0;
            bindings[0].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[0].inputRate = VkVertexInputRate.Vertex;

            bindings[1] = new VkVertexInputBindingDescription();
            bindings[1].binding = 1;
            bindings[1].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[1].inputRate = VkVertexInputRate.Vertex;

            bindings[2] = new VkVertexInputBindingDescription();
            bindings[2].binding = 2;
            bindings[2].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[2].inputRate = VkVertexInputRate.Vertex;

            bindings[3] = new VkVertexInputBindingDescription();
            bindings[3].binding = 3;
            bindings[3].stride = (uint)sizeof(VoxelRenderer.VoxelVertex);
            bindings[3].inputRate = VkVertexInputRate.Vertex;

            VkVertexInputAttributeDescription[] attribs = new VkVertexInputAttributeDescription[4];
            attribs[0].binding = 0;
            attribs[0].location = 0;
            attribs[0].format = VkFormat.R32g32b32Sfloat;
            attribs[0].offset = 0;

            attribs[1].binding = 1;
            attribs[1].location = 1;
            attribs[1].format = VkFormat.R32g32b32Sfloat;
            attribs[1].offset = 0;

            attribs[2].binding = 2;
            attribs[2].location = 2;
            attribs[2].format = VkFormat.R32g32b32Sfloat;
            attribs[2].offset = 0;

            attribs[3].binding = 3;
            attribs[3].location = 3;
            attribs[3].format = VkFormat.R32g32b32Sfloat;
            attribs[3].offset = 0;

            VkPipelineVertexInputStateCreateInfo vertexInput = VkPipelineVertexInputStateCreateInfo.New();
            fixed (VkVertexInputBindingDescription* ptr = bindings)
                vertexInput.pVertexBindingDescriptions = ptr;
            vertexInput.vertexBindingDescriptionCount = (uint)bindings.Length;
            fixed (VkVertexInputAttributeDescription* ptr = attribs)
                vertexInput.pVertexAttributeDescriptions = ptr;
            vertexInput.vertexAttributeDescriptionCount = (uint)attribs.Length;
            pCreateInfo.pVertexInputState = &vertexInput;

            VkPipelineInputAssemblyStateCreateInfo inputAssembly = VkPipelineInputAssemblyStateCreateInfo.New();
            inputAssembly.topology = VkPrimitiveTopology.TriangleList;
            pCreateInfo.pInputAssemblyState = &inputAssembly;

            VkPipelineViewportStateCreateInfo viewportState = VkPipelineViewportStateCreateInfo.New();
            viewportState.viewportCount = 1;
            viewportState.scissorCount = 1;
            pCreateInfo.pViewportState = &viewportState;

            VkPipelineRasterizationStateCreateInfo rasterizationState = VkPipelineRasterizationStateCreateInfo.New();
            rasterizationState.lineWidth = 1;
            pCreateInfo.pRasterizationState = &rasterizationState;

            VkPipelineMultisampleStateCreateInfo multisampleState = VkPipelineMultisampleStateCreateInfo.New();
            multisampleState.rasterizationSamples = VkSampleCountFlags.Count1;
            pCreateInfo.pMultisampleState = &multisampleState;

            VkPipelineDepthStencilStateCreateInfo depthState = VkPipelineDepthStencilStateCreateInfo.New();
            pCreateInfo.pDepthStencilState = &depthState;

            VkPipelineColorBlendAttachmentState colourAttachment = new VkPipelineColorBlendAttachmentState();
            colourAttachment.colorWriteMask = VkColorComponentFlags.R | VkColorComponentFlags.G | VkColorComponentFlags.B | VkColorComponentFlags.A;

            VkPipelineColorBlendStateCreateInfo colourState = VkPipelineColorBlendStateCreateInfo.New();
            colourState.pAttachments = &colourAttachment;
            colourState.attachmentCount = 1;
            pCreateInfo.pColorBlendState = &colourState;

            VkDynamicState[] dynamicStates = new VkDynamicState[2];
            dynamicStates[0] = VkDynamicState.Viewport;
            dynamicStates[1] = VkDynamicState.Scissor;

            VkPipelineDynamicStateCreateInfo dynamicState = VkPipelineDynamicStateCreateInfo.New();
            dynamicState.dynamicStateCount = (uint)dynamicStates.Length;
            fixed (VkDynamicState* ptr = &(dynamicStates[0]))
                dynamicState.pDynamicStates = ptr;
            pCreateInfo.pDynamicState = &dynamicState;

            this.pipelineLayout = CreatePipelineLayout(device, desclayout);
            pCreateInfo.layout = this.pipelineLayout;
            pCreateInfo.renderPass = renderPass;
            pCreateInfo.subpass = 0;


            VkPipeline pipeline = VkPipeline.Null;
            Assert(vkCreateGraphicsPipelines(device, pipelineCache, 1, &pCreateInfo, null, &pipeline));
            this.pipeline = pipeline;
        }
        private static VkSampler CreateSampler(VkDevice device)
        {
            VkSamplerCreateInfo createInfo = VkSamplerCreateInfo.New();
            createInfo.magFilter = VkFilter.Nearest;
            createInfo.minFilter = VkFilter.Nearest;

            createInfo.addressModeU = VkSamplerAddressMode.Repeat;
            createInfo.addressModeV = VkSamplerAddressMode.Repeat;
            createInfo.addressModeW = VkSamplerAddressMode.Repeat;

            createInfo.anisotropyEnable = VkBool32.False;
            createInfo.maxAnisotropy = 1;

            createInfo.borderColor = VkBorderColor.FloatOpaqueWhite;
            createInfo.unnormalizedCoordinates = VkBool32.False;

            createInfo.compareEnable = VkBool32.False;
            createInfo.compareOp = VkCompareOp.Always;

            createInfo.mipmapMode = VkSamplerMipmapMode.Linear;
            createInfo.mipLodBias = 0;
            createInfo.minLod = 0;
            createInfo.maxLod = 0;

            VkSampler sampler = VkSampler.Null;
            Assert(vkCreateSampler(device, &createInfo, null, &sampler));

            return sampler;
        }

        private static VkDescriptorSet[] AllocateDescriptorSets(VkDevice device, VkDescriptorSetLayout layout, VkDescriptorPool pool, uint swapchainImageCount)
        {
            VkDescriptorSetLayout[] localLayouts = new VkDescriptorSetLayout[swapchainImageCount];
            for (int i = 0; i < localLayouts.Length; i++)
            {
                localLayouts[i] = layout;
            }
            VkDescriptorSetAllocateInfo allocateInfo = VkDescriptorSetAllocateInfo.New();
            allocateInfo.descriptorPool = pool;
            allocateInfo.descriptorSetCount = swapchainImageCount;
            fixed (VkDescriptorSetLayout* ptr = localLayouts)
                allocateInfo.pSetLayouts = ptr;

            VkDescriptorSet[] sets = new VkDescriptorSet[swapchainImageCount];
            fixed (VkDescriptorSet* ptr = sets)
                Assert(vkAllocateDescriptorSets(device, &allocateInfo, ptr));
            return sets;
        }
        private static VkDescriptorPool CreateDescriptorPool(VkDevice device, uint swapchainImageCount)
        {
            VkDescriptorPoolSize size = new VkDescriptorPoolSize();
            size.descriptorCount = swapchainImageCount;
            size.type = VkDescriptorType.CombinedImageSampler;

            VkDescriptorPoolCreateInfo createInfo = VkDescriptorPoolCreateInfo.New();
            createInfo.poolSizeCount = 1;
            createInfo.pPoolSizes = &size;
            createInfo.maxSets = swapchainImageCount;

            VkDescriptorPool pool = VkDescriptorPool.Null;
            Assert(vkCreateDescriptorPool(device, &createInfo, null, &pool));
            return pool;
        }

        private static VkDescriptorSetLayout CreateDescriptorLayout(VkDevice device)
        {
            VkDescriptorSetLayoutBinding samplerLayoutBinding = new VkDescriptorSetLayoutBinding();
            samplerLayoutBinding.binding = 0;
            samplerLayoutBinding.descriptorCount = 1;
            samplerLayoutBinding.descriptorType = VkDescriptorType.CombinedImageSampler;
            samplerLayoutBinding.pImmutableSamplers = null;
            samplerLayoutBinding.stageFlags = VkShaderStageFlags.Fragment;

            VkDescriptorSetLayoutCreateInfo layoutInfo = VkDescriptorSetLayoutCreateInfo.New();
            layoutInfo.bindingCount = 1;
            layoutInfo.pBindings = &samplerLayoutBinding;

            VkDescriptorSetLayout descriptorSetLayout = VkDescriptorSetLayout.Null;
            Assert(vkCreateDescriptorSetLayout(device, &layoutInfo, null, &descriptorSetLayout));
            return descriptorSetLayout;
        }

        static VkPipelineLayout CreatePipelineLayout(VkDevice device, VkDescriptorSetLayout setLayout)
        {
            VkPipelineLayoutCreateInfo pCreateInfo = VkPipelineLayoutCreateInfo.New();
            pCreateInfo.pushConstantRangeCount = 0;
            pCreateInfo.setLayoutCount = 1;
            pCreateInfo.pSetLayouts = &setLayout;

            VkPipelineLayout layout = VkPipelineLayout.Null;
            Assert(vkCreatePipelineLayout(device, &pCreateInfo, null, &layout));
            return layout;
        }

        static VkShaderModule LoadShader(VkDevice device, string path)
        {
            byte[] bytes = File.ReadAllBytes(path);

            uint length = (uint)bytes.Length;

            VkShaderModuleCreateInfo pCreateInfo = VkShaderModuleCreateInfo.New();
            pCreateInfo.codeSize = (UIntPtr)length;
            fixed (byte* ptr = bytes)
                pCreateInfo.pCode = (uint*)ptr;

            VkShaderModule shaderModule = new VkShaderModule();
            Assert(vkCreateShaderModule(device, &pCreateInfo, null, &shaderModule));

            return shaderModule;
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