package rendering;

import java.nio.LongBuffer;

import org.lwjgl.BufferUtils;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkCommandBuffer;

import fabricor.main.RenderModel;

public class MasterRenderer {
	
	private static LongBuffer lp = BufferUtils.createLongBuffer(1);
	private static MemoryStack stack;
	private static VkCommandBuffer cmdbuff;

	
	public static void MasterRender(MemoryStack stack, VkCommandBuffer cmdbuff, RenderModel m) {
		MasterRenderer.stack=stack;
		MasterRenderer.cmdbuff=cmdbuff;
		
		BindModel(m);
		
        VK10.vkCmdDrawIndexed(cmdbuff, m.getVertexCount(), 1, 0, 0, 0);
	}
	
	private static void BindModel(RenderModel m) {
		lp.put(0, 0);
        LongBuffer pVertBuffers = MasterRenderer.stack.longs(m.vertexBuffer);
        
        VK10.vkCmdBindVertexBuffers(cmdbuff, 0, pVertBuffers, lp);
        VK10.vkCmdBindIndexBuffer(cmdbuff, m.indexBuffer, 0, VK10.VK_INDEX_TYPE_UINT16);
	}
	
}
