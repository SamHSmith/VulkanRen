package rendering;

import java.nio.LongBuffer;
import java.util.ArrayList;
import java.util.Iterator;

import org.joml.Matrix4f;
import org.joml.Vector3f;
import org.joml.Vector3fc;
import org.joml.sampling.BestCandidateSampling.Cube;
import org.lwjgl.BufferUtils;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkCommandBuffer;
import org.lwjgl.vulkan.VkDevice;

import fabricor.main.Main;
import grids.StaticGridCube;

public class MasterRenderer {

	private static LongBuffer lp = BufferUtils.createLongBuffer(1);
	private static MemoryStack stack;
	private static VkDevice device;
	private static VkCommandBuffer cmdbuff;
	private static Matrix4f viewMat = new Matrix4f();
	private static ShaderBuffer viewBuff;
	private static RenderModel quad;
	private static ArrayList<IRenderCube> toRenderCubes = new ArrayList<IRenderCube>();
	private static ShaderBuffer instBuff;

	static StaticGridCube cube;

	public static ShaderBuffer GetViewBuffer() {
		viewBuff.Put(viewMat, 0);
		return viewBuff;
	}

	public static void Init(VkDevice device, MemoryStack stack) {
		MasterRenderer.device = device;
		MasterRenderer.stack = stack;

		quad = new RenderModel(MasterRenderer.device);
		quad.prepare();

		viewBuff = new ShaderBuffer(MasterRenderer.device, 16 * 4);
		viewBuff.prepare(1, VK10.VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
		viewBuff.Put(viewMat, 0);

		cube = new StaticGridCube();
		cube.position = new Vector3f(0, 0, 3);
		toRenderCubes.add(cube);
	}

	public static void MasterRender(VkCommandBuffer cmdbuff) {
		MasterRenderer.cmdbuff = cmdbuff;
		viewMat.identity().setPerspectiveLH(70, (float)Main.getWidth()/(float)Main.getHeight(), 0.01f, 100000, true);
		viewBuff.Put(viewMat, 0);

		cube.rotation.rotateAxis((float) Math.toRadians(0.8f), new Vector3f(1, 1, 0));

		RenderCubes(toRenderCubes);

	}

	

	private static void RenderCubes(ArrayList<IRenderCube> cubes) {
		if (instBuff != null) {
			instBuff.free();
		}
		instBuff = new ShaderBuffer(device);
		instBuff.prepare(cubes.size() * 6);
		
		int i = 0;
		for (Iterator<IRenderCube> iterator = cubes.iterator(); iterator.hasNext();) {
			IRenderCube rc = (IRenderCube) iterator.next();
			i = BindCube(rc, i, instBuff);
		}
		BindModel(quad, instBuff);
		VK10.vkCmdDrawIndexed(cmdbuff, quad.getIndexCount(), i, 0, 0, 0);

	}

	private static int BindCube(IRenderCube cube, int index, ShaderBuffer instbuffer) {
		Matrix4f cubeMat = new Matrix4f();
		cubeMat.translate(cube.getWorldPosition());
		cubeMat.rotate(cube.getWorldRotation());

		instbuffer.Put(new Matrix4f(cubeMat).mul(new Matrix4f().translate(new Vector3f(0, -0.5f, 0))
				.rotate((float) Math.toRadians(-90), new Vector3f(1, 0, 0))), index);
		index++;

		instbuffer.Put(new Matrix4f(cubeMat).mul(new Matrix4f().translate(new Vector3f(0, 0, -0.5f))), index);
		index++;

		instbuffer.Put(new Matrix4f(cubeMat).mul(new Matrix4f().translate(new Vector3f(0, 0.5f, 0))
				.rotate((float) Math.toRadians(90), new Vector3f(1, 0, 0))), index);
		index++;

		instbuffer.Put(new Matrix4f(cubeMat).mul(new Matrix4f().translate(new Vector3f(0, 0, 0.5f)))
				.rotate((float) Math.PI, new Vector3f(1, 0, 0)), index);
		index++;

		instbuffer.Put(new Matrix4f(cubeMat).mul(new Matrix4f().translate(new Vector3f(0.5f, 0, 0))
				.rotate((float) Math.toRadians(-90), new Vector3f(0, 1, 0))), index);
		index++;

		instbuffer.Put(new Matrix4f(cubeMat).mul(new Matrix4f().translate(new Vector3f(-0.5f, 0, 0))
				.rotate((float) Math.toRadians(90), new Vector3f(0, 1, 0))), index);
		index++;

		return index;
	}

	private static void BindModel(RenderModel m, ShaderBuffer in) {
		lp.put(0, 0);
		LongBuffer pVertBuffers = MasterRenderer.stack.longs(m.vertexBuffer);
		LongBuffer pInstanceBuffers = MasterRenderer.stack.longs(in.buffer);

		VK10.vkCmdBindVertexBuffers(cmdbuff, 0, pVertBuffers, lp);
		VK10.vkCmdBindVertexBuffers(cmdbuff, 1, pInstanceBuffers, lp);
		

		VK10.vkCmdBindIndexBuffer(cmdbuff, m.indexBuffer, 0, VK10.VK_INDEX_TYPE_UINT16);
	}

	public static void cleanUp() {
		if (instBuff != null) {
			instBuff.free();
			instBuff=null;
		}
		viewBuff.free();
		quad.free();
	}

}
