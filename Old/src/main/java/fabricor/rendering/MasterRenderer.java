package fabricor.rendering;

import java.nio.LongBuffer;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Random;
import java.util.concurrent.Callable;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Future;
import java.util.concurrent.atomic.AtomicInteger;

import org.joml.Matrix4f;
import org.joml.Vector3f;
import org.joml.Vector3i;
import org.lwjgl.BufferUtils;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VkCommandBuffer;
import org.lwjgl.vulkan.VkDevice;

import fabricor.grids.Grid;
import fabricor.grids.StaticGridCube;
import fabricor.main.Main;

public class MasterRenderer {

	private static LongBuffer lp = BufferUtils.createLongBuffer(1);
	private static MemoryStack stack;
	private static VkDevice device;
	private static VkCommandBuffer cmdbuff;
	private static Matrix4f viewMat = new Matrix4f();
	private static ShaderBuffer viewBuff;
	private static RenderModel quad;
	private static HashMap<Grid, ShaderBuffer> gridbuffers = new HashMap<Grid, ShaderBuffer>();
	private static ExecutorService service = Executors.newFixedThreadPool(Runtime.getRuntime().availableProcessors());
	public static ArrayList<IRenderable> toRenderObjs = new ArrayList<IRenderable>();

	public static ArrayList<Camera> cameras = new ArrayList<Camera>();

	public static ShaderBuffer GetViewBuffer() {
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

	}

	public static void MasterRender(VkCommandBuffer cmdbuff) {
		MasterRenderer.cmdbuff = cmdbuff;
		if (viewBuff.getStrideCount() != cameras.size() * toRenderObjs.size()) {
			viewBuff.free();
			viewBuff.prepare(cameras.size() * toRenderObjs.size(), VK10.VK_BUFFER_USAGE_UNIFORM_BUFFER_BIT);
			Main.UpdateDescriptorSet();
		}
		int cameraindex = 0;
		for (Camera cam : cameras) {
			for (IRenderable obj : toRenderObjs) {
				viewMat.identity().setPerspectiveLH((float) Math.toRadians(cameras.get(0).getFov()),
						(float) Main.getWidth() / (float) Main.getHeight(), 0.1f, 100000, true);
				viewMat.mul(cam.getTransform().invert().mul(obj.getTransform()));
				viewBuff.put(viewMat, cameraindex);
				cameraindex++;
			}
		}
		cameraindex = 0;
		for (Camera cam : cameras) {
			for (IRenderable obj : toRenderObjs) {

				Main.bindDescriptorSets(cameraindex * viewBuff.localStride);
				cameraindex++;

				RenderObject(obj);
			}
		}

	}

	

	private static void RenderObject(IRenderable obj) {
		if (obj instanceof Grid) {
			RenderGrid((Grid) obj);
		}
	}

	private static void RenderGrid(Grid g) {

		if (!gridbuffers.containsKey(g))
			gridbuffers.put(g, null);

		if (g.IsGridEmpty())
			return;

		if (g.shouldBind())
			gridbuffers.put(g, bindCubes(g.getRenderCubes().toArray(new IRenderCube[0]), gridbuffers.get(g)));

		BindModel(quad, gridbuffers.get(g));
		VK10.vkCmdDrawIndexed(cmdbuff, quad.getIndexCount(), gridbuffers.get(g).getStrideCount(), 0, 0, 0);
	}

	private static ShaderBuffer bindCubes(final IRenderCube[] cubes, ShaderBuffer instBuff) {
		if (instBuff == null)
			instBuff = new ShaderBuffer(device);

		int totalquads = TotalQuads(cubes);
		if (instBuff.getStrideCount() != totalquads) {
			instBuff.free();
			instBuff.prepare(totalquads);
		}

		final ShaderBuffer buffer = instBuff;

		final int threadCount = Runtime.getRuntime().availableProcessors();

		final AtomicInteger ai = new AtomicInteger();
		final AtomicInteger quad = new AtomicInteger();
		Callable<Integer> c = new Callable<Integer>() {
			@Override
			public Integer call() throws Exception {

				int index = ai.getAndIncrement();
				int cubeCount = Math.floorDiv(cubes.length, threadCount);
				int i = index * cubeCount;

				if (index + 1 >= threadCount)
					cubeCount += cubes.length - (threadCount * cubeCount);
				for (int c = 0; c < cubeCount; c++) {
					IRenderCube cube = cubes[i];
					int bufferIndex = quad.getAndAdd(QuadCount(cube));
					BindCube(cube, bufferIndex, buffer);
					i++;
				}

				return i;
			}
		};

		ArrayList<Future<Integer>> result = new ArrayList<Future<Integer>>();
		for (int i = 0; i < threadCount; i++) {
			result.add(service.submit(c));
		}
		try {
			for (Future<Integer> res : result) {
				res.get();
			}
		} catch (InterruptedException e) {
			e.printStackTrace();
		} catch (ExecutionException e) {
			e.printStackTrace();
		}

		instBuff.pushMemory();
		return instBuff;
	}

	private static int TotalQuads(IRenderCube[] cubes) {
		int total = 0;
		for (int i = 0; i < cubes.length; i++) {
			total += QuadCount(cubes[i]);
		}
		return total;
	}

	private static int QuadCount(IRenderCube c) {
		int count = 0;
		for (int i = 0; i < c.getNonOccludedSides().length; i++) {
			if (c.getNonOccludedSides()[i]) {
				count++;
			}
		}
		return count;
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
		int i = 0;
		if (cube.getNonOccludedSides()[0]) {
			instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(upFace), index + i);
			i++;
		}
		if (cube.getNonOccludedSides()[1]) {
			instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(frontFace), index + i);
			i++;
		}
		if (cube.getNonOccludedSides()[2]) {
			instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(bottomFace), index + i);
			i++;
		}
		if (cube.getNonOccludedSides()[3]) {
			instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(backFace), index + i);
			i++;
		}
		if (cube.getNonOccludedSides()[4]) {
			instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(rightFace), index + i);
			i++;
		}
		if (cube.getNonOccludedSides()[5]) {
			instbuffer.putThreadSafe(new Matrix4f(cubeMat).mul(leftFace), index + i);
			i++;
		}
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
		for (ShaderBuffer sb : gridbuffers.values()) {
			if (sb != null)
				sb.free();
		}
		viewBuff.free();
		quad.free();
		service.shutdown();
	}

}