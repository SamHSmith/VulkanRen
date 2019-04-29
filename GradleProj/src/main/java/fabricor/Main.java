package fabricor;

import java.nio.ByteBuffer;
import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.nio.LongBuffer;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;

import org.checkerframework.checker.units.qual.kg;
import org.lwjgl.PointerBuffer;
import org.lwjgl.glfw.GLFW;
import org.lwjgl.glfw.GLFWVulkan;
import org.lwjgl.system.MemoryUtil;
import org.lwjgl.vulkan.EXTDebugReport;
import org.lwjgl.vulkan.KHRSurface;
import org.lwjgl.vulkan.KHRSwapchain;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VK11;
import org.lwjgl.vulkan.VkApplicationInfo;
import org.lwjgl.vulkan.VkDebugReportCallbackCreateInfoEXT;
import org.lwjgl.vulkan.VkDebugReportCallbackEXT;
import org.lwjgl.vulkan.VkDevice;
import org.lwjgl.vulkan.VkDeviceCreateInfo;
import org.lwjgl.vulkan.VkDeviceQueueCreateInfo;
import org.lwjgl.vulkan.VkExtensionProperties;
import org.lwjgl.vulkan.VkExtent2D;
import org.lwjgl.vulkan.VkInstance;
import org.lwjgl.vulkan.VkInstanceCreateInfo;
import org.lwjgl.vulkan.VkLayerProperties;
import org.lwjgl.vulkan.VkPhysicalDevice;
import org.lwjgl.vulkan.VkPhysicalDeviceFeatures;
import org.lwjgl.vulkan.VkPhysicalDeviceProperties;
import org.lwjgl.vulkan.VkQueue;
import org.lwjgl.vulkan.VkQueueFamilyProperties;
import org.lwjgl.vulkan.VkSurfaceCapabilitiesKHR;
import org.lwjgl.vulkan.VkSurfaceFormatKHR;

public class Main {

	private static final ByteBuffer[] validationLayers = new ByteBuffer[] {
			StandardCharsets.UTF_8.encode("VK_LAYER_LUNARG_standard_validation") };
	private static PointerBuffer deviceExtensions;
	private static boolean enableValidationLayers = false;
	private static final VkDebugReportCallbackEXT debugCallback = new VkDebugReportCallbackEXT() {
		public int invoke(int flags, int objectType, long object, long location, int messageCode, long pLayerPrefix,
				long pMessage, long pUserData) {
			System.err.println("ERROR OCCURED: " + VkDebugReportCallbackEXT.getString(pMessage));
			return 0;
		}
	};
	
	static int Width=1600,Height=900;
	static int RenderWidth=1600,RenderHeight=900;

	static long window = 0;
	static long debugCallbackHandle = 0;
	static long windowSurface = 0;
	static VkInstance inst;
	static VkDevice device;
	static VkQueue graphicsQueue;

	public static void main(String[] args) {
		if (args.length > 0 && args[0].contentEquals("debug"))
			enableValidationLayers = true;

		if (!GLFW.glfwInit()) {
			throw new RuntimeException("Failed to initialize GLFW");
		}

		deviceExtensions = MemoryUtil.memAllocPointer(1);
		ByteBuffer VK_KHR_SWAPCHAIN_EXTENSION = MemoryUtil.memUTF8("VK_KHR_swapchain");
		deviceExtensions.put(VK_KHR_SWAPCHAIN_EXTENSION);
		deviceExtensions.flip();

		inst = createVkInstance();
		if (enableValidationLayers)
			debugCallbackHandle = SetUpDebugLogs(inst, EXTDebugReport.VK_DEBUG_REPORT_ERROR_BIT_EXT
					| EXTDebugReport.VK_DEBUG_REPORT_WARNING_BIT_EXT | EXTDebugReport.VK_DEBUG_REPORT_DEBUG_BIT_EXT);
		CreateGLFWWindow();

		VkPhysicalDevice physicalDevice = PickPhysicalDevice(inst);
		device = CreateLogicalDevice(physicalDevice);

		while (!GLFW.glfwWindowShouldClose(window)) {
			GLFW.glfwPollEvents();
		}

		VK10.vkDestroyDevice(device, null);
		EXTDebugReport.vkDestroyDebugReportCallbackEXT(inst, debugCallbackHandle, null);
		VK10.vkDestroyInstance(inst, null);
		GLFW.glfwDestroyWindow(window);
		GLFW.glfwTerminate();
		deviceExtensions.free();
	}

	private static VkDevice CreateLogicalDevice(VkPhysicalDevice dev) {
		QueueFamilyIndices ind = FindQueueFamilies(dev);

		VkDeviceQueueCreateInfo queueCreate = VkDeviceQueueCreateInfo.calloc();
		queueCreate.sType(VK10.VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO).queueFamilyIndex(ind.graphics.get(0));
		FloatBuffer priority = MemoryUtil.memAllocFloat(1);
		queueCreate.pQueuePriorities(priority);

		VkDeviceQueueCreateInfo.Buffer queueCreateInfos = VkDeviceQueueCreateInfo.calloc(1);
		queueCreateInfos.put(queueCreate);
		queueCreateInfos.flip();

		VkPhysicalDeviceFeatures feat = VkPhysicalDeviceFeatures.calloc();

		PointerBuffer ppEnabledLayerNames = MemoryUtil.memAllocPointer(validationLayers.length);
		for (int i = 0; enableValidationLayers && i < validationLayers.length; i++)
			ppEnabledLayerNames.put(validationLayers[i]);

		VkDeviceCreateInfo createInfo = VkDeviceCreateInfo.calloc();
		createInfo.pQueueCreateInfos(queueCreateInfos).pEnabledFeatures(feat).ppEnabledExtensionNames(deviceExtensions)
				.ppEnabledLayerNames(ppEnabledLayerNames);
		PointerBuffer pDevice = MemoryUtil.memAllocPointer(1);
		if (VK10.vkCreateDevice(dev, createInfo, null, pDevice) != VK10.VK_SUCCESS) {
			throw new RuntimeException("Failed to create logical vulkan device!");
		}
		long device = pDevice.get(0);
		VkDevice logdev = new VkDevice(device, dev, createInfo);

		PointerBuffer pQueue = MemoryUtil.memAllocPointer(1);
		VK10.vkGetDeviceQueue(logdev, ind.graphics.get(0), 0, pQueue);
		long gQueue = pQueue.get(0);
		graphicsQueue = new VkQueue(gQueue, logdev);

		queueCreate.free();
		MemoryUtil.memFree(priority);
		queueCreateInfos.free();
		feat.free();
		ppEnabledLayerNames.free();
		createInfo.free();
		pDevice.free();
		pQueue.free();

		return logdev;
	}

	private static void CreateGLFWWindow() {
		GLFW.glfwWindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
		GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_FALSE);
		window = GLFW.glfwCreateWindow(Width, Height, "Vulkan Window", 0, 0);

		long[] surface = new long[1];
		if (GLFWVulkan.glfwCreateWindowSurface(inst, window, null, surface) != VK10.VK_SUCCESS) {
			throw new RuntimeException("Failed to create window Surface!");
		}
		windowSurface = surface[0];
		
	}

	private static int ChooseSwapSurfaceFormat(ArrayList<VkSurfaceFormatKHR> formats) {
		if (formats.size() == 1 && formats.get(0).format() == VK10.VK_FORMAT_UNDEFINED) {
			return -1;
		} else {
			for (int i = 0; i < formats.size(); i++) {
				if (formats.get(i).format() == VK10.VK_FORMAT_B8G8R8A8_UNORM
						&& formats.get(i).colorSpace() == KHRSurface.VK_COLOR_SPACE_SRGB_NONLINEAR_KHR) {
					return i;
				}
			}
			return 0;
		}
	}
	
	private static int ChooseSwapPresentMode(ArrayList<Integer> modes) {
		if(modes.contains(KHRSurface.VK_PRESENT_MODE_FIFO_RELAXED_KHR)) {
			return KHRSurface.VK_PRESENT_MODE_FIFO_RELAXED_KHR;
		}
		return KHRSurface.VK_PRESENT_MODE_IMMEDIATE_KHR;
	}
	
	private static VkExtent2D ChooseSwapExtent(VkSurfaceCapabilitiesKHR cap) {
		if(RenderWidth < cap.minImageExtent().width()) {
			RenderWidth=cap.minImageExtent().width();
		}
		if(RenderWidth > cap.maxImageExtent().width()) {
			RenderWidth=cap.maxImageExtent().width();
		}
		
		if(RenderHeight < cap.minImageExtent().height()) {
			RenderHeight=cap.minImageExtent().height();
		}
		if(RenderHeight > cap.maxImageExtent().height()) {
			RenderHeight=cap.maxImageExtent().height();
		}
		
		VkExtent2D extent=VkExtent2D.calloc();
		extent.width(RenderWidth).height(RenderHeight);
		
		return extent;
	}

	private static VkPhysicalDevice PickPhysicalDevice(VkInstance inst) {
		int[] deviceCount = new int[1];
		int err = VK10.vkEnumeratePhysicalDevices(inst, deviceCount, null);
		if (err != VK10.VK_SUCCESS || deviceCount[0] == 0) {
			throw new RuntimeException("Failed to find GPUs with Vulkan support!");
		}
		PointerBuffer pPhysicalDevices = MemoryUtil.memAllocPointer(deviceCount[0]);
		err = VK10.vkEnumeratePhysicalDevices(inst, deviceCount, pPhysicalDevices);
		if (err != VK10.VK_SUCCESS)
			throw new RuntimeException("Failed to get physical devices.");

		long device = 0;

		for (int i = 0; i < pPhysicalDevices.capacity(); i++) {
			if (EvaluateDevice(pPhysicalDevices.get(i))) {
				device = pPhysicalDevices.get(i);
				break;
			}
		}
		if (device == 0) {
			throw new RuntimeException("Could not find suitible physical device for vulkan.");
		}

		pPhysicalDevices.free();
		// TODO Pick best device not first.
		return new VkPhysicalDevice(device, inst);
	}

	private static boolean EvaluateDevice(long device) {
		boolean Approved = true;
		VkPhysicalDevice dev = new VkPhysicalDevice(device, inst);
		VkPhysicalDeviceProperties prop = VkPhysicalDeviceProperties.calloc();
		VK10.vkGetPhysicalDeviceProperties(dev, prop);
		VkPhysicalDeviceFeatures feat = VkPhysicalDeviceFeatures.calloc();
		VK10.vkGetPhysicalDeviceFeatures(dev, feat);

		if (prop.apiVersion() < 4198490) {
			Approved = false;
		}
		Approved &= FindQueueFamilies(dev).IsComplete();

		Approved &= checkDeviceExtensionSupport(dev);

		SwapChainSupportDetails det = SwapChainSupport(dev);
		Approved &= !det.formats.isEmpty();

		det.Free();
		prop.free();
		feat.free();
		return Approved;
	}

	private static boolean checkDeviceExtensionSupport(VkPhysicalDevice dev) {
		IntBuffer pExtensionCount = MemoryUtil.memAllocInt(1);
		VK10.vkEnumerateDeviceExtensionProperties(dev, MemoryUtil.memCalloc(1), pExtensionCount, null);
		VkExtensionProperties.Buffer availableExtensionsBuffer = VkExtensionProperties.calloc(pExtensionCount.get(0));
		VK10.vkEnumerateDeviceExtensionProperties(dev, MemoryUtil.memCalloc(1), pExtensionCount,
				availableExtensionsBuffer);
		MemoryUtil.memFree(pExtensionCount);
		for (int i = 0; i < deviceExtensions.capacity(); i++) {
			String strlayer = deviceExtensions.getStringUTF8(i);
			boolean layerfound = false;

			for (VkExtensionProperties vklayer : availableExtensionsBuffer) {
				if (vklayer.extensionNameString().contentEquals(strlayer)) {
					layerfound = true;
					break;
				}
			}

			if (!layerfound) {
				availableExtensionsBuffer.free();
				return false;
			}
		}
		availableExtensionsBuffer.free();
		return true;
	}

	private static QueueFamilyIndices FindQueueFamilies(VkPhysicalDevice dev) {
		int[] queueFamCount = new int[1];
		VK10.vkGetPhysicalDeviceQueueFamilyProperties(dev, queueFamCount, null);

		VkQueueFamilyProperties.Buffer fams = VkQueueFamilyProperties.calloc(queueFamCount[0]);
		VK10.vkGetPhysicalDeviceQueueFamilyProperties(dev, queueFamCount, fams);
		QueueFamilyIndices indices = new QueueFamilyIndices();
		for (int i = 0; i < fams.capacity(); i++) {
			if ((fams.get(i).queueFlags() & VK10.VK_QUEUE_GRAPHICS_BIT) != 0) {
				indices.graphics.add(i);
			}
		}
		return indices;
	}

	private static SwapChainSupportDetails SwapChainSupport(VkPhysicalDevice dev) {
		SwapChainSupportDetails det = new SwapChainSupportDetails();
		KHRSurface.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(dev, windowSurface, det.capabilities);

		IntBuffer pSurfaceFormatCount = MemoryUtil.memAllocInt(1);
		KHRSurface.vkGetPhysicalDeviceSurfaceFormatsKHR(dev, windowSurface, pSurfaceFormatCount, null);

		if (pSurfaceFormatCount.get(0) != 0) {
			VkSurfaceFormatKHR.Buffer formats = VkSurfaceFormatKHR.calloc(pSurfaceFormatCount.get(0));
			KHRSurface.vkGetPhysicalDeviceSurfaceFormatsKHR(dev, windowSurface, pSurfaceFormatCount, formats);
			for (int i = 0; i < formats.capacity(); i++) {
				det.formats.add(formats.get(i));
			}
			formats.free();
		}
		MemoryUtil.memFree(pSurfaceFormatCount);

		IntBuffer pPresentModeCount = MemoryUtil.memAllocInt(1);
		KHRSurface.vkGetPhysicalDeviceSurfacePresentModesKHR(dev, windowSurface, pPresentModeCount, null);
		int presentModeCount = pPresentModeCount.get(0);

		IntBuffer pPresentModes = MemoryUtil.memAllocInt(presentModeCount);
		KHRSurface.vkGetPhysicalDeviceSurfacePresentModesKHR(dev, windowSurface, pPresentModeCount, pPresentModes);
		MemoryUtil.memFree(pPresentModeCount);

		for (int i = 0; i < pPresentModes.capacity(); i++) {
			det.presentModes.add(pPresentModes.get(i));
		}
		MemoryUtil.memFree(pPresentModes);

		return det;
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

class QueueFamilyIndices {
	public ArrayList<Integer> graphics = new ArrayList<Integer>();

	public boolean IsComplete() {
		return !graphics.isEmpty();
	}
}

class SwapChainSupportDetails {
	VkSurfaceCapabilitiesKHR capabilities = VkSurfaceCapabilitiesKHR.calloc();
	ArrayList<VkSurfaceFormatKHR> formats = new ArrayList<VkSurfaceFormatKHR>();
	ArrayList<Integer> presentModes = new ArrayList<Integer>();

	public void Free() {
		capabilities.free();
	}
}
