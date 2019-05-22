package fabricor.logic;

import org.joml.Vector2f;
import org.joml.Vector3f;
import org.lwjgl.glfw.GLFW;

import fabricor.logic.input.InputManager;
import fabricor.main.Main;
import fabricor.rendering.Camera;
import fabricor.rendering.MasterRenderer;
import fabricor.util.Mathf;

public class LogicHandler {
	
	Camera cam=new Camera();
	
	float xrot=0,yrot=0;
	
	public void Update() {
		InputManager.UpdateInput();
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_ESCAPE).isReleased())
			GLFW.glfwSetWindowShouldClose(Main.getWindow(), true);
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_TAB).isPressed())
			InputManager.ToggleCursor();
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_F).isPressed()) {
			cam=new Camera();
			MasterRenderer.cameras.add(cam);
		}
		
		Vector2f mouse=InputManager.getMouseDelta();
		
		xrot+=mouse.y;
		yrot-=mouse.x;
		
		xrot=Mathf.Clamp(xrot, -90, 90);
		
		cam.rotation.identity();
		cam.rotation.rotateAxis((float) Math.toRadians(xrot), new Vector3f(1,0,0));
		cam.rotation.rotateAxis((float) Math.toRadians(yrot), new Vector3f(0,1,0));
		
		
	}

	public void init() {
		InputManager.HideCursor();
		MasterRenderer.cameras.add(cam);
	}
}
