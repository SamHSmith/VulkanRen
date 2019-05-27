package fabricor.logic.input;

import java.util.HashMap;

import org.joml.Vector2f;
import org.lwjgl.glfw.GLFW;

import fabricor.main.Main;

public class InputManager {

	public static float MOUSE_SENSITIVITY=2f,MOUSE_SENSITIVITY_X=0.2f,MOUSE_SENSITIVITY_Y=0.15f;
	
	private static float lastX = 0, lastY = 0;
	static float deltaY = 0, deltaX = 0;
	static float lastdeltaY = 0, lastdeltaX = 0;
	private static boolean hasUpdatedDelta = false;

	private static HashMap<Integer, Key> keyboard = new HashMap<Integer, Key>();
	private static HashMap<Integer, Integer> newKeyboard = new HashMap<Integer, Integer>();

	private static HashMap<Integer, Key> mouse = new HashMap<Integer, Key>();
	private static HashMap<Integer, Integer> newMouse = new HashMap<Integer, Integer>();

	public static Key getKeyboard(int key) {
		return keyboard.getOrDefault(key, new Key());
	}
	
	private static float getDeltaX() {
		return (deltaX+lastdeltaX)/2;
	}
	
	private static float getDeltaY() {
		return (deltaY+lastdeltaY)/2;
	}

	public static Vector2f getMouseDelta() {
		Vector2f v=new Vector2f(getDeltaX(), getDeltaY()).mul(MOUSE_SENSITIVITY);
		return v;
	}

	public static Key getMouse(int button) {
		return mouse.getOrDefault(button, new Key());
	}

	public static void InvokeKey(int key, int action) {
		newKeyboard.put(key, action);
	}

	public static void InvokeMouse(double x, double y) {
		x*=MOUSE_SENSITIVITY_X;
		y*=MOUSE_SENSITIVITY_Y;
		
		deltaX = (float) x - lastX;
		lastX = (float) x;

		deltaY = (float) y - lastY;
		lastY = (float) y;
		hasUpdatedDelta = true;
		
	}

	public static void InvokeMouseButton(int button, int action) {
		newMouse.put(button, action);
	}

	public static void UpdateInput() {
		if (!hasUpdatedDelta) {
			deltaX = 0;
			deltaY = 0;
		}
		
		lastdeltaX=(deltaX+lastdeltaX)/2;
		lastdeltaY=(deltaY+lastdeltaY)/2;
		
		hasUpdatedDelta = false;
		for (Key k : keyboard.values()) {
			k.pressed = false;
		}
		for (Key k : mouse.values()) {
			k.pressed = false;
		}

		for (int k : newKeyboard.keySet()) {
			if (!keyboard.containsKey(k))
				keyboard.put(k, new Key());

			Key button = keyboard.get(k);
			if (newKeyboard.get(k) == GLFW.GLFW_PRESS) {
				button.pressed = true;
				button.down = true;
			}
			if (newKeyboard.get(k) == GLFW.GLFW_RELEASE) {
				button.released = true;
				button.down = false;
			}
		}
		newKeyboard.clear();

		for (int k : newMouse.keySet()) {
			if (!mouse.containsKey(k))
				mouse.put(k, new Key());

			Key button = mouse.get(k);
			if (newMouse.get(k) == GLFW.GLFW_PRESS) {
				button.pressed = true;
				button.down = true;
			}
			if (newMouse.get(k) == GLFW.GLFW_RELEASE) {
				button.released = true;
				button.down = false;
			}
		}
		newMouse.clear();

	}

	private static boolean cursorHidden = false;

	public static void HideCursor() {
		GLFW.glfwSetInputMode(Main.getWindow(), GLFW.GLFW_CURSOR, GLFW.GLFW_CURSOR_DISABLED);
		cursorHidden = true;
		
	}

	public static void ShowCursor() {
		GLFW.glfwSetInputMode(Main.getWindow(), GLFW.GLFW_CURSOR, GLFW.GLFW_CURSOR_NORMAL);
		cursorHidden = false;
	}

	public static boolean IsCursorHidden() {
		return cursorHidden;
	}

	public static void ToggleCursor() {
		if (IsCursorHidden()) {
			ShowCursor();
		} else {
			HideCursor();
		}
	}
}
