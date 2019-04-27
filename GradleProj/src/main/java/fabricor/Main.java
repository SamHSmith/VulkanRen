package fabricor;

import org.lwjgl.glfw.GLFW;
import org.lwjgl.vulkan.VkInstance;
import org.lwjgl.vulkan.VkInstanceCreateInfo;

public class Main {

	public static void main(String[] args) {
		if(!GLFW.glfwInit()) {
			System.err.println("Failed to initialize glfw.");
			return;
		}
		
		GLFW.glfwWindowHint(GLFW.GLFW_CLIENT_API, GLFW.GLFW_NO_API);
		GLFW.glfwWindowHint(GLFW.GLFW_RESIZABLE, GLFW.GLFW_FALSE);
		long window = GLFW.glfwCreateWindow(800, 600, "Vulkan Window", 0, 0);
		
		VkInstance instance;
		//VkInstanceCreateInfo cinfo=new VkInstanceCrea
		//TODO Learn about stack and memory allocation.
		
		while(!GLFW.glfwWindowShouldClose(window)) {
			GLFW.glfwPollEvents();
		}
		
		GLFW.glfwDestroyWindow(window);
		GLFW.glfwTerminate();
	}

}
