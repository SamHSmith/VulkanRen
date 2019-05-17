package rendering;

import java.nio.FloatBuffer;
import java.nio.LongBuffer;

import org.joml.Matrix4d;
import org.joml.Matrix4f;
import org.joml.Vector4f;
import org.lwjgl.BufferUtils;
import org.lwjgl.PointerBuffer;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.system.MemoryUtil;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkBufferCreateInfo;
import org.lwjgl.vulkan.VkDevice;
import org.lwjgl.vulkan.VkMemoryAllocateInfo;
import org.lwjgl.vulkan.VkMemoryRequirements;

import fabricor.main.Main;

public class ShaderBuffer {

	private static LongBuffer lp = BufferUtils.createLongBuffer(1);
	private final PointerBuffer pp = MemoryUtil.memAllocPointer(1);
	public static final int BufferStride = (16 * 4);// +2+2+2+2;//transform matrix plus TODO texture atlas coords
	public int localStride=0;//exists for viewmatrix
	public long buffer, memory;
	private VkDevice device;
	private long allocationSize;

	public ShaderBuffer(VkDevice device) {
		this.device=device;
		localStride=BufferStride;
	}
	
	public ShaderBuffer(VkDevice device, int stride) {
		this.device=device;
		this.localStride=stride;
	}
	
	public void prepare(int count) {
		prepare(count, VK10.VK_BUFFER_USAGE_VERTEX_BUFFER_BIT);
	}

	public void prepare(int count, int usage) {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkBufferCreateInfo buf_info = VkBufferCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_BUFFER_CREATE_INFO).size(count*localStride)
					.usage(usage).sharingMode(VK10.VK_SHARING_MODE_EXCLUSIVE);

			Main.check(VK10.vkCreateBuffer(device, buf_info, null, lp));
			buffer = lp.get(0);

			VkMemoryRequirements mem_reqs = VkMemoryRequirements.mallocStack(stack);
			VK10.vkGetBufferMemoryRequirements(device, buffer, mem_reqs);

			VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO).allocationSize(mem_reqs.size());
			boolean pass = Main.memory_type_from_properties(mem_reqs.memoryTypeBits(),
					VK10.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK10.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT, mem_alloc);
			assert (pass);

			Main.check(VK10.vkAllocateMemory(device, mem_alloc, null, lp));
			memory = lp.get(0);
			allocationSize=mem_alloc.allocationSize();
			

		}
		
		Main.check(VK10.vkBindBufferMemory(device, buffer, memory, 0));
	}
	
	public void Put(Matrix4f mat, int index) {
		Main.check(VK10.vkMapMemory(device, memory, index*localStride, localStride, 0, pp));
		FloatBuffer data = pp.getFloatBuffer(0, localStride/4);
		data.put(mat.get(BufferUtils.createFloatBuffer(localStride/4)));
//		for (int i = 0; i < data.capacity(); i++) {
//			System.out.println(data.get(i));
//		}
		VK10.vkUnmapMemory(device, memory);
		
	}
	
	public void free() {
		VK10.vkFreeMemory(device, memory, null);
		VK10.vkDestroyBuffer(device, buffer, null);
	}

}
