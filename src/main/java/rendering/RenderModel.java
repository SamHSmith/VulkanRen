package rendering;

import java.nio.FloatBuffer;
import java.nio.LongBuffer;
import java.nio.ShortBuffer;

import org.lwjgl.BufferUtils;
import org.lwjgl.PointerBuffer;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.system.MemoryUtil;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkBufferCreateInfo;
import org.lwjgl.vulkan.VkDevice;
import org.lwjgl.vulkan.VkMemoryAllocateInfo;
import org.lwjgl.vulkan.VkMemoryRequirements;
import org.lwjgl.vulkan.VkPipelineVertexInputStateCreateInfo;
import org.lwjgl.vulkan.VkVertexInputAttributeDescription;
import org.lwjgl.vulkan.VkVertexInputBindingDescription;

import fabricor.main.Main;

public class RenderModel {

	public static final int VertexSize = 4 * 5;
	public long vertexBuffer, indexBuffer, vertexMemory, indexMemory;
	private final LongBuffer lp = BufferUtils.createLongBuffer(1);
	private final PointerBuffer pp = MemoryUtil.memAllocPointer(1);
	private int IndexCount = 0;
	static VkPipelineVertexInputStateCreateInfo vi = VkPipelineVertexInputStateCreateInfo.calloc();
	static VkVertexInputBindingDescription.Buffer vi_bindings = VkVertexInputBindingDescription.calloc(2);
	static VkVertexInputAttributeDescription.Buffer vi_attrs = VkVertexInputAttributeDescription.calloc(2+4);

	private VkDevice device;
	
	public RenderModel(VkDevice device) {
		this.device=device;
	}
	
	public static VkPipelineVertexInputStateCreateInfo getVi() {
		vi.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_VERTEX_INPUT_STATE_CREATE_INFO).pNext(0)
				.pVertexBindingDescriptions(vi_bindings).pVertexAttributeDescriptions(vi_attrs);

		vi_bindings.get(0).binding(0).stride(VertexSize).inputRate(VK10.VK_VERTEX_INPUT_RATE_VERTEX);
		
		vi_bindings.get(1).binding(1).stride(ShaderBuffer.BufferStride).inputRate(VK10.VK_VERTEX_INPUT_RATE_INSTANCE);

		vi_attrs.get(0).binding(0).location(0).format(VK10.VK_FORMAT_R32G32B32_SFLOAT).offset(0);

		vi_attrs.get(1).binding(0).location(1).format(VK10.VK_FORMAT_R32G32_SFLOAT).offset(4 * 3);
		
		vi_attrs.get(2).binding(1).location(2).format(VK10.VK_FORMAT_R32G32B32A32_SFLOAT).offset(0);//matrix
		
		vi_attrs.get(3).binding(1).location(3).format(VK10.VK_FORMAT_R32G32B32A32_SFLOAT).offset(4*4);
		
		vi_attrs.get(4).binding(1).location(4).format(VK10.VK_FORMAT_R32G32B32A32_SFLOAT).offset(4*4*2);
		
		vi_attrs.get(5).binding(1).location(5).format(VK10.VK_FORMAT_R32G32B32A32_SFLOAT).offset(4*4*3);
		return vi;
	}

	public static void cleanUp() {
		vi.free();
		vi_bindings.free();
		vi_attrs.free();
	}
	
	

	public void prepare() {

		float[][] vb = GetVerticies();
		short[] ind = GetIndicies();

		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkBufferCreateInfo buf_info = VkBufferCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO).size(/* sizeof(vb) */ vb.length * VertexSize)
					.usage(VK10.VK_BUFFER_USAGE_VERTEX_BUFFER_BIT).sharingMode(VK10.VK_SHARING_MODE_EXCLUSIVE);

			Main.check(VK10.vkCreateBuffer(device, buf_info, null, lp));
			vertexBuffer = lp.get(0);

			VkMemoryRequirements mem_reqs = VkMemoryRequirements.mallocStack(stack);
			VK10.vkGetBufferMemoryRequirements(device, vertexBuffer, mem_reqs);

			VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO).allocationSize(mem_reqs.size());
			boolean pass = Main.memory_type_from_properties(mem_reqs.memoryTypeBits(),
					VK10.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK10.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, mem_alloc);
			assert (pass);

			Main.check(VK10.vkAllocateMemory(device, mem_alloc, null, lp));
			vertexMemory = lp.get(0);

			Main.check(VK10.vkMapMemory(device, vertexMemory, 0, mem_alloc.allocationSize(), 0, pp));
			FloatBuffer data = pp.getFloatBuffer(0, ((int) mem_alloc.allocationSize()) >> 2);
			for (int i = 0; i < vb.length; i++) {
				data.put(vb[i]);
			}
			data.flip();

		}

		VK10.vkUnmapMemory(device, vertexMemory);

		Main.check(VK10.vkBindBufferMemory(device, vertexBuffer, vertexMemory, 0));
		
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkBufferCreateInfo buf_info = VkBufferCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO).size(/* sizeof(ind) */ ind.length * 2)
					.usage(VK10.VK_BUFFER_USAGE_INDEX_BUFFER_BIT).sharingMode(VK10.VK_SHARING_MODE_EXCLUSIVE);

			Main.check(VK10.vkCreateBuffer(device, buf_info, null, lp));
			indexBuffer = lp.get(0);

			VkMemoryRequirements mem_reqs = VkMemoryRequirements.mallocStack(stack);
			VK10.vkGetBufferMemoryRequirements(device, indexBuffer, mem_reqs);

			VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO).allocationSize(mem_reqs.size());
			boolean pass = Main.memory_type_from_properties(mem_reqs.memoryTypeBits(),
					VK10.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK10.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, mem_alloc);
			assert (pass);

			Main.check(VK10.vkAllocateMemory(device, mem_alloc, null, lp));
			indexMemory = lp.get(0);

			Main.check(VK10.vkMapMemory(device, indexMemory, 0, mem_alloc.allocationSize(), 0, pp));
			ShortBuffer data = pp.getShortBuffer(0, ((int) mem_alloc.allocationSize()) >> 1);
			data.put(ind);
			data.flip();

		}

		VK10.vkUnmapMemory(device, indexMemory);

		Main.check(VK10.vkBindBufferMemory(device, indexBuffer, indexMemory, 0));

	}
	
	

	private float[][] GetVerticies() {
		float[][] vb = {
				/* position texcoord */
				{ -0.5f, -0.5f, 0f, 0.0f, 0.0f }, { 0.5f, -0.5f, 0f, 1.0f, 0.0f },
				{ -0.5f, 0.5f, 0f, 0.0f, 1.0f }, { 0.5f, 0.5f, 0f, 1.0f, 1.0f }, };
		return vb;
	}
	
	public void free() {
		VK10.vkDestroyBuffer(device, vertexBuffer, null);
	    VK10.vkFreeMemory(device, vertexMemory, null);
	    VK10.vkDestroyBuffer(device, indexBuffer, null);
	    VK10.vkFreeMemory(device, indexMemory, null);
	}

	private short[] GetIndicies() {
		short[] ind = { 0, 1, 2, 1, 3, 2 };
		IndexCount = ind.length;
		return ind;
	}

	public int getIndexCount() {
		return IndexCount;
	}
}
