package rendering;

import java.nio.LongBuffer;

import org.joml.Matrix4f;
import org.joml.Vector3f;
import org.joml.Vector3fc;
import org.lwjgl.BufferUtils;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkCommandBuffer;
import org.lwjgl.vulkan.VkDevice;

public class MasterRenderer {
	
	private static LongBuffer lp = BufferUtils.createLongBuffer(1);
	private static MemoryStack stack;
	private static VkCommandBuffer cmdbuff;

	static Matrix4f mat=new Matrix4f().translation(0.5f, -0.5f, 0);
	
	static Matrix4f viewMat=new Matrix4f().setPerspectiveLH(70, 1, 0.01f, 100000, true);
	static InstanceBuffer viewBuff;
	
	
	public static void Init(VkDevice device, MemoryStack stack) {
		MasterRenderer.stack=stack;
		
		viewBuff=new InstanceBuffer(device, 16*4);
		viewBuff.prepare(1);
		viewBuff.Put(viewMat, 0);
	}
	
	public static void MasterRender(RenderModel m,  VkCommandBuffer cmdbuff) {
		MasterRenderer.cmdbuff=cmdbuff;
		viewMat.translate(0,0,0.01f);
		viewBuff.Put(viewMat, 0);
		mat.rotate(0.05f, new Vector3f(0,0,1));
		m.inst.Put(mat, 0);
		BindModel(m,m.inst);
		
        VK10.vkCmdDrawIndexed(cmdbuff, m.getIndexCount(), 1, 0, 0, 0);
        
        
	}
	
	private static void BindModel(RenderModel m,InstanceBuffer in) {
		lp.put(0, 0);
        LongBuffer pVertBuffers = MasterRenderer.stack.longs(m.vertexBuffer);
        LongBuffer pInstanceBuffers = MasterRenderer.stack.longs(in.buffer);
        LongBuffer pViewBuffers = MasterRenderer.stack.longs(viewBuff.buffer);
        
        VK10.vkCmdBindVertexBuffers(cmdbuff, 0, pVertBuffers, lp);
        VK10.vkCmdBindVertexBuffers(cmdbuff, 1, pInstanceBuffers, lp);
        VK10.vkCmdBindVertexBuffers(cmdbuff, 2, pViewBuffers, lp);
        VK10.vkCmdBindIndexBuffer(cmdbuff, m.indexBuffer, 0, VK10.VK_INDEX_TYPE_UINT16);
	}
	
}
