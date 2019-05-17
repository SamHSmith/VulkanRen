package fabricor.rendering;

import java.nio.LongBuffer;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.Random;
import java.util.concurrent.atomic.AtomicInteger;

import org.joml.Matrix4f;
import org.joml.Vector3f;
import org.lwjgl.BufferUtils;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkCommandBuffer;
import org.lwjgl.vulkan.VkDevice;

import fabricor.grids.*;
import fabricor.main.Main;

public class MasterRenderer {

	private static LongBuffer lp = BufferUtils.createLongBuffer(1);
	private static MemoryStack stack;
	private static VkDevice device;
	private static VkCommandBuffer cmdbuff;
	private static Matrix4f viewMat = new Matrix4f();
	private static Matrix4f cameraMat = new Matrix4f();
	private static ShaderBuffer viewBuff;
	private static RenderModel quad;
	private static ArrayList<IRenderCube> toRenderCubes = new ArrayList<IRenderCube>();
	private static ShaderBuffer instBuff;
	private static ArrayList<StaticGridCube> cubes = new ArrayList<StaticGridCube>();

	public static ShaderBuffer GetViewBuffer() {
		viewBuff.put(viewMat, 0);
		return viewBuff;
	}

	public static void Init(VkDevice device, MemoryStack stack) {
		MasterRenderer.device = device;
		MasterRenderer.stack = stack;

		quad = new RenderModel(MasterRenderer.device);
		quad.prepare();

		viewBuff = new ShaderBuffer(MasterRenderer.device, 16 * 4);
		viewBuff.prepare(1, VK10.VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
		viewBuff.put(viewMat, 0);

		Random r = new Random();
		for (int i = 0; i < 100000; i++) {
			StaticGridCube cube = new StaticGridCube();
			cube.position = new Vector3f((float) r.nextDouble() * 100 - 50, (float) r.nextDouble() * 100 - 50,
					(float) r.nextDouble() * 100);
			cube.rotation.rotateAxis((float) r.nextDouble(),
					new Vector3f((float) r.nextDouble(), (float) r.nextDouble(), (float) r.nextDouble()));
			cubes.add(cube);
			toRenderCubes.add((IRenderCube) cube);
		}

		instBuff= bindCubes(toRenderCubes);
	}

	static long renderTime = 0;

	public static void MasterRender(VkCommandBuffer cmdbuff) {
		MasterRenderer.cmdbuff = cmdbuff;
		viewMat.identity().setPerspectiveLH(70, (float) Main.getWidth() / (float) Main.getHeight(), 0.1f, 100000,
				true);
		viewMat.mul(cameraMat);
		viewBuff.put(viewMat, 0);

		cameraMat.translate(0, 0, 0.1f);

		for (Iterator<StaticGridCube> iterator = cubes.iterator(); iterator.hasNext();) {
			StaticGridCube cube = (StaticGridCube) iterator.next();
			cube.rotation.rotateAxis((float) Math.toRadians(0.8f), new Vector3f(1, 1, 0));
		}

		BindModel(quad, instBuff);
		VK10.vkCmdDrawIndexed(cmdbuff, quad.getIndexCount(), instBuff.getStrideCount(), 0, 0, 0);
	}

	private static ShaderBuffer bindCubes(ArrayList<IRenderCube> cubes) {
		ShaderBuffer instBuff = new ShaderBuffer(device);
		instBuff.prepare(cubes.size() * 6);

		instBuff.mapMemory();
		int i = 0;
		for (Iterator<IRenderCube> iterator = cubes.iterator(); iterator.hasNext();) {
			IRenderCube cube = (IRenderCube) iterator.next();
			BindCube(cube, i * 6, instBuff);
			i++;
		}
		instBuff.unMapMemory();
		return instBuff;
	}

	private static Matrix4f upFace = new Matrix4f().translate(new Vector3f(0, -0.5f, 0))
			.rotate((float) Math.toRadians(-90), new Vector3f(1, 0, 0)),
			frontFace = new Matrix4f().translate(new Vector3f(0, 0, -0.5f)),
			bottomFace = new Matrix4f().translate(new Vector3f(0, 0.5f, 0)).rotate((float) Math.toRadians(90),
					new Vector3f(1, 0, 0)),
			backFace = new Matrix4f().translate(new Vector3f(0, 0, 0.5f)).rotate((float) Math.PI,
					new Vector3f(1, 0, 0)),
			rightFace = new Matrix4f().translate(new Vector3f(0.5f, 0, 0)).rotate((float) Math.toRadians(-90),
					new Vector3f(0, 1, 0)),
			leftFace = new Matrix4f().translate(new Vector3f(-0.5f, 0, 0)).rotate((float) Math.toRadians(90),
					new Vector3f(0, 1, 0));

	private static void BindCube(IRenderCube cube, int index, ShaderBuffer instbuffer) {
		Matrix4f cubeMat = new Matrix4f();
		cubeMat.translate(cube.getWorldPosition());
		cubeMat.rotate(cube.getWorldRotation());

		instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(upFace), index);
		index++;

		instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(frontFace), index);
		index++;

		instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(bottomFace), index);
		index++;

		instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(backFace), index);
		index++;

		instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(rightFace), index);
		index++;

		instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(leftFace), index);
		index++;

	}

	private static void BindModel(RenderModel m, ShaderBuffer in) {
		lp.put(0, 0);
		LongBuffer pVertBuffers = MasterRenderer.stack.longs(m.vertexBuffer);
		LongBuffer pInstanceBuffers = MasterRenderer.stack.longs(in.buffer);

		VK10.vkCmdBindVertexBuffers(cmdbuff, 0, pVertBuffers, lp);
		VK10.vkCmdBindVertexBuffers(cmdbuff, 1, pInstanceBuffers, lp);

		VK10.vkCmdBindIndexBuffer(cmdbuff, m.indexBuffer, 0, VK10.VK_INDEX_TYPE_UINT16);
	}
	
	public static void RenderModels(RenderModel m, ShaderBuffer in) {
		
	}

	public static void cleanUp() {
		if (instBuff != null) {
			instBuff.free();
			instBuff = null;
		}
		viewBuff.free();
		quad.free();
	}

}
