package fabricor.main;

import java.io.IOException;
import java.io.InputStream;
import java.net.URISyntaxException;
import java.net.URL;
import java.nio.ByteBuffer;
import java.nio.IntBuffer;
import java.nio.LongBuffer;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.Objects;
import java.util.function.Consumer;

import org.lwjgl.PointerBuffer;
import org.lwjgl.glfw.Callbacks;
import org.lwjgl.glfw.GLFW;
import org.lwjgl.glfw.GLFWCursorPosCallbackI;
import org.lwjgl.glfw.GLFWErrorCallback;
import org.lwjgl.glfw.GLFWFramebufferSizeCallbackI;
import org.lwjgl.glfw.GLFWKeyCallbackI;
import org.lwjgl.glfw.GLFWMouseButtonCallbackI;
import org.lwjgl.glfw.GLFWVulkan;
import org.lwjgl.glfw.GLFWWindowRefreshCallbackI;
import org.lwjgl.system.MemoryStack;
import org.lwjgl.system.MemoryUtil;
import org.lwjgl.vulkan.EXTDebugReport;
import org.lwjgl.vulkan.KHRSurface;
import org.lwjgl.vulkan.KHRSwapchain;
import org.lwjgl.vulkan.VK10;
import org.lwjgl.vulkan.VK11;
import org.lwjgl.vulkan.VkApplicationInfo;
import org.lwjgl.vulkan.VkAttachmentDescription;
import org.lwjgl.vulkan.VkAttachmentReference;
import org.lwjgl.vulkan.VkClearValue;
import org.lwjgl.vulkan.VkCommandBuffer;
import org.lwjgl.vulkan.VkCommandBufferAllocateInfo;
import org.lwjgl.vulkan.VkCommandBufferBeginInfo;
import org.lwjgl.vulkan.VkCommandPoolCreateInfo;
import org.lwjgl.vulkan.VkComponentMapping;
import org.lwjgl.vulkan.VkDebugReportCallbackCreateInfoEXT;
import org.lwjgl.vulkan.VkDebugReportCallbackEXT;
import org.lwjgl.vulkan.VkDebugReportCallbackEXTI;
import org.lwjgl.vulkan.VkDescriptorBufferInfo;
import org.lwjgl.vulkan.VkDescriptorImageInfo;
import org.lwjgl.vulkan.VkDescriptorPoolCreateInfo;
import org.lwjgl.vulkan.VkDescriptorPoolSize;
import org.lwjgl.vulkan.VkDescriptorSetAllocateInfo;
import org.lwjgl.vulkan.VkDescriptorSetLayoutBinding;
import org.lwjgl.vulkan.VkDescriptorSetLayoutCreateInfo;
import org.lwjgl.vulkan.VkDevice;
import org.lwjgl.vulkan.VkDeviceCreateInfo;
import org.lwjgl.vulkan.VkDeviceQueueCreateInfo;
import org.lwjgl.vulkan.VkExtensionProperties;
import org.lwjgl.vulkan.VkExtent2D;
import org.lwjgl.vulkan.VkExtent3D;
import org.lwjgl.vulkan.VkFormatProperties;
import org.lwjgl.vulkan.VkFramebufferCreateInfo;
import org.lwjgl.vulkan.VkGraphicsPipelineCreateInfo;
import org.lwjgl.vulkan.VkImageCopy;
import org.lwjgl.vulkan.VkImageCreateInfo;
import org.lwjgl.vulkan.VkImageMemoryBarrier;
import org.lwjgl.vulkan.VkImageSubresource;
import org.lwjgl.vulkan.VkImageSubresourceLayers;
import org.lwjgl.vulkan.VkImageSubresourceRange;
import org.lwjgl.vulkan.VkImageViewCreateInfo;
import org.lwjgl.vulkan.VkInstance;
import org.lwjgl.vulkan.VkInstanceCreateInfo;
import org.lwjgl.vulkan.VkLayerProperties;
import org.lwjgl.vulkan.VkMemoryAllocateInfo;
import org.lwjgl.vulkan.VkMemoryRequirements;
import org.lwjgl.vulkan.VkOffset2D;
import org.lwjgl.vulkan.VkOffset3D;
import org.lwjgl.vulkan.VkPhysicalDevice;
import org.lwjgl.vulkan.VkPhysicalDeviceFeatures;
import org.lwjgl.vulkan.VkPhysicalDeviceMemoryProperties;
import org.lwjgl.vulkan.VkPhysicalDeviceProperties;
import org.lwjgl.vulkan.VkPipelineCacheCreateInfo;
import org.lwjgl.vulkan.VkPipelineColorBlendAttachmentState;
import org.lwjgl.vulkan.VkPipelineColorBlendStateCreateInfo;
import org.lwjgl.vulkan.VkPipelineDepthStencilStateCreateInfo;
import org.lwjgl.vulkan.VkPipelineDynamicStateCreateInfo;
import org.lwjgl.vulkan.VkPipelineInputAssemblyStateCreateInfo;
import org.lwjgl.vulkan.VkPipelineLayoutCreateInfo;
import org.lwjgl.vulkan.VkPipelineMultisampleStateCreateInfo;
import org.lwjgl.vulkan.VkPipelineRasterizationStateCreateInfo;
import org.lwjgl.vulkan.VkPipelineShaderStageCreateInfo;
import org.lwjgl.vulkan.VkPipelineViewportStateCreateInfo;
import org.lwjgl.vulkan.VkPresentInfoKHR;
import org.lwjgl.vulkan.VkQueue;
import org.lwjgl.vulkan.VkQueueFamilyProperties;
import org.lwjgl.vulkan.VkRect2D;
import org.lwjgl.vulkan.VkRenderPassBeginInfo;
import org.lwjgl.vulkan.VkRenderPassCreateInfo;
import org.lwjgl.vulkan.VkSamplerCreateInfo;
import org.lwjgl.vulkan.VkSemaphoreCreateInfo;
import org.lwjgl.vulkan.VkShaderModuleCreateInfo;
import org.lwjgl.vulkan.VkStencilOpState;
import org.lwjgl.vulkan.VkSubmitInfo;
import org.lwjgl.vulkan.VkSubpassDescription;
import org.lwjgl.vulkan.VkSubresourceLayout;
import org.lwjgl.vulkan.VkSurfaceCapabilitiesKHR;
import org.lwjgl.vulkan.VkSurfaceFormatKHR;
import org.lwjgl.vulkan.VkSwapchainCreateInfoKHR;
import org.lwjgl.vulkan.VkViewport;
import org.lwjgl.vulkan.VkWriteDescriptorSet;

import fabricor.logic.LogicHandler;
import fabricor.logic.input.InputManager;
import fabricor.rendering.MasterRenderer;
import fabricor.rendering.RenderModel;
import fabricor.rendering.ShaderBuffer;

public class Main {

	private static String VK_EXT_DEBUG_REPORT_EXTENSION_NAME = "VK_EXT_debug_report";
	private static String VK_KHR_SWAPCHAIN_EXTENSION_NAME = "VK_KHR_swapchain";
	public static boolean VALIDATE = false;

	private static final boolean USE_STAGING_BUFFER = true;

	private static final int DEMO_TEXTURE_COUNT = 1;

	private static final ByteBuffer KHR_swapchain = MemoryUtil.memUTF8(VK_KHR_SWAPCHAIN_EXTENSION_NAME);
	private static final ByteBuffer EXT_debug_report = MemoryUtil.memUTF8(VK_EXT_DEBUG_REPORT_EXTENSION_NAME);

// buffers for handle output-params
	private final IntBuffer ip = MemoryUtil.memAllocInt(1);
	private final static LongBuffer lp = MemoryUtil.memAllocLong(1);
	private final PointerBuffer pp = MemoryUtil.memAllocPointer(1);

	private PointerBuffer extension_names = MemoryUtil.memAllocPointer(64);

	private VkInstance inst;
	private VkPhysicalDevice gpu;

	private long msg_callback;

	private VkPhysicalDeviceProperties gpu_props = VkPhysicalDeviceProperties.malloc();
	private VkPhysicalDeviceFeatures gpu_features = VkPhysicalDeviceFeatures.malloc();

	private VkQueueFamilyProperties.Buffer queue_props;

	private static int width = 1600;
	private static int height = 900;

	public static int getWidth() {
		return width;
	}

	public static int getHeight() {
		return height;
	}

	private static long window;

	private long surface;

	private int graphics_queue_node_index;

	private static VkDevice device;
	private VkQueue queue;

	private int format;
	private int color_space;

	private static VkPhysicalDeviceMemoryProperties memory_properties = VkPhysicalDeviceMemoryProperties.malloc();

	private long cmd_pool;
	private static VkCommandBuffer draw_cmd;

	private long swapchain;
	private int swapchainImageCount;
	private SwapchainBuffers[] buffers;
	private int current_buffer;

	private VkCommandBuffer setup_cmd;

	private Depth depth = new Depth();

	private TextureObject[] textures = new TextureObject[DEMO_TEXTURE_COUNT];

	private static long desc_layout;
	private static long pipeline_layout;

	private long render_pass;

	private long pipeline;

	private static long desc_pool;
	private static long desc_set;

	private LongBuffer framebuffers;

	private LogicHandler logic = new LogicHandler();

	private Main() {
		for (int i = 0; i < textures.length; i++) {
			textures[i] = new TextureObject();
		}
	}

	private final VkDebugReportCallbackEXT dbgFunc = VkDebugReportCallbackEXT.create(new VkDebugReportCallbackEXTI() {
		public int invoke(int flags, int objectType, long object, long location, int messageCode, long pLayerPrefix,
				long pMessage, long pUserData) {
			String type;
			if ((flags & EXTDebugReport.VK_DEBUG_REPORT_INFORMATION_BIT_EXT) != 0) {
				type = "INFORMATION";
			} else if ((flags & EXTDebugReport.VK_DEBUG_REPORT_WARNING_BIT_EXT) != 0) {
				type = "WARNING";
			} else if ((flags & EXTDebugReport.VK_DEBUG_REPORT_PERFORMANCE_WARNING_BIT_EXT) != 0) {
				type = "PERFORMANCE WARNING";
			} else if ((flags & EXTDebugReport.VK_DEBUG_REPORT_ERROR_BIT_EXT) != 0) {
				type = "ERROR";
			} else if ((flags & EXTDebugReport.VK_DEBUG_REPORT_DEBUG_BIT_EXT) != 0) {
				type = "DEBUG";
			} else {
				type = "UNKNOWN";
			}

			System.err.format("%s: [%s] Code %d : %s\n", type, MemoryUtil.memASCII(pLayerPrefix), messageCode,
					VkDebugReportCallbackEXT.getString(pMessage));

			/*
			 * false indicates that layer should not bail-out of an API call that had
			 * validation failures. This may mean that the app dies inside the driver due to
			 * invalid parameter(s). That's what would happen without validation layers, so
			 * we'll keep that behavior here.
			 */
			return VK10.VK_FALSE;
		}
	});

	public static void check(int errcode) {
		if (errcode != 0) {
			throw new IllegalStateException(String.format("Vulkan error [0x%X]", errcode));
		}
	}

	private static void demo_init_connection() {
		GLFWErrorCallback.createPrint().set();
		if (!GLFW.glfwInit()) {
			throw new IllegalStateException("Unable to initialize GLFW");
		}

		if (!GLFWVulkan.glfwVulkanSupported()) {
			throw new IllegalStateException("Cannot find a compatible Vulkan installable client driver (ICD)");
		}
	}

	/**
	 * Return true if all layer names specified in {@code check_names} can be found
	 * in given {@code layer} properties.
	 */
	private static PointerBuffer demo_check_layers(MemoryStack stack, VkLayerProperties.Buffer available,
			String... layers) {
		PointerBuffer required = stack.mallocPointer(layers.length);
		for (int i = 0; i < layers.length; i++) {
			boolean found = false;

			for (int j = 0; j < available.capacity(); j++) {
				available.position(j);
				if (layers[i].equals(available.layerNameString())) {
					found = true;
					break;
				}
			}

			if (!found) {
				System.err.format("Cannot find layer: %s\n", layers[i]);
				return null;
			}

			required.put(i, stack.ASCII(layers[i]));
		}

		return required;
	}

	private void demo_init_vk() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			PointerBuffer requiredLayers = null;
			if (VALIDATE) {
				check(VK10.vkEnumerateInstanceLayerProperties(ip, null));

				if (ip.get(0) > 0) {
					VkLayerProperties.Buffer availableLayers = VkLayerProperties.mallocStack(ip.get(0), stack);
					check(VK10.vkEnumerateInstanceLayerProperties(ip, availableLayers));

					requiredLayers = demo_check_layers(stack, availableLayers,
							"VK_LAYER_LUNARG_standard_validation"/*
																	 * , "VK_LAYER_LUNARG_assistant_layer"
																	 */
					);
					if (requiredLayers == null) // use alternative set of validation layers
					{
						requiredLayers = demo_check_layers(stack, availableLayers, "VK_LAYER_GOOGLE_threading",
								"VK_LAYER_LUNARG_parameter_validation", "VK_LAYER_LUNARG_object_tracker",
								"VK_LAYER_LUNARG_core_validation",
								"VK_LAYER_GOOGLE_unique_objects"/*
																 * , "VK_LAYER_LUNARG_assistant_layer"
																 */
						);
					}
				}

				if (requiredLayers == null) {
					throw new IllegalStateException(
							"vkEnumerateInstanceLayerProperties failed to find required validation layer.");
				}
			}

			PointerBuffer required_extensions = GLFWVulkan.glfwGetRequiredInstanceExtensions();
			if (required_extensions == null) {
				throw new IllegalStateException(
						"glfwGetRequiredInstanceExtensions failed to find the platform surface extensions.");
			}

			for (int i = 0; i < required_extensions.capacity(); i++) {
				extension_names.put(required_extensions.get(i));
			}

			check(VK10.vkEnumerateInstanceExtensionProperties((String) null, ip, null));

			if (ip.get(0) != 0) {
				VkExtensionProperties.Buffer instance_extensions = VkExtensionProperties.mallocStack(ip.get(0), stack);
				check(VK10.vkEnumerateInstanceExtensionProperties((String) null, ip, instance_extensions));

				for (int i = 0; i < ip.get(0); i++) {
					instance_extensions.position(i);
					if (VK_EXT_DEBUG_REPORT_EXTENSION_NAME.equals(instance_extensions.extensionNameString())) {
						if (VALIDATE) {
							extension_names.put(EXT_debug_report);
						}
					}
				}
			}

			ByteBuffer APP_SHORT_NAME = stack.UTF8("tri");

			VkApplicationInfo app = VkApplicationInfo.mallocStack(stack).sType(VK10.VK_STRUCTURE_TYPE_APPLICATION_INFO)
					.pNext(0).pApplicationName(APP_SHORT_NAME).applicationVersion(1).pEngineName(APP_SHORT_NAME)
					.engineVersion(1).apiVersion(VK11.VK_API_VERSION_1_1);

			extension_names.flip();
			VkInstanceCreateInfo inst_info = VkInstanceCreateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_INSTANCE_CREATE_INFO).pNext(0).flags(0).pApplicationInfo(app)
					.ppEnabledLayerNames(requiredLayers).ppEnabledExtensionNames(extension_names);
			extension_names.clear();

			VkDebugReportCallbackCreateInfoEXT dbgCreateInfo = null;
			if (VALIDATE) {
				dbgCreateInfo = VkDebugReportCallbackCreateInfoEXT.mallocStack(stack)
						.sType(EXTDebugReport.VK_STRUCTURE_TYPE_DEBUG_REPORT_CALLBACK_CREATE_INFO_EXT).pNext(0)
						.flags(EXTDebugReport.VK_DEBUG_REPORT_ERROR_BIT_EXT
								| EXTDebugReport.VK_DEBUG_REPORT_WARNING_BIT_EXT)
						.pfnCallback(dbgFunc).pUserData(0);

				inst_info.pNext(dbgCreateInfo.address());
			}

			int err = VK10.vkCreateInstance(inst_info, null, pp);
			if (err == VK10.VK_ERROR_INCOMPATIBLE_DRIVER) {
				throw new IllegalStateException("Cannot find a compatible Vulkan installable client driver (ICD).");
			} else if (err == VK10.VK_ERROR_EXTENSION_NOT_PRESENT) {
				throw new IllegalStateException(
						"Cannot find a specified extension library. Make sure your layers path is set appropriately.");
			} else if (err != 0) {
				throw new IllegalStateException(
						"vkCreateInstance failed. Do you have a compatible Vulkan installable client driver (ICD) installed?");
			}

			inst = new VkInstance(pp.get(0), inst_info);

			/* Make initial call to query gpu_count, then second call for gpu info */
			check(VK10.vkEnumeratePhysicalDevices(inst, ip, null));

			if (ip.get(0) > 0) {
				PointerBuffer physical_devices = stack.mallocPointer(ip.get(0));
				check(VK10.vkEnumeratePhysicalDevices(inst, ip, physical_devices));

				/* For tri demo we just grab the first physical device */
				gpu = new VkPhysicalDevice(physical_devices.get(0), inst);
			} else {
				throw new IllegalStateException("vkEnumeratePhysicalDevices reported zero accessible devices.");
			}

			/* Look for device extensions */
			boolean swapchainExtFound = false;
			check(VK10.vkEnumerateDeviceExtensionProperties(gpu, (String) null, ip, null));

			if (ip.get(0) > 0) {
				VkExtensionProperties.Buffer device_extensions = VkExtensionProperties.mallocStack(ip.get(0), stack);
				check(VK10.vkEnumerateDeviceExtensionProperties(gpu, (String) null, ip, device_extensions));

				for (int i = 0; i < ip.get(0); i++) {
					device_extensions.position(i);
					if (VK_KHR_SWAPCHAIN_EXTENSION_NAME.equals(device_extensions.extensionNameString())) {
						swapchainExtFound = true;
						extension_names.put(KHR_swapchain);
					}
				}
			}

			if (!swapchainExtFound) {
				throw new IllegalStateException("vkEnumerateDeviceExtensionProperties failed to find the "
						+ VK_KHR_SWAPCHAIN_EXTENSION_NAME + " extension.");
			}

			if (VALIDATE) {
				err = EXTDebugReport.vkCreateDebugReportCallbackEXT(inst, dbgCreateInfo, null, lp);
				switch (err) {
				case VK10.VK_SUCCESS:
					msg_callback = lp.get(0);
					break;
				case VK10.VK_ERROR_OUT_OF_HOST_MEMORY:
					throw new IllegalStateException("CreateDebugReportCallback: out of host memory");
				default:
					throw new IllegalStateException("CreateDebugReportCallback: unknown failure");
				}
			}

			VK10.vkGetPhysicalDeviceProperties(gpu, gpu_props);

			// Query with NULL data to get count
			VK10.vkGetPhysicalDeviceQueueFamilyProperties(gpu, ip, null);

			queue_props = VkQueueFamilyProperties.malloc(ip.get(0));
			VK10.vkGetPhysicalDeviceQueueFamilyProperties(gpu, ip, queue_props);
			if (ip.get(0) == 0) {
				throw new IllegalStateException();
			}

			VK10.vkGetPhysicalDeviceFeatures(gpu, gpu_features);

			// Graphics queue and MemMgr queue can be separate.
			// TODO: Add support for separate queues, including synchronization,
			// and appropriate tracking for QueueSubmit
		}
	}

	private void demo_init() {
		demo_init_connection();
		demo_init_vk();
	}

	private void demo_create_window() {
		GLFW.glfwWindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);

		window = GLFW.glfwCreateWindow(width, height, "Fabricor", 0, 0);
//		GLFW.glfwSetWindowAttrib(window, GLFW.GLFW_DECORATED, GLFW.GLFW_FALSE);

		if (window == 0) {
			throw new IllegalStateException("Cannot create a window in which to draw!");
		}

		GLFW.glfwSetWindowRefreshCallback(window, new GLFWWindowRefreshCallbackI() {
			@Override
			public void invoke(long window) {
				demo_draw();
			}
		});

		GLFW.glfwSetFramebufferSizeCallback(window, new GLFWFramebufferSizeCallbackI() {
			@Override
			public void invoke(long window, int width, int height) {
				Main.width = width;
				Main.height = height;

				if (width != 0 && height != 0) {
					demo_resize();
				}
			}
		});

		GLFW.glfwSetKeyCallback(window, new GLFWKeyCallbackI() {
			@Override
			public void invoke(long window, int key, int scancode, int action, int mods) {
				InputManager.InvokeKey(key, action);
			}
		});

		GLFW.glfwSetCursorPosCallback(window, new GLFWCursorPosCallbackI() {

			@Override
			public void invoke(long window, double xpos, double ypos) {
				InputManager.InvokeMouse(xpos, ypos);
			}
		});

		GLFW.glfwSetMouseButtonCallback(window, new GLFWMouseButtonCallbackI() {

			@Override
			public void invoke(long window, int button, int action, int mods) {
				InputManager.InvokeMouseButton(button, action);
			}
		});

	}

	public static long getWindow() {
		return window;
	}

	private void demo_init_device() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkDeviceQueueCreateInfo.Buffer queue = VkDeviceQueueCreateInfo.mallocStack(1, stack)
					.sType(VK10.VK_STRUCTURE_TYPE_DEVICE_QUEUE_CREATE_INFO).pNext(0).flags(0)
					.queueFamilyIndex(graphics_queue_node_index).pQueuePriorities(stack.floats(0.0f));

			VkPhysicalDeviceFeatures features = VkPhysicalDeviceFeatures.callocStack(stack);
			if (gpu_features.shaderClipDistance()) {
				features.shaderClipDistance(true);
			}

			extension_names.flip();
			VkDeviceCreateInfo device = VkDeviceCreateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_DEVICE_CREATE_INFO).pNext(0).flags(0).pQueueCreateInfos(queue)
					.ppEnabledLayerNames(null).ppEnabledExtensionNames(extension_names).pEnabledFeatures(features);

			System.out.println(device.sType());
			check(VK10.vkCreateDevice(gpu, device, null, pp));

			this.device = new VkDevice(pp.get(0), gpu, device);
		}
	}

	private void demo_init_vk_swapchain() {
		// Create a WSI surface for the window:
		GLFWVulkan.glfwCreateWindowSurface(inst, window, null, lp);
		surface = lp.get(0);

		try (MemoryStack stack = MemoryStack.stackPush()) {
			// Iterate over each queue to learn whether it supports presenting:
			IntBuffer supportsPresent = stack.mallocInt(queue_props.capacity());
			int graphicsQueueNodeIndex;
			int presentQueueNodeIndex;
			for (int i = 0; i < supportsPresent.capacity(); i++) {
				supportsPresent.position(i);
				KHRSurface.vkGetPhysicalDeviceSurfaceSupportKHR(gpu, i, surface, supportsPresent);
			}

			// Search for a graphics and a present queue in the array of queue
			// families, try to find one that supports both
			graphicsQueueNodeIndex = Integer.MAX_VALUE;
			presentQueueNodeIndex = Integer.MAX_VALUE;
			for (int i = 0; i < supportsPresent.capacity(); i++) {
				if ((queue_props.get(i).queueFlags() & VK10.VK_QUEUE_GRAPHICS_BIT) != 0) {
					if (graphicsQueueNodeIndex == Integer.MAX_VALUE) {
						graphicsQueueNodeIndex = i;
					}

					if (supportsPresent.get(i) == VK10.VK_TRUE) {
						graphicsQueueNodeIndex = i;
						presentQueueNodeIndex = i;
						break;
					}
				}
			}
			if (presentQueueNodeIndex == Integer.MAX_VALUE) {
				// If didn't find a queue that supports both graphics and present, then
				// find a separate present queue.
				for (int i = 0; i < supportsPresent.capacity(); ++i) {
					if (supportsPresent.get(i) == VK10.VK_TRUE) {
						presentQueueNodeIndex = i;
						break;
					}
				}
			}

			// Generate error if could not find both a graphics and a present queue
			if (graphicsQueueNodeIndex == Integer.MAX_VALUE || presentQueueNodeIndex == Integer.MAX_VALUE) {
				throw new IllegalStateException("Could not find a graphics and a present queue");
			}

			// TODO: Add support for separate queues, including presentation,
			// synchronization, and appropriate tracking for QueueSubmit.
			// NOTE: While it is possible for an application to use a separate graphics
			// and a present queues, this demo program assumes it is only using
			// one:
			if (graphicsQueueNodeIndex != presentQueueNodeIndex) {
				throw new IllegalStateException("Could not find a common graphics and a present queue");
			}

			graphics_queue_node_index = graphicsQueueNodeIndex;

			demo_init_device();

			VK10.vkGetDeviceQueue(device, graphicsQueueNodeIndex, 0, pp);
			queue = new VkQueue(pp.get(0), device);

			// Get the list of VkFormat's that are supported:
			check(KHRSurface.vkGetPhysicalDeviceSurfaceFormatsKHR(gpu, surface, ip, null));

			VkSurfaceFormatKHR.Buffer surfFormats = VkSurfaceFormatKHR.mallocStack(ip.get(0), stack);
			check(KHRSurface.vkGetPhysicalDeviceSurfaceFormatsKHR(gpu, surface, ip, surfFormats));

			// If the format list includes just one entry of VK_FORMAT_UNDEFINED,
			// the surface has no preferred format. Otherwise, at least one
			// supported format will be returned.
			if (ip.get(0) == 1 && surfFormats.get(0).format() == VK10.VK_FORMAT_UNDEFINED) {
				format = VK10.VK_FORMAT_B8G8R8A8_UNORM;
			} else {
				assert ip.get(0) >= 1;
				format = surfFormats.get(0).format();
			}
			color_space = surfFormats.get(0).colorSpace();

			// Get Memory information and properties
			VK10.vkGetPhysicalDeviceMemoryProperties(gpu, memory_properties);
		}
	}

	private static class SwapchainBuffers {
		long image;
		VkCommandBuffer cmd;
		long view;
	}

	private void demo_set_image_layout(long image, final int aspectMask, int old_image_layout, int new_image_layout,
			int srcAccessMask) {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			if (setup_cmd == null) {
				VkCommandBufferAllocateInfo cmd = VkCommandBufferAllocateInfo.mallocStack(stack)
						.sType(VK10.VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO).pNext(0).commandPool(cmd_pool)
						.level(VK10.VK_COMMAND_BUFFER_LEVEL_PRIMARY).commandBufferCount(1);

				check(VK10.vkAllocateCommandBuffers(device, cmd, pp));
				setup_cmd = new VkCommandBuffer(pp.get(0), device);

				VkCommandBufferBeginInfo cmd_buf_info = VkCommandBufferBeginInfo.mallocStack(stack)
						.sType(VK10.VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO).pNext(0).flags(0)
						.pInheritanceInfo(null);
				check(VK10.vkBeginCommandBuffer(setup_cmd, cmd_buf_info));
			}

			VkImageMemoryBarrier.Buffer image_memory_barrier = VkImageMemoryBarrier.mallocStack(1, stack)
					.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER).pNext(0).srcAccessMask(srcAccessMask)
					.dstAccessMask(0).oldLayout(old_image_layout).newLayout(new_image_layout).srcQueueFamilyIndex(0)
					.dstQueueFamilyIndex(0).image(image).subresourceRange(new Consumer<VkImageSubresourceRange>() {
						@Override
						public void accept(VkImageSubresourceRange it) {
							it.aspectMask(aspectMask).baseMipLevel(0).levelCount(1).baseArrayLayer(0).layerCount(1);
						}
					});

			switch (new_image_layout) {
			case VK10.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL:
				image_memory_barrier.dstAccessMask(VK10.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT);
				break;
			case VK10.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL:
				image_memory_barrier.dstAccessMask(VK10.VK_ACCESS_DEPTH_STENCIL_ATTACHMENT_WRITE_BIT);
				break;
			case VK10.VK_IMAGE_LAYOUT_SHADER_READ_ONLY_OPTIMAL:
				/* Make sure any Copy or CPU writes to image are flushed */
				image_memory_barrier
						.dstAccessMask(VK10.VK_ACCESS_SHADER_READ_BIT | VK10.VK_ACCESS_INPUT_ATTACHMENT_READ_BIT);
				break;
			case VK10.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL:
				image_memory_barrier.srcAccessMask(VK10.VK_ACCESS_MEMORY_READ_BIT);
				/* Make sure anything that was copying from this image has completed */
				image_memory_barrier.dstAccessMask(VK10.VK_ACCESS_TRANSFER_READ_BIT);
				break;
			case KHRSwapchain.VK_IMAGE_LAYOUT_PRESENT_SRC_KHR:
				image_memory_barrier.dstAccessMask(VK10.VK_ACCESS_MEMORY_READ_BIT);
				break;
			}

			int src_stages = VK10.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;
			int dest_stages = VK10.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT;

			VK10.vkCmdPipelineBarrier(setup_cmd, src_stages, dest_stages, 0, null, null, image_memory_barrier);
		}
	}

	private void demo_prepare_buffers() {
		long oldSwapchain = swapchain;

		try (MemoryStack stack = MemoryStack.stackPush()) {
			// Check the surface capabilities and formats
			VkSurfaceCapabilitiesKHR surfCapabilities = VkSurfaceCapabilitiesKHR.mallocStack(stack);
			check(KHRSurface.vkGetPhysicalDeviceSurfaceCapabilitiesKHR(gpu, surface, surfCapabilities));

			check(KHRSurface.vkGetPhysicalDeviceSurfacePresentModesKHR(gpu, surface, ip, null));

			IntBuffer presentModes = stack.mallocInt(ip.get(0));
			check(KHRSurface.vkGetPhysicalDeviceSurfacePresentModesKHR(gpu, surface, ip, presentModes));

			VkExtent2D swapchainExtent = VkExtent2D.mallocStack(stack);
			// width and height are either both 0xFFFFFFFF, or both not 0xFFFFFFFF.
			if (surfCapabilities.currentExtent().width() == 0xFFFFFFFF) {
				// If the surface size is undefined, the size is set to the size
				// of the images requested, which must fit within the minimum and
				// maximum values.
				swapchainExtent.width(width);
				swapchainExtent.height(height);

				if (swapchainExtent.width() < surfCapabilities.minImageExtent().width()) {
					swapchainExtent.width(surfCapabilities.minImageExtent().width());
				} else if (swapchainExtent.width() > surfCapabilities.maxImageExtent().width()) {
					swapchainExtent.width(surfCapabilities.maxImageExtent().width());
				}

				if (swapchainExtent.height() < surfCapabilities.minImageExtent().height()) {
					swapchainExtent.height(surfCapabilities.minImageExtent().height());
				} else if (swapchainExtent.height() > surfCapabilities.maxImageExtent().height()) {
					swapchainExtent.height(surfCapabilities.maxImageExtent().height());
				}
			} else {
				// If the surface size is defined, the swap chain size must match
				swapchainExtent.set(surfCapabilities.currentExtent());
				width = surfCapabilities.currentExtent().width();
				height = surfCapabilities.currentExtent().height();
			}

			int swapchainPresentMode = KHRSurface.VK_PRESENT_MODE_FIFO_KHR;

			// Determine the number of VkImage's to use in the swap chain.
			// Application desires to only acquire 1 image at a time (which is
			// "surfCapabilities.minImageCount").
			int desiredNumOfSwapchainImages = surfCapabilities.minImageCount();
			// If maxImageCount is 0, we can ask for as many images as we want;
			// otherwise we're limited to maxImageCount
			if ((surfCapabilities.maxImageCount() > 0)
					&& (desiredNumOfSwapchainImages > surfCapabilities.maxImageCount())) {
				// Application must settle for fewer images than desired:
				desiredNumOfSwapchainImages = surfCapabilities.maxImageCount();
			}

			int preTransform;
			if ((surfCapabilities.supportedTransforms() & KHRSurface.VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR) != 0) {
				preTransform = KHRSurface.VK_SURFACE_TRANSFORM_IDENTITY_BIT_KHR;
			} else {
				preTransform = surfCapabilities.currentTransform();
			}

			VkSwapchainCreateInfoKHR swapchain = VkSwapchainCreateInfoKHR.callocStack(stack)
					.sType(KHRSwapchain.VK_STRUCTURE_TYPE_SWAPCHAIN_CREATE_INFO_KHR).surface(surface)
					.minImageCount(desiredNumOfSwapchainImages).imageFormat(format).imageColorSpace(color_space)
					.imageExtent(swapchainExtent).imageArrayLayers(1)
					.imageUsage(VK10.VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT)
					.imageSharingMode(VK10.VK_SHARING_MODE_EXCLUSIVE).preTransform(preTransform)
					.compositeAlpha(KHRSurface.VK_COMPOSITE_ALPHA_OPAQUE_BIT_KHR).presentMode(swapchainPresentMode)
					.clipped(true).oldSwapchain(oldSwapchain);

			check(KHRSwapchain.vkCreateSwapchainKHR(device, swapchain, null, lp));
			this.swapchain = lp.get(0);

			// If we just re-created an existing swapchain, we should destroy the old
			// swapchain at this point.
			// Note: destroying the swapchain also cleans up all its associated
			// presentable images once the platform is done with them.
			if (oldSwapchain != VK10.VK_NULL_HANDLE) {
				KHRSwapchain.vkDestroySwapchainKHR(device, oldSwapchain, null);
			}

			check(KHRSwapchain.vkGetSwapchainImagesKHR(device, this.swapchain, ip, null));
			swapchainImageCount = ip.get(0);

			LongBuffer swapchainImages = stack.mallocLong(swapchainImageCount);
			check(KHRSwapchain.vkGetSwapchainImagesKHR(device, this.swapchain, ip, swapchainImages));

			buffers = new SwapchainBuffers[swapchainImageCount];

			for (int i = 0; i < swapchainImageCount; i++) {
				buffers[i] = new SwapchainBuffers();
				buffers[i].image = swapchainImages.get(i);

				VkImageViewCreateInfo color_attachment_view = VkImageViewCreateInfo.mallocStack(stack)
						.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO).pNext(0).flags(0).image(buffers[i].image)
						.viewType(VK10.VK_IMAGE_VIEW_TYPE_2D).format(format)
						.components(new Consumer<VkComponentMapping>() {
							@Override
							public void accept(VkComponentMapping it) {
								it.r(VK10.VK_COMPONENT_SWIZZLE_R).g(VK10.VK_COMPONENT_SWIZZLE_G)
										.b(VK10.VK_COMPONENT_SWIZZLE_B).a(VK10.VK_COMPONENT_SWIZZLE_A);
							}
						}).subresourceRange(new Consumer<VkImageSubresourceRange>() {
							@Override
							public void accept(VkImageSubresourceRange it) {
								it.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).baseMipLevel(0).levelCount(1)
										.baseArrayLayer(0).layerCount(1);
							}
						});

				check(VK10.vkCreateImageView(device, color_attachment_view, null, lp));
				buffers[i].view = lp.get(0);
			}

			current_buffer = 0;
		}
	}

	private static class Depth {
		int format;

		long image;
		long mem;
		long view;
	}

	public static boolean memory_type_from_properties(int typeBits, int requirements_mask,
			VkMemoryAllocateInfo mem_alloc) {
		// Search memtypes to find first index with those properties
		for (int i = 0; i < VK10.VK_MAX_MEMORY_TYPES; i++) {
			if ((typeBits & 1) == 1) {
				// Type is available, does it match user properties?
				if ((memory_properties.memoryTypes().get(i).propertyFlags() & requirements_mask) == requirements_mask) {
					mem_alloc.memoryTypeIndex(i);
					return true;
				}
			}
			typeBits >>= 1;
		}
		// No memory types matched, return failure
		return false;
	}

	private void demo_prepare_depth() {
		depth.format = VK10.VK_FORMAT_D16_UNORM;

		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkImageCreateInfo image = VkImageCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO).pNext(0).imageType(VK10.VK_IMAGE_TYPE_2D)
					.format(depth.format).extent(new Consumer<VkExtent3D>() {
						@Override
						public void accept(VkExtent3D it) {
							it.width(width).height(height).depth(1);
						}
					}).mipLevels(1).arrayLayers(1).samples(VK10.VK_SAMPLE_COUNT_1_BIT)
					.tiling(VK10.VK_IMAGE_TILING_OPTIMAL).usage(VK10.VK_IMAGE_USAGE_DEPTH_STENCIL_ATTACHMENT_BIT);

			/* create image */
			check(VK10.vkCreateImage(device, image, null, lp));
			depth.image = lp.get(0);

			/* get memory requirements for this object */
			VkMemoryRequirements mem_reqs = VkMemoryRequirements.mallocStack(stack);
			VK10.vkGetImageMemoryRequirements(device, depth.image, mem_reqs);

			/* select memory size and type */
			VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO).pNext(0).allocationSize(mem_reqs.size())
					.memoryTypeIndex(0);
			boolean pass = memory_type_from_properties(mem_reqs.memoryTypeBits(), 0, /* No requirements */
					mem_alloc);
			assert (pass);

			/* allocate memory */
			check(VK10.vkAllocateMemory(device, mem_alloc, null, lp));
			depth.mem = lp.get(0);

			/* bind memory */
			check(VK10.vkBindImageMemory(device, depth.image, depth.mem, 0));

			demo_set_image_layout(depth.image, VK10.VK_IMAGE_ASPECT_DEPTH_BIT, VK10.VK_IMAGE_LAYOUT_UNDEFINED,
					VK10.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL, 0);

			/* create image view */
			VkImageViewCreateInfo view = VkImageViewCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO).pNext(0).flags(0).image(depth.image)
					.viewType(VK10.VK_IMAGE_VIEW_TYPE_2D).format(depth.format)
					.subresourceRange(new Consumer<VkImageSubresourceRange>() {
						@Override
						public void accept(VkImageSubresourceRange it) {
							it.aspectMask(VK10.VK_IMAGE_ASPECT_DEPTH_BIT).baseMipLevel(0).levelCount(1)
									.baseArrayLayer(0).layerCount(1);
						}
					});

			check(VK10.vkCreateImageView(device, view, null, lp));
			depth.view = lp.get(0);
		}
	}

	private static class TextureObject {
		long sampler;

		long image;
		int imageLayout;

		long mem;
		long view;
		int tex_width, tex_height;
	}

	private void demo_prepare_texture_image(int[] tex_colors, TextureObject tex_obj, int tiling, int usage,
			int required_props) {
		int tex_format = VK10.VK_FORMAT_B8G8R8A8_UNORM;

		final int tex_width = 2;
		final int tex_height = 2;

		boolean pass;

		tex_obj.tex_width = tex_width;
		tex_obj.tex_height = tex_height;

		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkImageCreateInfo image_create_info = VkImageCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_CREATE_INFO).pNext(0).imageType(VK10.VK_IMAGE_TYPE_2D)
					.format(tex_format).extent(new Consumer<VkExtent3D>() {
						@Override
						public void accept(VkExtent3D it) {
							it.width(tex_width).height(tex_height).depth(1);
						}
					}).mipLevels(1).arrayLayers(1).samples(VK10.VK_SAMPLE_COUNT_1_BIT).tiling(tiling).usage(usage)
					.flags(0).initialLayout(VK10.VK_IMAGE_LAYOUT_PREINITIALIZED);

			check(VK10.vkCreateImage(device, image_create_info, null, lp));
			tex_obj.image = lp.get(0);

			VkMemoryRequirements mem_reqs = VkMemoryRequirements.mallocStack(stack);
			VK10.vkGetImageMemoryRequirements(device, tex_obj.image, mem_reqs);
			VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_MEMORY_ALLOCATE_INFO).pNext(0).allocationSize(mem_reqs.size())
					.memoryTypeIndex(0);
			pass = memory_type_from_properties(mem_reqs.memoryTypeBits(), required_props, mem_alloc);
			assert (pass);

			/* allocate memory */
			check(VK10.vkAllocateMemory(device, mem_alloc, null, lp));
			tex_obj.mem = lp.get(0);

			/* bind memory */
			check(VK10.vkBindImageMemory(device, tex_obj.image, tex_obj.mem, 0));

			if ((required_props & VK10.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT) != 0) {
				VkImageSubresource subres = VkImageSubresource.mallocStack(stack)
						.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).mipLevel(0).arrayLayer(0);

				VkSubresourceLayout layout = VkSubresourceLayout.mallocStack(stack);
				VK10.vkGetImageSubresourceLayout(device, tex_obj.image, subres, layout);

				check(VK10.vkMapMemory(device, tex_obj.mem, 0, mem_alloc.allocationSize(), 0, pp));

				for (int y = 0; y < tex_height; y++) {
					IntBuffer row = MemoryUtil.memIntBuffer(pp.get(0) + layout.rowPitch() * y, tex_width);
					for (int x = 0; x < tex_width; x++) {
						row.put(x, tex_colors[(x & 1) ^ (y & 1)]);
					}
				}

				VK10.vkUnmapMemory(device, tex_obj.mem);
			}

			tex_obj.imageLayout = VK10.VK_IMAGE_LAYOUT_GENERAL;
			demo_set_image_layout(tex_obj.image, VK10.VK_IMAGE_ASPECT_COLOR_BIT, VK10.VK_IMAGE_LAYOUT_PREINITIALIZED,
					tex_obj.imageLayout, VK10.VK_ACCESS_HOST_WRITE_BIT);
			/*
			 * setting the image layout does not reference the actual memory so no need to
			 * add a mem ref
			 */
		}
	}

	private void demo_destroy_texture_image(TextureObject tex_obj) {
		/* clean up staging resources */
		VK10.vkDestroyImage(device, tex_obj.image, null);
		VK10.vkFreeMemory(device, tex_obj.mem, null);
	}

	private void demo_flush_init_cmd() {
		if (setup_cmd == null) {
			return;
		}

		check(VK10.vkEndCommandBuffer(setup_cmd));

		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkSubmitInfo submit_info = VkSubmitInfo.callocStack(stack).sType(VK10.VK_STRUCTURE_TYPE_SUBMIT_INFO)
					.pCommandBuffers(pp.put(0, setup_cmd));

			check(VK10.vkQueueSubmit(queue, submit_info, VK10.VK_NULL_HANDLE));
		}

		check(VK10.vkQueueWaitIdle(queue));

		VK10.vkFreeCommandBuffers(device, cmd_pool, pp);
		setup_cmd = null;
	}

	private void demo_prepare_textures() {
		int tex_format = VK10.VK_FORMAT_B8G8R8A8_UNORM;

		int[][] tex_colors = { { 0xffff0000, 0xff00ff00 } };

		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkFormatProperties props = VkFormatProperties.mallocStack(stack);
			VK10.vkGetPhysicalDeviceFormatProperties(gpu, tex_format, props);

			for (int i = 0; i < DEMO_TEXTURE_COUNT; i++) {
				if ((props.linearTilingFeatures() & VK10.VK_FORMAT_FEATURE_SAMPLED_IMAGE_BIT) != 0
						&& !USE_STAGING_BUFFER) {
					/* Device can texture using linear textures */
					demo_prepare_texture_image(tex_colors[i], textures[i], VK10.VK_IMAGE_TILING_LINEAR,
							VK10.VK_IMAGE_USAGE_SAMPLED_BIT,
							VK10.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK10.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);
				} else if ((props.optimalTilingFeatures() & VK10.VK_FORMAT_FEATURE_SAMPLED_IMAGE_BIT) != 0) {
					/* Must use staging buffer to copy linear texture to optimized */
					final TextureObject staging_texture = new TextureObject();

					demo_prepare_texture_image(tex_colors[i], staging_texture, VK10.VK_IMAGE_TILING_LINEAR,
							VK10.VK_IMAGE_USAGE_TRANSFER_SRC_BIT,
							VK10.VK_MEMORY_PROPERTY_HOST_VISIBLE_BIT | VK10.VK_MEMORY_PROPERTY_HOST_COHERENT_BIT);

					demo_prepare_texture_image(tex_colors[i], textures[i], VK10.VK_IMAGE_TILING_OPTIMAL,
							(VK10.VK_IMAGE_USAGE_TRANSFER_DST_BIT | VK10.VK_IMAGE_USAGE_SAMPLED_BIT),
							VK10.VK_MEMORY_PROPERTY_DEVICE_LOCAL_BIT);

					demo_set_image_layout(staging_texture.image, VK10.VK_IMAGE_ASPECT_COLOR_BIT,
							staging_texture.imageLayout, VK10.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL, 0);

					demo_set_image_layout(textures[i].image, VK10.VK_IMAGE_ASPECT_COLOR_BIT, textures[i].imageLayout,
							VK10.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, 0);

					VkImageCopy.Buffer copy_region = VkImageCopy.mallocStack(1, stack)
							.srcSubresource(new Consumer<VkImageSubresourceLayers>() {
								@Override
								public void accept(VkImageSubresourceLayers it) {
									it.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).mipLevel(0).baseArrayLayer(0)
											.layerCount(1);
								}
							}).srcOffset(new Consumer<VkOffset3D>() {
								@Override
								public void accept(VkOffset3D it) {
									it.x(0).y(0).z(0);
								}
							}).dstSubresource(new Consumer<VkImageSubresourceLayers>() {
								@Override
								public void accept(VkImageSubresourceLayers it) {
									it.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).mipLevel(0).baseArrayLayer(0)
											.layerCount(1);
								}
							}).dstOffset(new Consumer<VkOffset3D>() {
								@Override
								public void accept(VkOffset3D it) {
									it.x(0).y(0).z(0);
								}
							}).extent(new Consumer<VkExtent3D>() {
								@Override
								public void accept(VkExtent3D it) {
									it.width(staging_texture.tex_width).height(staging_texture.tex_height).depth(1);
								}
							});

					VK10.vkCmdCopyImage(setup_cmd, staging_texture.image, VK10.VK_IMAGE_LAYOUT_TRANSFER_SRC_OPTIMAL,
							textures[i].image, VK10.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, copy_region);

					demo_set_image_layout(textures[i].image, VK10.VK_IMAGE_ASPECT_COLOR_BIT,
							VK10.VK_IMAGE_LAYOUT_TRANSFER_DST_OPTIMAL, textures[i].imageLayout, 0);

					demo_flush_init_cmd();

					demo_destroy_texture_image(staging_texture);
				} else {
					/* Can't support VK_FORMAT_B8G8R8A8_UNORM !? */
					throw new IllegalStateException("No support for B8G8R8A8_UNORM as texture image format");
				}

				VkSamplerCreateInfo sampler = VkSamplerCreateInfo.callocStack(stack)
						.sType(VK10.VK_STRUCTURE_TYPE_SAMPLER_CREATE_INFO).pNext(0).magFilter(VK10.VK_FILTER_NEAREST)
						.minFilter(VK10.VK_FILTER_NEAREST).mipmapMode(VK10.VK_SAMPLER_MIPMAP_MODE_NEAREST)
						.addressModeU(VK10.VK_SAMPLER_ADDRESS_MODE_REPEAT)
						.addressModeV(VK10.VK_SAMPLER_ADDRESS_MODE_REPEAT)
						.addressModeW(VK10.VK_SAMPLER_ADDRESS_MODE_REPEAT).mipLodBias(0.0f).anisotropyEnable(false)
						.maxAnisotropy(1).compareOp(VK10.VK_COMPARE_OP_NEVER).minLod(0.0f).maxLod(0.0f)
						.borderColor(VK10.VK_BORDER_COLOR_FLOAT_OPAQUE_WHITE).unnormalizedCoordinates(false);

				/* create sampler */
				check(VK10.vkCreateSampler(device, sampler, null, lp));
				textures[i].sampler = lp.get(0);

				VkImageViewCreateInfo view = VkImageViewCreateInfo.mallocStack(stack)
						.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_VIEW_CREATE_INFO).pNext(0).image(VK10.VK_NULL_HANDLE)
						.viewType(VK10.VK_IMAGE_VIEW_TYPE_2D).format(tex_format).flags(0)
						.components(new Consumer<VkComponentMapping>() {
							@Override
							public void accept(VkComponentMapping it) {
								it.r(VK10.VK_COMPONENT_SWIZZLE_R).g(VK10.VK_COMPONENT_SWIZZLE_G)
										.b(VK10.VK_COMPONENT_SWIZZLE_B).a(VK10.VK_COMPONENT_SWIZZLE_A);
							}
						}).subresourceRange(new Consumer<VkImageSubresourceRange>() {
							@Override
							public void accept(VkImageSubresourceRange it) {
								it.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).baseMipLevel(0).levelCount(1)
										.baseArrayLayer(0).layerCount(1);
							}
						});

				/* create image view */
				view.image(textures[i].image);
				check(VK10.vkCreateImageView(device, view, null, lp));
				textures[i].view = lp.get(0);
			}
		}
	}

	private void demo_prepare_descriptor_layout() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkDescriptorSetLayoutCreateInfo descriptor_layout = VkDescriptorSetLayoutCreateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO).pNext(0).flags(0)
					.pBindings(VkDescriptorSetLayoutBinding.callocStack(1, stack).binding(0)
							.descriptorType(VK10.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC).descriptorCount(1)
							.stageFlags(VK10.VK_SHADER_STAGE_VERTEX_BIT));

			LongBuffer layouts = stack.mallocLong(1);
			check(VK10.vkCreateDescriptorSetLayout(device, descriptor_layout, null, layouts));
			desc_layout = layouts.get(0);
			/*
			 * descriptor_layout = VkDescriptorSetLayoutCreateInfo.mallocStack(stack)
			 * .sType(VK10.VK_STRUCTURE_TYPE_DESCRIPTOR_SET_LAYOUT_CREATE_INFO) .pNext(0)
			 * .flags(0) .pBindings( VkDescriptorSetLayoutBinding.callocStack(1, stack)
			 * .binding(1) .descriptorType(VK10.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC)
			 * .descriptorCount(1) .stageFlags(VK10.VK_SHADER_STAGE_VERTEX_BIT) );
			 * 
			 * layouts = stack.mallocLong(1); check(VK10.vkCreateDescriptorSetLayout(device,
			 * descriptor_layout, null, layouts)); uniform_layout = layouts.get(0);
			 * 
			 * layouts= stack.mallocLong(2); layouts.put(desc_layout);
			 * layouts.put(uniform_layout);
			 * 
			 */
			VkPipelineLayoutCreateInfo pPipelineLayoutCreateInfo = VkPipelineLayoutCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_LAYOUT_CREATE_INFO).pNext(0).pSetLayouts(layouts);

			check(VK10.vkCreatePipelineLayout(device, pPipelineLayoutCreateInfo, null, lp));
			pipeline_layout = lp.get(0);
		}
	}

	private void demo_prepare_render_pass() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkAttachmentDescription.Buffer attachments = VkAttachmentDescription.mallocStack(2, stack);
			attachments.get(0).flags(0).format(format).samples(VK10.VK_SAMPLE_COUNT_1_BIT)
					.loadOp(VK10.VK_ATTACHMENT_LOAD_OP_CLEAR).storeOp(VK10.VK_ATTACHMENT_STORE_OP_STORE)
					.stencilLoadOp(VK10.VK_ATTACHMENT_LOAD_OP_DONT_CARE)
					.stencilStoreOp(VK10.VK_ATTACHMENT_STORE_OP_DONT_CARE)
					.initialLayout(VK10.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
					.finalLayout(VK10.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL);
			attachments.get(1).flags(0).format(depth.format).samples(VK10.VK_SAMPLE_COUNT_1_BIT)
					.loadOp(VK10.VK_ATTACHMENT_LOAD_OP_CLEAR).storeOp(VK10.VK_ATTACHMENT_STORE_OP_DONT_CARE)
					.stencilLoadOp(VK10.VK_ATTACHMENT_LOAD_OP_DONT_CARE)
					.stencilStoreOp(VK10.VK_ATTACHMENT_STORE_OP_DONT_CARE)
					.initialLayout(VK10.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL)
					.finalLayout(VK10.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL);

			VkSubpassDescription.Buffer subpass = VkSubpassDescription.callocStack(1, stack)
					.pipelineBindPoint(VK10.VK_PIPELINE_BIND_POINT_GRAPHICS).colorAttachmentCount(1)
					.pColorAttachments(VkAttachmentReference.mallocStack(1, stack).attachment(0)
							.layout(VK10.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL))
					.pDepthStencilAttachment(VkAttachmentReference.mallocStack(stack).attachment(1)
							.layout(VK10.VK_IMAGE_LAYOUT_DEPTH_STENCIL_ATTACHMENT_OPTIMAL));

			VkRenderPassCreateInfo rp_info = VkRenderPassCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_RENDER_PASS_CREATE_INFO).pAttachments(attachments)
					.pSubpasses(subpass);

			check(VK10.vkCreateRenderPass(device, rp_info, null, lp));
			render_pass = lp.get(0);
		}
	}

	private static long[] loadShaderFromClasspath(String name, VkDevice dev) {
		String path = "/" + name + "/";
		InputStream vert = Main.class.getResourceAsStream(path + "vert.spv");
		InputStream frag = Main.class.getResourceAsStream(path + "frag.spv");

		byte[] bytevert = new byte[1];
		byte[] bytefrag = new byte[1];
		try {
			bytevert = vert.readAllBytes();
			bytefrag = frag.readAllBytes();
		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}

		ByteBuffer vertbuff = MemoryUtil.memCalloc(bytevert.length);

		ByteBuffer fragbuff = MemoryUtil.memCalloc(bytefrag.length);

		for (int i = 0; i < bytevert.length; i++) {
			vertbuff.put(bytevert[i]);
		}
		vertbuff.flip();

		for (int i = 0; i < bytefrag.length; i++) {
			fragbuff.put(bytefrag[i]);
		}
		fragbuff.flip();

		long[] modules = new long[2];

		int err;
		VkShaderModuleCreateInfo moduleCreateInfo = VkShaderModuleCreateInfo.calloc()
				.sType(VK10.VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO).pNext(0).pCode(vertbuff).flags(0);
		LongBuffer pShaderModule = MemoryUtil.memAllocLong(1);
		err = VK10.vkCreateShaderModule(dev, moduleCreateInfo, null, pShaderModule);
		modules[0] = pShaderModule.get(0);
		MemoryUtil.memFree(pShaderModule);
		if (err != VK10.VK_SUCCESS) {
			throw new AssertionError("Failed to create vertex shader module: " + err);
		}

		moduleCreateInfo = VkShaderModuleCreateInfo.calloc().sType(VK10.VK_STRUCTURE_TYPE_SHADER_MODULE_CREATE_INFO)
				.pNext(0).pCode(fragbuff).flags(0);
		pShaderModule = MemoryUtil.memAllocLong(1);
		err = VK10.vkCreateShaderModule(dev, moduleCreateInfo, null, pShaderModule);
		modules[1] = pShaderModule.get(0);
		MemoryUtil.memFree(pShaderModule);
		if (err != VK10.VK_SUCCESS) {
			throw new AssertionError("Failed to create fragment shader module: " + err);
		}

		return modules;
	}

	private void demo_prepare_pipeline() {
		long vert_shader_module;
		long frag_shader_module;
		long pipelineCache;

		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkGraphicsPipelineCreateInfo.Buffer pipeline = VkGraphicsPipelineCreateInfo.callocStack(1, stack);

			// Two stages: vs and fs
			ByteBuffer main = stack.UTF8("main");

			long[] shaders = loadShaderFromClasspath("triangle", device);

			VkPipelineShaderStageCreateInfo.Buffer shaderStages = VkPipelineShaderStageCreateInfo.callocStack(2, stack);
			shaderStages.get(0).sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO)
					.stage(VK10.VK_SHADER_STAGE_VERTEX_BIT).module(vert_shader_module = shaders[0]).pName(main);
			shaderStages.get(1).sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_SHADER_STAGE_CREATE_INFO)
					.stage(VK10.VK_SHADER_STAGE_FRAGMENT_BIT).module(frag_shader_module = shaders[1]).pName(main);

			VkPipelineDepthStencilStateCreateInfo ds = VkPipelineDepthStencilStateCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_DEPTH_STENCIL_STATE_CREATE_INFO).depthTestEnable(true)
					.depthWriteEnable(true).depthCompareOp(VK10.VK_COMPARE_OP_LESS_OR_EQUAL)
					.depthBoundsTestEnable(false).stencilTestEnable(false).back(new Consumer<VkStencilOpState>() {
						@Override
						public void accept(VkStencilOpState it) {
							it.failOp(VK10.VK_STENCIL_OP_KEEP).passOp(VK10.VK_STENCIL_OP_REPLACE)
									.compareOp(VK10.VK_COMPARE_OP_ALWAYS);
						}
					});
			ds.front(ds.back());

			pipeline.sType(VK10.VK_STRUCTURE_TYPE_GRAPHICS_PIPELINE_CREATE_INFO).pStages(shaderStages)
					.pVertexInputState(RenderModel.getVi())
					.pInputAssemblyState(VkPipelineInputAssemblyStateCreateInfo.callocStack(stack)
							.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_INPUT_ASSEMBLY_STATE_CREATE_INFO)
							.topology(VK10.VK_PRIMITIVE_TOPOLOGY_TRIANGLE_LIST))
					.pViewportState(VkPipelineViewportStateCreateInfo.callocStack(stack)
							.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_VIEWPORT_STATE_CREATE_INFO).viewportCount(1)
							.scissorCount(1))
					.pRasterizationState(VkPipelineRasterizationStateCreateInfo.callocStack(stack)
							.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_RASTERIZATION_STATE_CREATE_INFO)
							.polygonMode(VK10.VK_POLYGON_MODE_FILL).cullMode(VK10.VK_CULL_MODE_BACK_BIT)
							.frontFace(VK10.VK_FRONT_FACE_CLOCKWISE).depthClampEnable(false)
							.rasterizerDiscardEnable(false).depthBiasEnable(false).lineWidth(1.0f))
					.pMultisampleState(VkPipelineMultisampleStateCreateInfo.callocStack(stack)
							.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_MULTISAMPLE_STATE_CREATE_INFO).pSampleMask(null)
							.rasterizationSamples(VK10.VK_SAMPLE_COUNT_1_BIT))
					.pDepthStencilState(ds)
					.pColorBlendState(VkPipelineColorBlendStateCreateInfo.callocStack(stack)
							.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_COLOR_BLEND_STATE_CREATE_INFO)
							.pAttachments(VkPipelineColorBlendAttachmentState.callocStack(1, stack).colorWriteMask(0xf)
									.blendEnable(false)))
					.pDynamicState(VkPipelineDynamicStateCreateInfo.callocStack(stack)
							.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_DYNAMIC_STATE_CREATE_INFO)
							.pDynamicStates(stack.ints(VK10.VK_DYNAMIC_STATE_VIEWPORT, VK10.VK_DYNAMIC_STATE_SCISSOR)))
					.layout(pipeline_layout).renderPass(render_pass);

			VkPipelineCacheCreateInfo pipelineCacheCI = VkPipelineCacheCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_PIPELINE_CACHE_CREATE_INFO);

			check(VK10.vkCreatePipelineCache(device, pipelineCacheCI, null, lp));
			pipelineCache = lp.get(0);

			check(VK10.vkCreateGraphicsPipelines(device, pipelineCache, pipeline, null, lp));
			this.pipeline = lp.get(0);

			VK10.vkDestroyPipelineCache(device, pipelineCache, null);

			VK10.vkDestroyShaderModule(device, frag_shader_module, null);
			VK10.vkDestroyShaderModule(device, vert_shader_module, null);
		}
	}

	private void demo_prepare_descriptor_pool() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkDescriptorPoolCreateInfo descriptor_pool = VkDescriptorPoolCreateInfo.callocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_DESCRIPTOR_POOL_CREATE_INFO).pNext(0).maxSets(1)
					.pPoolSizes(VkDescriptorPoolSize.mallocStack(1, stack)
							.type(VK10.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC).descriptorCount(1));

			check(VK10.vkCreateDescriptorPool(device, descriptor_pool, null, lp));
			desc_pool = lp.get(0);
		}
	}

	private void demo_prepare_descriptor_set() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			LongBuffer layouts = stack.longs(desc_layout);
			VkDescriptorSetAllocateInfo alloc_info = VkDescriptorSetAllocateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_DESCRIPTOR_SET_ALLOCATE_INFO).pNext(0).descriptorPool(desc_pool)
					.pSetLayouts(layouts);

			check(VK10.vkAllocateDescriptorSets(device, alloc_info, lp));
			desc_set = lp.get(0);

			ShaderBuffer shbuff = MasterRenderer.GetViewBuffer();
			VkDescriptorBufferInfo buff = VkDescriptorBufferInfo.create();
			buff.offset(0).range(shbuff.localStride).buffer(shbuff.buffer);

			VkWriteDescriptorSet.Buffer write = VkWriteDescriptorSet.callocStack(1, stack)
					.sType(VK10.VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET).dstSet(desc_set)
					.descriptorType(VK10.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC)
					.pBufferInfo(VkDescriptorBufferInfo.calloc(1).put(0, buff));

			VK10.vkUpdateDescriptorSets(device, write, null);
		}
	}

	public static void UpdateDescriptorSet() {
		try (MemoryStack stack = MemoryStack.stackPush()) {

			ShaderBuffer shbuff = MasterRenderer.GetViewBuffer();
			VkDescriptorBufferInfo buff = VkDescriptorBufferInfo.create();
			buff.offset(0).range(shbuff.localStride).buffer(shbuff.buffer);

			VkWriteDescriptorSet.Buffer write = VkWriteDescriptorSet.callocStack(1, stack)
					.sType(VK10.VK_STRUCTURE_TYPE_WRITE_DESCRIPTOR_SET).dstSet(desc_set)
					.descriptorType(VK10.VK_DESCRIPTOR_TYPE_UNIFORM_BUFFER_DYNAMIC)
					.pBufferInfo(VkDescriptorBufferInfo.calloc(1).put(0, buff));

			VK10.vkUpdateDescriptorSets(device, write, null);
		}
	}

	private void demo_prepare_framebuffers() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			LongBuffer attachments = stack.longs(0, depth.view);

			VkFramebufferCreateInfo fb_info = VkFramebufferCreateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_FRAMEBUFFER_CREATE_INFO).pNext(0).flags(0).renderPass(render_pass)
					.pAttachments(attachments).width(width).height(height).layers(1);

			framebuffers = MemoryUtil.memAllocLong(swapchainImageCount);

			for (int i = 0; i < swapchainImageCount; i++) {
				attachments.put(0, buffers[i].view);
				check(VK10.vkCreateFramebuffer(device, fb_info, null, lp));
				framebuffers.put(i, lp.get(0));
			}
		}
	}

	private void demo_prepare() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkCommandPoolCreateInfo cmd_pool_info = VkCommandPoolCreateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_COMMAND_POOL_CREATE_INFO).pNext(0)
					.flags(VK10.VK_COMMAND_POOL_CREATE_RESET_COMMAND_BUFFER_BIT)
					.queueFamilyIndex(graphics_queue_node_index);

			check(VK10.vkCreateCommandPool(device, cmd_pool_info, null, lp));

			cmd_pool = lp.get(0);

			VkCommandBufferAllocateInfo cmd = VkCommandBufferAllocateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_COMMAND_BUFFER_ALLOCATE_INFO).pNext(0).commandPool(cmd_pool)
					.level(VK10.VK_COMMAND_BUFFER_LEVEL_PRIMARY).commandBufferCount(1);

			check(VK10.vkAllocateCommandBuffers(device, cmd, pp));

		}

		draw_cmd = new VkCommandBuffer(pp.get(0), device);

		demo_prepare_buffers();
		demo_prepare_depth();
		// demo_prepare_textures();

		demo_prepare_descriptor_layout();
		demo_prepare_render_pass();
		demo_prepare_pipeline();

		demo_prepare_descriptor_pool();
		demo_prepare_descriptor_set();

		demo_prepare_framebuffers();

	}

	private void demo_draw_build_cmd() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkCommandBufferBeginInfo cmd_buf_info = VkCommandBufferBeginInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_COMMAND_BUFFER_BEGIN_INFO).pNext(0).flags(0).pInheritanceInfo(null);

			check(VK10.vkBeginCommandBuffer(draw_cmd, cmd_buf_info));

			VkClearValue.Buffer clear_values = VkClearValue.mallocStack(2, stack);
			clear_values.get(0).color().float32(0, 0.2f).float32(1, 0.2f).float32(2, 0.2f).float32(3, 0.2f);
			clear_values.get(1).depthStencil().depth(1).stencil(0);

			VkRenderPassBeginInfo rp_begin = VkRenderPassBeginInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_RENDER_PASS_BEGIN_INFO).pNext(0).renderPass(render_pass)
					.framebuffer(framebuffers.get(current_buffer)).renderArea(new Consumer<VkRect2D>() {
						@Override
						public void accept(VkRect2D ra) {
							ra.offset(new Consumer<VkOffset2D>() {
								@Override
								public void accept(VkOffset2D it) {
									it.x(0).y(0);
								}
							}).extent(new Consumer<VkExtent2D>() {
								@Override
								public void accept(VkExtent2D it) {
									it.width(width).height(height);
								}
							});
						}
					}).pClearValues(clear_values);

			// We can use LAYOUT_UNDEFINED as a wildcard here because we don't care what
			// happens to the previous contents of the image
			VkImageMemoryBarrier.Buffer image_memory_barrier = VkImageMemoryBarrier.mallocStack(1, stack)
					.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER).pNext(0).srcAccessMask(0)
					.dstAccessMask(VK10.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT).oldLayout(VK10.VK_IMAGE_LAYOUT_UNDEFINED)
					.newLayout(VK10.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
					.srcQueueFamilyIndex(VK10.VK_QUEUE_FAMILY_IGNORED).dstQueueFamilyIndex(VK10.VK_QUEUE_FAMILY_IGNORED)
					.image(buffers[current_buffer].image).subresourceRange(new Consumer<VkImageSubresourceRange>() {
						@Override
						public void accept(VkImageSubresourceRange it) {
							it.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).baseMipLevel(0).levelCount(1)
									.baseArrayLayer(0).layerCount(1);
						}
					});

			VK10.vkCmdPipelineBarrier(draw_cmd, VK10.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
					VK10.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT, 0, null, null, image_memory_barrier);
			VK10.vkCmdBeginRenderPass(draw_cmd, rp_begin, VK10.VK_SUBPASS_CONTENTS_INLINE);

			VK10.vkCmdBindPipeline(draw_cmd, VK10.VK_PIPELINE_BIND_POINT_GRAPHICS, pipeline);

			VkViewport.Buffer viewport = VkViewport.callocStack(1, stack).height(height).width(width).minDepth(0.0f)
					.maxDepth(1.0f);
			VK10.vkCmdSetViewport(draw_cmd, 0, viewport);

			VkRect2D.Buffer scissor = VkRect2D.callocStack(1, stack).extent(new Consumer<VkExtent2D>() {
				@Override
				public void accept(VkExtent2D it) {
					it.width(width).height(height);
				}
			}).offset(new Consumer<VkOffset2D>() {
				@Override
				public void accept(VkOffset2D it) {
					it.x(0).y(0);
				}
			});
			VK10.vkCmdSetScissor(draw_cmd, 0, scissor);

			MasterRenderer.MasterRender(draw_cmd);

			VK10.vkCmdEndRenderPass(draw_cmd);

			VkImageMemoryBarrier.Buffer prePresentBarrier = VkImageMemoryBarrier.mallocStack(1, stack)
					.sType(VK10.VK_STRUCTURE_TYPE_IMAGE_MEMORY_BARRIER).pNext(0)
					.srcAccessMask(VK10.VK_ACCESS_COLOR_ATTACHMENT_WRITE_BIT)
					.dstAccessMask(VK10.VK_ACCESS_MEMORY_READ_BIT)
					.oldLayout(VK10.VK_IMAGE_LAYOUT_COLOR_ATTACHMENT_OPTIMAL)
					.newLayout(KHRSwapchain.VK_IMAGE_LAYOUT_PRESENT_SRC_KHR)
					.srcQueueFamilyIndex(VK10.VK_QUEUE_FAMILY_IGNORED).dstQueueFamilyIndex(VK10.VK_QUEUE_FAMILY_IGNORED)
					.image(buffers[current_buffer].image).subresourceRange(new Consumer<VkImageSubresourceRange>() {
						@Override
						public void accept(VkImageSubresourceRange it) {
							it.aspectMask(VK10.VK_IMAGE_ASPECT_COLOR_BIT).baseMipLevel(0).levelCount(1)
									.baseArrayLayer(0).layerCount(1);
						}
					});

			VK10.vkCmdPipelineBarrier(draw_cmd, VK10.VK_PIPELINE_STAGE_ALL_COMMANDS_BIT,
					VK10.VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT, 0, null, null, prePresentBarrier);

			check(VK10.vkEndCommandBuffer(draw_cmd));
		}
	}

	public static void bindDescriptorSets(int offset) {
		lp.put(0, desc_set);
		VK10.vkCmdBindDescriptorSets(draw_cmd, VK10.VK_PIPELINE_BIND_POINT_GRAPHICS, pipeline_layout, 0, lp,
				MemoryStack.stackCallocInt(1).put(offset).rewind());
	}

	private void demo_draw() {
		try (MemoryStack stack = MemoryStack.stackPush()) {
			VkSemaphoreCreateInfo semaphoreCreateInfo = VkSemaphoreCreateInfo.mallocStack(stack)
					.sType(VK10.VK_STRUCTURE_TYPE_SEMAPHORE_CREATE_INFO).pNext(0).flags(0);

			check(VK10.vkCreateSemaphore(device, semaphoreCreateInfo, null, lp));
			long imageAcquiredSemaphore = lp.get(0);

			check(VK10.vkCreateSemaphore(device, semaphoreCreateInfo, null, lp));
			long drawCompleteSemaphore = lp.get(0);

			// Get the index of the next available swapchain image:
			int err = KHRSwapchain.vkAcquireNextImageKHR(device, swapchain, ~0L, imageAcquiredSemaphore, 0, // TODO:
																											// Show use
																											// of fence
					ip);
			if (err == KHRSwapchain.VK_ERROR_OUT_OF_DATE_KHR) {
				// demo->swapchain is out of date (e.g. the window was resized) and
				// must be recreated:
				demo_resize();
				demo_draw();
				VK10.vkDestroySemaphore(device, drawCompleteSemaphore, null);
				VK10.vkDestroySemaphore(device, imageAcquiredSemaphore, null);
				return;
			} else if (err == KHRSwapchain.VK_SUBOPTIMAL_KHR) {
				// demo->swapchain is not as optimal as it could be, but the platform's
				// presentation engine will still present the image correctly.
			} else {
				check(err);
			}
			current_buffer = ip.get(0);

			demo_flush_init_cmd();

			// Wait for the present complete semaphore to be signaled to ensure
			// that the image won't be rendered to until the presentation
			// engine has fully released ownership to the application, and it is
			// okay to render to the image.

			demo_draw_build_cmd();
			LongBuffer lp2 = stack.mallocLong(1);
			VkSubmitInfo submit_info = VkSubmitInfo.mallocStack(stack).sType(VK10.VK_STRUCTURE_TYPE_SUBMIT_INFO)
					.pNext(0).waitSemaphoreCount(1).pWaitSemaphores(lp.put(0, imageAcquiredSemaphore))
					.pWaitDstStageMask(ip.put(0, VK10.VK_PIPELINE_STAGE_BOTTOM_OF_PIPE_BIT))
					.pCommandBuffers(pp.put(0, draw_cmd)).pSignalSemaphores(lp2.put(0, drawCompleteSemaphore));

			check(VK10.vkQueueSubmit(queue, submit_info, VK10.VK_NULL_HANDLE));

			VkPresentInfoKHR present = VkPresentInfoKHR.callocStack(stack)
					.sType(KHRSwapchain.VK_STRUCTURE_TYPE_PRESENT_INFO_KHR).pNext(0).pWaitSemaphores(lp2)
					.swapchainCount(1).pSwapchains(lp.put(0, swapchain)).pImageIndices(ip.put(0, current_buffer));

			err = KHRSwapchain.vkQueuePresentKHR(queue, present);
			if (err == KHRSwapchain.VK_ERROR_OUT_OF_DATE_KHR) {
				// demo->swapchain is out of date (e.g. the window was resized) and
				// must be recreated:
				demo_resize();
			} else if (err == KHRSwapchain.VK_SUBOPTIMAL_KHR) {
				// demo->swapchain is not as optimal as it could be, but the platform's
				// presentation engine will still present the image correctly.
			} else {
				check(err);
			}

			check(VK10.vkQueueWaitIdle(queue));

			VK10.vkDestroySemaphore(device, drawCompleteSemaphore, null);
			VK10.vkDestroySemaphore(device, imageAcquiredSemaphore, null);
		}
	}

	private void demo_resize() {
		// In order to properly resize the window, we must re-create the swapchain
		// AND redo the command buffers, etc.
		//
		// First, perform part of the demo_cleanup() function:

		for (int i = 0; i < swapchainImageCount; i++) {
			VK10.vkDestroyFramebuffer(device, framebuffers.get(i), null);
		}
		MemoryUtil.memFree(framebuffers);
		VK10.vkDestroyDescriptorPool(device, desc_pool, null);

		if (setup_cmd != null) {
			VK10.vkFreeCommandBuffers(device, cmd_pool, setup_cmd);
			setup_cmd = null;
		}
		VK10.vkFreeCommandBuffers(device, cmd_pool, draw_cmd);
		VK10.vkDestroyCommandPool(device, cmd_pool, null);

		VK10.vkDestroyPipeline(device, pipeline, null);
		VK10.vkDestroyRenderPass(device, render_pass, null);
		VK10.vkDestroyPipelineLayout(device, pipeline_layout, null);
		VK10.vkDestroyDescriptorSetLayout(device, desc_layout, null);

		for (int i = 0; i < swapchainImageCount; i++) {
			VK10.vkDestroyImageView(device, buffers[i].view, null);
		}

		VK10.vkDestroyImageView(device, depth.view, null);
		VK10.vkDestroyImage(device, depth.image, null);
		VK10.vkFreeMemory(device, depth.mem, null);

		buffers = null;

		// Second, re-perform the demo_prepare() function, which will re-create the
		// swapchain:
		demo_prepare();
	}

	private void demo_run() {
		int c = 0;
		long t = System.nanoTime();

		while (!GLFW.glfwWindowShouldClose(window)) {
			GLFW.glfwPollEvents();

			logic.Update();

			demo_draw();

			c++;
			if (System.nanoTime() - t > 1000 * 1000 * 1000) {
				System.out.println(c);
				t = System.nanoTime();
				c = 0;
			}

			// Wait for work to finish before updating MVP.
			VK10.vkDeviceWaitIdle(device);
		}
	}

	private void demo_cleanup() {
		for (int i = 0; i < swapchainImageCount; i++) {
			VK10.vkDestroyFramebuffer(device, framebuffers.get(i), null);
		}
		MemoryUtil.memFree(framebuffers);
		VK10.vkDestroyDescriptorPool(device, desc_pool, null);

		if (setup_cmd != null) {
			VK10.vkFreeCommandBuffers(device, cmd_pool, setup_cmd);
			setup_cmd = null;
		}
		VK10.vkFreeCommandBuffers(device, cmd_pool, draw_cmd);
		VK10.vkDestroyCommandPool(device, cmd_pool, null);

		VK10.vkDestroyPipeline(device, pipeline, null);
		VK10.vkDestroyRenderPass(device, render_pass, null);
		VK10.vkDestroyPipelineLayout(device, pipeline_layout, null);
		VK10.vkDestroyDescriptorSetLayout(device, desc_layout, null);

		RenderModel.cleanUp();
		MasterRenderer.cleanUp();

		for (int i = 0; i < swapchainImageCount; i++) {
			VK10.vkDestroyImageView(device, buffers[i].view, null);
		}

		VK10.vkDestroyImageView(device, depth.view, null);
		VK10.vkDestroyImage(device, depth.image, null);
		VK10.vkFreeMemory(device, depth.mem, null);

		KHRSwapchain.vkDestroySwapchainKHR(device, swapchain, null);
		buffers = null;

		VK10.vkDestroyDevice(device, null);
		KHRSurface.vkDestroySurfaceKHR(inst, surface, null);
		if (msg_callback != 0) {
			EXTDebugReport.vkDestroyDebugReportCallbackEXT(inst, msg_callback, null);
		}
		VK10.vkDestroyInstance(inst, null);
		dbgFunc.free();

		gpu_features.free();
		gpu_props.free();
		queue_props.free();
		memory_properties.free();

		Callbacks.glfwFreeCallbacks(window);
		GLFW.glfwDestroyWindow(window);
		GLFW.glfwTerminate();
		Objects.requireNonNull(GLFW.glfwSetErrorCallback(null)).free();

		MemoryUtil.memFree(extension_names);

		MemoryUtil.memFree(pp);
		MemoryUtil.memFree(lp);
		MemoryUtil.memFree(ip);

		MemoryUtil.memFree(EXT_debug_report);
		MemoryUtil.memFree(KHR_swapchain);
	}

	private void run() {
		demo_init();
		demo_create_window();
		demo_init_vk_swapchain();
		MasterRenderer.Init(device, MemoryStack.stackPush());
		logic.init();
		demo_prepare();
		demo_run();

		demo_cleanup();
	}

	public static void main(String[] args) {
		if (args.length > 0 && args[0].equalsIgnoreCase("debug"))
			Main.VALIDATE = true;
		new Main().run();
	}
}
