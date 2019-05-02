package fabricor.main;

import java.nio.ByteBuffer;
import java.nio.FloatBuffer;
import java.nio.IntBuffer;
import java.nio.LongBuffer;
import java.util.ArrayList;
import java.util.Iterator;

import org.lwjgl.PointerBuffer;
import org.lwjgl.glfw.GLFW;
import org.lwjgl.glfw.GLFWVulkan;
import org.lwjgl.system.MemoryUtil;
import org.lwjgl.vulkan.EXTDebugUtils;
import org.lwjgl.vulkan.KHRSurface;
import org.lwjgl.vulkan.KHRSwapchain;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VK11;
import org.lwjgl.vulkan.VkApplicationInfo;
import org.lwjgl.vulkan.VkDebugUtilsMessengerCallbackDataEXT;
import org.lwjgl.vulkan.VkDebugUtilsMessengerCallbackEXT;
import org.lwjgl.vulkan.VkDebugUtilsMessengerCreateInfoEXT;
import org.lwjgl.vulkan.VkDevice;
import org.lwjgl.vulkan.VkDeviceCreateInfo;
import org.lwjgl.vulkan.VkDeviceQueueCreateInfo;
import org.lwjgl.vulkan.VkExtensionProperties;
import org.lwjgl.vulkan.VkExtent2D;
import org.lwjgl.vulkan.VkImageViewCreateInfo;
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
import org.lwjgl.vulkan.VkSwapchainCreateInfoKHR;

public class Main {

	private static final ByteBuffer[] validationLayers = new ByteBuffer[]{
			MemoryUtil.memUTF8("VK_LAYER_LUNARG_standard_validation")};
	private static PointerBuffer deviceExtensions;
	private static boolean enableValidationLayers = false;
	private static final VkDebugUtilsMessengerCallbackEXT debugCallback = new VkDebugUtilsMessengerCallbackEXT() {

		public int invoke(int messageSeverity, int messageTypes,
				long pCallbackData, long pUserData) {
			VkDebugUtilsMessengerCallbackDataEXT data = VkDebugUtilsMessengerCallbackDataEXT
					.create(pCallbackData);
			System.out.println(data.pMessageString());
			return 0;
		}
	};

	static int Width = 1600, Height = 900;

	static long window = 0;
	static long debugCallbackHandle = 0;
	static long windowSurface = 0;
	static long swapChain = 0;
	static long[] swapChainImages = new long[0];
	static long[] swapChainImageViews = new long[0];
	static VkSurfaceFormatKHR swapChainFormat;
	static VkExtent2D swapChainExtent;
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
		ByteBuffer VK_KHR_SWAPCHAIN_EXTENSION = MemoryUtil
				.memUTF8("VK_KHR_swapchain");
		deviceExtensions.put(VK_KHR_SWAPCHAIN_EXTENSION);
		deviceExtensions.flip();

		inst = createVkInstance();

		if (enableValidationLayers)
			debugCallbackHandle = SetUpDebugLogs(inst);

		CreateGLFWWindow();

		VkPhysicalDevice physicalDevice = PickPhysicalDevice(inst,
				windowSurface);

		device = CreateLogicalDevice(physicalDevice, windowSurface);

		swapChain = CreateSwapChain(physicalDevice, windowSurface);

		createSwapChainImages();

		while (!GLFW.glfwWindowShouldClose(window)) {
			GLFW.glfwPollEvents();
		}

		for (int i = 0; i < swapChainImageViews.length; i++) {
			VK10.vkDestroyImageView(device, swapChainImageViews[i], null);
		}
		VK10.vkDestroyDevice(device, null);
		if (enableValidationLayers)
			EXTDebugUtils.vkDestroyDebugUtilsMessengerEXT(inst,
					debugCallbackHandle, null);
		VK10.vkDestroyInstance(inst, null);
		GLFW.glfwDestroyWindow(window);
		GLFW.glfwTerminate();
		deviceExtensions.free();
	}

	private static VkDevice CreateLogicalDevice(VkPhysicalDevice dev,
			long surface) {
		QueueFamilyIndices ind = FindQueueFamilies(dev, surface);

		VkDeviceQueueCreateInfo queueCreate = VkDeviceQueueCreateInfo.calloc();
		queueCreate.sType(VK10.VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO)
				.queueFamilyIndex(ind.graphics.get(0));
		FloatBuffer priority = MemoryUtil.memAllocFloat(1);
		queueCreate.pQueuePriorities(priority);

		VkDeviceQueueCreateInfo.Buffer queueCreateInfos = VkDeviceQueueCreateInfo
				.calloc(1);
		queueCreateInfos.put(queueCreate);
		queueCreateInfos.flip();

		VkPhysicalDeviceFeatures feat = VkPhysicalDeviceFeatures.calloc();

		PointerBuffer ppEnabledLayerNames = MemoryUtil.memAllocPointer(0);
		if (enableValidationLayers) {
			MemoryUtil.memFree(ppEnabledLayerNames);
			ppEnabledLayerNames = MemoryUtil
					.memAllocPointer(validationLayers.length);
			for (int i = 0; enableValidationLayers
					&& i < validationLayers.length; i++)
				ppEnabledLayerNames.put(validationLayers[i]);

		}

		VkDeviceCreateInfo createInfo = VkDeviceCreateInfo.calloc();
		createInfo.pQueueCreateInfos(queueCreateInfos).pEnabledFeatures(feat)
				.ppEnabledExtensionNames(deviceExtensions)
				.ppEnabledLayerNames(ppEnabledLayerNames);
		PointerBuffer pDevice = MemoryUtil.memAllocPointer(1);
		if (VK10.vkCreateDevice(dev, createInfo, null,
				pDevice) != VK10.VK_SUCCESS) {
			throw new RuntimeException(
					"Failed to create logical vulkan device!");
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
		if (GLFWVulkan.glfwCreateWindowSurface(inst, window, null,
				surface) != VK10.VK_SUCCESS) {
			throw new RuntimeException("Failed to create window Surface!");
		}
		windowSurface = surface[0];

	}

	private static long CreateSwapChain(VkPhysicalDevice dev, long surface) {
		SwapChainSupportDetails details = SwapChainSupport(dev);

		VkSurfaceCapabilitiesKHR cap = details.capabilities;
		int presentMode = ChooseSwapPresentMode(details.presentModes);
		VkSurfaceFormatKHR format = details.formats
				.get(ChooseSwapSurfaceFormat(details.formats));
		VkExtent2D extent = ChooseSwapExtent(cap);
		int swapChainImageCount = cap.minImageCount() + 1;
		if (cap.maxImageCount() > 0
				&& swapChainImageCount > cap.maxImageCount())
			swapChainImageCount = cap.maxImageCount();

		VkSwapchainCreateInfoKHR swapCreate = VkSwapchainCreateInfoKHR.calloc();
		swapCreate.sType(VkSwapchainCreateInfoKHR.STYPE).surface(surface)
				.minImageCount(swapChainImageCount).imageFormat(format.format())
				.imageColorSpace(format.colorSpace()).imageExtent(extent)
				.imageArrayLayers(1)
				.imageUsage(VK10.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT);

		QueueFamilyIndices indicies = FindQueueFamilies(dev, surface);
		IntBuffer queueFamInds = MemoryUtil.memCallocInt(2);
		queueFamInds.put(indicies.graphics.get(0));
		queueFamInds.put(indicies.present.get(0));

		if (indicies.graphics.get(0) != indicies.present.get(0)) {
			swapCreate.imageSharingMode(VK10.VK_SHARING_MODE_CONCURRENT)
					.pQueueFamilyIndices(queueFamInds);
		} else {
			swapCreate.imageSharingMode(VK10.VK_SHARING_MODE_EXCLUSIVE);
		}

		swapCreate.preTransform(cap.currentTransform())
				.compositeAlpha(KHRSurface.VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR)
				.presentMode(presentMode).clipped(true)
				.oldSwapchain(VK10.VK_NULL_HANDLE);
		long[] pSwapchain = new long[1];
		KHRSwapchain.vkCreateSwapchainKHR(device, swapCreate, null, pSwapchain);

		details.Free();
		extent.free();
		swapCreate.free();

		Main.swapChainFormat = format;
		Main.swapChainExtent = extent;

		return pSwapchain[0];
	}

	private static void createSwapChainImages() {
		int[] imgCount = new int[1];
		KHRSwapchain.vkGetSwapchainImagesKHR(device, swapChain, imgCount, null);
		swapChainImages = new long[imgCount[0]];
		KHRSwapchain.vkGetSwapchainImagesKHR(device, swapChain, imgCount,
				swapChainImages);

		swapChainImageViews = new long[swapChainImages.length];

		for (int i = 0; i < swapChainImages.length; i++) {
			VkImageViewCreateInfo createInfo = VkImageViewCreateInfo.malloc();
			createInfo.sType(VkImageViewCreateInfo.STYPE)
					.image(swapChainImages[i])
					.viewType(VK10.VK_IMAGE_VIEW_TYPE_2D).components()
					.r(VK10.VK_COMPONENT_SWIZZLE_IDENTITY);
			createInfo.components().g(VK10.VK_COMPONENT_SWIZZLE_IDENTITY);
			createInfo.components().b(VK10.VK_COMPONENT_SWIZZLE_IDENTITY);
			createInfo.components().a(VK10.VK_COMPONENT_SWIZZLE_IDENTITY);
			createInfo.subresourceRange()
					.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT);
			createInfo.subresourceRange().baseMipLevel(0);
			createInfo.subresourceRange().levelCount(1);
			createInfo.subresourceRange().baseArrayLayer(0);
			createInfo.subresourceRange().layerCount(0);

			long[] imgView = new long[1];

			if (VK10.vkCreateImageView(device, createInfo, null,
					imgView) != VK10.VK_SUCCESS) {
				throw new RuntimeException("Failed to create image views!");
			}
			swapChainImageViews[i] = imgView[0];

			createInfo.free();

		}
	}

	private static int ChooseSwapSurfaceFormat(
			ArrayList<VkSurfaceFormatKHR> formats) {
		if (formats.size() == 1
				&& formats.get(0).format() == VK10.VK_FORMAT_UNDEFINED) {
			return -1;
		} else {
			for (int i = 0; i < formats.size(); i++) {
				if (formats.get(i).format() == VK10.VK_FORMAT_B8G8R8A8_UNORM
						&& formats.get(i)
								.colorSpace() == KHRSurface.VK_COLOR_SPACE_SRGB_NONLINEAR_KHR) {
					return i;
				}
			}
			return 0;
		}
	}

	private static int ChooseSwapPresentMode(ArrayList<Integer> modes) {
		if (modes.contains(KHRSurface.VK_PRESENT_MODE_FIFO_RELAXED_KHR)) {
			return KHRSurface.VK_PRESENT_MODE_FIFO_RELAXED_KHR;
		}
		return KHRSurface.VK_PRESENT_MODE_IMMEDIATE_KHR;
	}

	private static VkExtent2D ChooseSwapExtent(VkSurfaceCapabilitiesKHR cap) {
		if (Width < cap.minImageExtent().width()) {
			Width = cap.minImageExtent().width();
		}
		if (Width > cap.maxImageExtent().width()) {
			Width = cap.maxImageExtent().width();
		}

		if (Height < cap.minImageExtent().height()) {
			Height = cap.minImageExtent().height();
		}
		if (Height > cap.maxImageExtent().height()) {
			Height = cap.maxImageExtent().height();
		}

		VkExtent2D extent = VkExtent2D.calloc();
		extent.width(Width).height(Height);

		return extent;
	}

	private static VkPhysicalDevice PickPhysicalDevice(VkInstance inst,
			long surface) {
		int[] deviceCount = new int[1];
		int err = VK10.vkEnumeratePhysicalDevices(inst, deviceCount, null);
		if (err != VK10.VK_SUCCESS || deviceCount[0] == 0) {
			throw new RuntimeException(
					"Failed to find GPUs with Vulkan support!");
		}
		PointerBuffer pPhysicalDevices = MemoryUtil
				.memAllocPointer(deviceCount[0]);
		err = VK10.vkEnumeratePhysicalDevices(inst, deviceCount,
				pPhysicalDevices);
		if (err != VK10.VK_SUCCESS)
			throw new RuntimeException("Failed to get physical devices.");

		long device = 0;

		for (int i = 0; i < pPhysicalDevices.capacity(); i++) {
			if (EvaluateDevice(pPhysicalDevices.get(i), surface)) {
				device = pPhysicalDevices.get(i);
				break;
			}
		}
		if (device == 0) {
			throw new RuntimeException(
					"Could not find suitible physical device for vulkan.");
		}

		pPhysicalDevices.free();
		// TODO Pick best device not first.
		return new VkPhysicalDevice(device, inst);
	}

	private static boolean EvaluateDevice(long device, long surface) {
		boolean Approved = true;
		VkPhysicalDevice dev = new VkPhysicalDevice(device, inst);
		VkPhysicalDeviceProperties prop = VkPhysicalDeviceProperties.calloc();
		VK10.vkGetPhysicalDeviceProperties(dev, prop);
		VkPhysicalDeviceFeatures feat = VkPhysicalDeviceFeatures.calloc();
		VK10.vkGetPhysicalDeviceFeatures(dev, feat);

		if (prop.apiVersion() < 4198490) {
			Approved = false;
		}
		Approved &= FindQueueFamilies(dev, surface).IsComplete();

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
		VK10.vkEnumerateDeviceExtensionProperties(dev, MemoryUtil.memCalloc(1),
				pExtensionCount, null);
		VkExtensionProperties.Buffer availableExtensionsBuffer = VkExtensionProperties
				.calloc(pExtensionCount.get(0));
		VK10.vkEnumerateDeviceExtensionProperties(dev, MemoryUtil.memCalloc(1),
				pExtensionCount, availableExtensionsBuffer);
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

	private static QueueFamilyIndices FindQueueFamilies(VkPhysicalDevice dev,
			long surface) {
		int[] queueFamCount = new int[1];
		VK10.vkGetPhysicalDeviceQueueFamilyProperties(dev, queueFamCount, null);

		VkQueueFamilyProperties.Buffer fams = VkQueueFamilyProperties
				.calloc(queueFamCount[0]);
		VK10.vkGetPhysicalDeviceQueueFamilyProperties(dev, queueFamCount, fams);
		QueueFamilyIndices indices = new QueueFamilyIndices();

		IntBuffer supportsPresent = MemoryUtil.memAllocInt(queueFamCount[0]);
		for (int i = 0; i < queueFamCount[0]; i++) {
			supportsPresent.position(i);
			int err = KHRSurface.vkGetPhysicalDeviceSurfaceSupportKHR(dev, i,
					surface, supportsPresent);
			if (err != VK10.VK_SUCCESS) {
				throw new AssertionError(
						"Failed to physical device surface support.");
			}
		}

		for (int i = 0; i < fams.capacity(); i++) {
			if ((fams.get(i).queueFlags() & VK10.VK_QUEUE_GRAPHICS_BIT) != 0) {
				indices.graphics.add(i);
			}
			if (supportsPresent.get(i) == VK10.VK_TRUE) {
				indices.present.add(i);
			}
		}
		MemoryUtil.memFree(supportsPresent);

		for (Iterator<Integer> iterator = indices.graphics.iterator(); iterator
				.hasNext();) {
			int g = (int) iterator.next();
			if (indices.present.contains(g)) {
				indices.graphics.add(0, g);
				indices.present.add(0, g);
				return indices;
			}
		}

		return indices;
	}

	private static SwapChainSupportDetails SwapChainSupport(
			VkPhysicalDevice dev) {
		SwapChainSupportDetails det = new SwapChainSupportDetails();
		KHRSurface.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(dev, windowSurface,
				det.capabilities);

		IntBuffer pSurfaceFormatCount = MemoryUtil.memAllocInt(1);
		KHRSurface.vkGetPhysicalDeviceSurfaceFormatsKHR(dev, windowSurface,
				pSurfaceFormatCount, null);

		if (pSurfaceFormatCount.get(0) != 0) {
			VkSurfaceFormatKHR.Buffer formats = VkSurfaceFormatKHR
					.calloc(pSurfaceFormatCount.get(0));
			KHRSurface.vkGetPhysicalDeviceSurfaceFormatsKHR(dev, windowSurface,
					pSurfaceFormatCount, formats);
			for (int i = 0; i < formats.capacity(); i++) {
				det.formats.add(formats.get(i));
			}
			formats.free();
		}
		MemoryUtil.memFree(pSurfaceFormatCount);

		IntBuffer pPresentModeCount = MemoryUtil.memAllocInt(1);
		KHRSurface.vkGetPhysicalDeviceSurfacePresentModesKHR(dev, windowSurface,
				pPresentModeCount, null);
		int presentModeCount = pPresentModeCount.get(0);

		IntBuffer pPresentModes = MemoryUtil.memAllocInt(presentModeCount);
		KHRSurface.vkGetPhysicalDeviceSurfacePresentModesKHR(dev, windowSurface,
				pPresentModeCount, pPresentModes);
		MemoryUtil.memFree(pPresentModeCount);

		for (int i = 0; i < pPresentModes.capacity(); i++) {
			det.presentModes.add(pPresentModes.get(i));
		}
		MemoryUtil.memFree(pPresentModes);

		return det;
	}

	private static long SetUpDebugLogs(VkInstance inst) {
		VkDebugUtilsMessengerCreateInfoEXT ci = VkDebugUtilsMessengerCreateInfoEXT
				.calloc();
		ci.sType(VkDebugUtilsMessengerCallbackDataEXT.STYPE)
				.messageSeverity(Integer.MAX_VALUE)
				.messageType(Integer.MAX_VALUE).pfnUserCallback(debugCallback);

		LongBuffer pCallback = MemoryUtil.memAllocLong(1);

		int err = EXTDebugUtils.vkCreateDebugUtilsMessengerEXT(inst, ci, null,
				pCallback);
		long callbackHandle = pCallback.get(0);
		MemoryUtil.memFree(pCallback);
		ci.free();
		if (err != VK10.VK_SUCCESS) {
			throw new AssertionError(
					"Failed to create Debug logs for Vulkan: " + err);
		}
		return callbackHandle;
	}

	private static boolean checkValidationLayerSupport() {
		IntBuffer pPropertyCount = MemoryUtil.memAllocInt(1);
		VK10.vkEnumerateInstanceLayerProperties(pPropertyCount, null);

		VkLayerProperties.Buffer availableLayersBuffer = VkLayerProperties
				.calloc(pPropertyCount.get(0));

		VK10.vkEnumerateInstanceLayerProperties(pPropertyCount,
				availableLayersBuffer);
		MemoryUtil.memFree(pPropertyCount);
		for (ByteBuffer layer : validationLayers) {
			layer.position(0);
			String strlayer = MemoryUtil.memUTF8(layer);
			strlayer = strlayer.substring(0, strlayer.length() - 1);// This
																	// string
																	// has a
																	// null
																	// character
																	// at the
																	// end that
			// has to be removed. I think it has to do with the UTF8 null
			// termination.
			boolean layerfound = false;

			for (VkLayerProperties vklayer : availableLayersBuffer) {
				if (strlayer.equalsIgnoreCase(vklayer.layerNameString())) {
					layerfound = true;
					break;
				}
			}

			if (!layerfound) {
				availableLayersBuffer.free();
				System.err.println("ValidationLayer: " + strlayer);
				return false;
			}

		}
		availableLayersBuffer.free();
		return true;
	}

	private static PointerBuffer getRequiredExtensions() {
		PointerBuffer glfwExtensions = GLFWVulkan
				.glfwGetRequiredInstanceExtensions();
		int debugExtensionCount = 0;
		if (enableValidationLayers) {
			debugExtensionCount = 1;
		}
		PointerBuffer extensions = MemoryUtil.memCallocPointer(
				glfwExtensions.capacity() + debugExtensionCount);
		for (int i = 0; i < glfwExtensions.capacity(); i++) {
			extensions.put(glfwExtensions.get());
		}
		if (enableValidationLayers) {
			ByteBuffer VK_EXT_DEBUG_REPORT_EXTENSION = MemoryUtil
					.memUTF8("VK_EXT_debug_utils");
			extensions.put(VK_EXT_DEBUG_REPORT_EXTENSION);
		}
		extensions.flip();

		return extensions;
	}

	private static VkInstance createVkInstance() {
		if (enableValidationLayers && !checkValidationLayerSupport()) {
			throw new RuntimeException(
					"Validation layers requested but not available!");
		}

		VkApplicationInfo appInfo = VkApplicationInfo.calloc()
				.sType(VK10.VK_STRUCTURE_TYPE_APPLICATION_INFO)
				.apiVersion(VK11.VK_API_VERSION_1_1);

		VkInstanceCreateInfo createInfo = VkInstanceCreateInfo.calloc()
				.sType(VK10.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO)
				.pApplicationInfo(appInfo);

		PointerBuffer extensions = getRequiredExtensions();
		createInfo.ppEnabledExtensionNames(extensions);// extensions

		PointerBuffer layernames = MemoryUtil
				.memCallocPointer(validationLayers.length);// validation layers
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
	public ArrayList<Integer> present = new ArrayList<Integer>();

	public boolean IsComplete() {
		return !graphics.isEmpty() && !present.isEmpty();
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
