package fabricor;

import java.nio.Buffer;
import java.nio.ByteBuffer;
import java.nio.IntBuffer;
import java.nio.LongBuffer;
import java.nio.charset.StandardCharsets;

import javax.management.RuntimeErrorException;

import org.lwjgl.PointerBuffer;
import org.lwjgl.glfw.GLFW;
import org.lwjgl.glfw.GLFWVulkan;
import org.lwjgl.system.MemoryUtil;
import org.lwjgl.vulkan.EXTDebugReport;
import org.lwjgl.vulkan.VK;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VK11;
import org.lwjgl.vulkan.VkApplicationInfo;
import org.lwjgl.vulkan.VkDebugReportCallbackCreateInfoEXT;
import org.lwjgl.vulkan.VkDebugReportCallbackEXT;
import org.lwjgl.vulkan.VkDebugUtilsLabelEXT;
import org.lwjgl.vulkan.VkDevice;
import org.lwjgl.vulkan.VkInstance;
import org.lwjgl.vulkan.VkInstanceCreateInfo;
import org.lwjgl.vulkan.VkLayerProperties;
import org.lwjgl.vulkan.VkPhysicalDevice;

public class Main {

	private static final ByteBuffer[] validationLayers = new ByteBuffer[] {
			StandardCharsets.UTF_8.encode("VK_LAYER_LUNARG_standard_validation") };
	private static boolean enableValidationLayers = false;
	private static final VkDebugReportCallbackEXT debugCallback = new VkDebugReportCallbackEXT() {
		public int invoke(int flags, int objectType, long object, long location, int messageCode, long pLayerPrefix,
				long pMessage, long pUserData) {
			System.err.println("ERROR OCCURED: " + VkDebugReportCallbackEXT.getString(pMessage));
			return 0;
		}
	};

	public static void main(String[] args) {
		if (args.length > 0 && args[0].contentEquals("debug"))
			enableValidationLayers = true;

		if (!GLFW.glfwInit()) {
			throw new RuntimeException("Failed to initialize GLFW");
		}

		GLFW.glfwWindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
		GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_FALSE);
		long window = GLFW.glfwCreateWindow(800, 600, "Vulkan Window", 0, 0);

		VkInstance inst = createVkInstance();
		final long debugCallbackHandle = SetUpDebugLogs(inst,
				EXTDebugReport.VK_DEBUG_REPORT_ERROR_BIT_EXT |
				EXTDebugReport.VK_DEBUG_REPORT_WARNING_BIT_EXT |
				EXTDebugReport.VK_DEBUG_REPORT_DEBUG_BIT_EXT);
		VkPhysicalDevice physicalDevice=PickPhysicalDevice(inst);
		
		

		while (!GLFW.glfwWindowShouldClose(window)) {
			GLFW.glfwPollEvents();
		}

	    EXTDebugReport.vkDestroyDebugReportCallbackEXT(inst, debugCallbackHandle, null);
		VK10.vkDestroyInstance(inst, null);
		GLFW.glfwDestroyWindow(window);
		GLFW.glfwTerminate();
	}
	/*
	private static VkDevice CreateLogicalDevice() {
		
	}
	*/
	
	private static VkPhysicalDevice PickPhysicalDevice(VkInstance inst) {
		int[] deviceCount=new int[1];
		int err =VK10.vkEnumeratePhysicalDevices(inst, deviceCount, null);
		if(err !=VK10.VK_SUCCESS|| deviceCount[0]==0) {
			throw new RuntimeException("Failed to find GPUs with Vulkan support!");
		}
		PointerBuffer pPhysicalDevices = MemoryUtil.memAllocPointer(deviceCount[0]);
		err = VK10.vkEnumeratePhysicalDevices(inst, deviceCount, pPhysicalDevices);
		if(err != VK10.VK_SUCCESS)
			throw new RuntimeException("Failed to get physical devices.");
		
		long device=pPhysicalDevices.get(0);
		pPhysicalDevices.free();
		//TODO Pick best device not first.
		return new VkPhysicalDevice(device,inst);
	}

	private static long SetUpDebugLogs(VkInstance inst, int flags) {
		VkDebugReportCallbackCreateInfoEXT dgbCInfo = VkDebugReportCallbackCreateInfoEXT.calloc()
				.sType(EXTDebugReport.VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT).pNext(0)
				.pfnCallback(debugCallback).flags(flags).pUserData(0);
		LongBuffer pCallback = MemoryUtil.memAllocLong(1);
		
		int err = EXTDebugReport.vkCreateDebugReportCallbackEXT(inst, dgbCInfo, null, pCallback);
		long callbackHandle = pCallback.get(0);
		MemoryUtil.memFree(pCallback);
		dgbCInfo.free();
		if (err != VK10.VK_SUCCESS) {
			throw new AssertionError("Failed to create Debug logs for Vulkan: " + err);
		}
		return callbackHandle;
	}

	private static boolean checkValidationLayerSupport() {
		IntBuffer pPropertyCount = MemoryUtil.memAllocInt(1);
		VK10.vkEnumerateInstanceLayerProperties(pPropertyCount, null);

		VkLayerProperties.Buffer availableLayersBuffer = VkLayerProperties.calloc(pPropertyCount.get(0));

		VK10.vkEnumerateInstanceLayerProperties(pPropertyCount, availableLayersBuffer);
		MemoryUtil.memFree(pPropertyCount);
		for (ByteBuffer layer : validationLayers) {
			String strlayer = StandardCharsets.UTF_8.decode(layer).toString();
			boolean layerfound = false;

			for (VkLayerProperties vklayer : availableLayersBuffer) {
				if (vklayer.layerNameString().contentEquals(strlayer)) {
					layerfound = true;
					break;
				}
			}

			if (!layerfound) {
				availableLayersBuffer.free();
				return false;
			}
		}
		availableLayersBuffer.free();
		return true;
	}

	private static PointerBuffer getRequiredExtensions() {
		PointerBuffer glfwExtensions = GLFWVulkan.glfwGetRequiredInstanceExtensions();
		int debugExtensionCount = 0;
		if (enableValidationLayers) {
			debugExtensionCount = 1;
		}
		PointerBuffer extensions = MemoryUtil.memCallocPointer(glfwExtensions.capacity() + debugExtensionCount);
		for (int i = 0; i < glfwExtensions.capacity(); i++) {
			extensions.put(glfwExtensions.get());
		}
		if (enableValidationLayers) {
			ByteBuffer VK_EXT_DEBUG_REPORT_EXTENSION = MemoryUtil.memUTF8("VK_EXT_debug_report");
			extensions.put(VK_EXT_DEBUG_REPORT_EXTENSION);
		}
		extensions.flip();

		return extensions;
	}

	private static VkInstance createVkInstance() {
		if (enableValidationLayers && !checkValidationLayerSupport()) {
			throw new RuntimeException("Validation layers requested but not available!");
		}

		VkApplicationInfo appInfo = VkApplicationInfo.calloc().sType(VK10.VK_STRUCTURE_TYPE_APPLICATION_INFO)
				.apiVersion(VK11.VK_API_VERSION_1_1);

		VkInstanceCreateInfo createInfo = VkInstanceCreateInfo.calloc()
				.sType(VK10.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO).pApplicationInfo(appInfo);

		PointerBuffer extensions = getRequiredExtensions();
		createInfo.ppEnabledExtensionNames(extensions);// extensions

		PointerBuffer layernames = MemoryUtil.memCallocPointer(validationLayers.length);// validation layers
		if (enableValidationLayers) {

			for (int i = 0; i < validationLayers.length; i++) {
				layernames.put(validationLayers[i]);
			}
			createInfo.ppEnabledLayerNames(layernames);

		}

		PointerBuffer vkinst = MemoryUtil.memAllocPointer(1);
		int err = VK10.vkCreateInstance(createInfo, null, vkinst);
		if (err != VK10.VK_SUCCESS) {
			System.err.println(err);
			throw new RuntimeException("Failed to initialize Vulkan");
		}
		long inst = vkinst.get(0);
		MemoryUtil.memFree(vkinst);
		VkInstance vk = new VkInstance(inst, createInfo);
		appInfo.free();
		createInfo.free();
		layernames.free();
		return vk;
	}

}
