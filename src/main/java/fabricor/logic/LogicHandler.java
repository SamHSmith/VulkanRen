package fabricor.logic;

import org.joml.Vector2f;
import org.joml.Vector3f;
import org.joml.Vector3i;
import org.lwjgl.glfw.GLFW;

import fabricor.grids.Grid;
import fabricor.grids.StaticGridCube;
import fabricor.logic.input.InputManager;
import fabricor.main.Main;
import fabricor.rendering.Camera;
import fabricor.rendering.MasterRenderer;
import fabricor.util.Mathf;

public class LogicHandler {
	
	Camera cam=new Camera();
	
	float xrot=0,yrot=0;

	private Grid g;
	
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
		
		xrot-=mouse.y;
		yrot+=mouse.x;
		
		xrot=Mathf.Clamp(xrot, -90, 90);
		
		cam.rotation.identity();
		cam.rotation.rotateAxis((float) Math.toRadians(yrot), new Vector3f(0,1,0));
		cam.rotation.rotateAxis((float) Math.toRadians(xrot), new Vector3f(1,0,0));
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_W).isPressed()) {
			cam.position.add(new Vector3f(0,0,1).rotate(cam.rotation));
		}
		
		
		
	}

	public void init() {
		InputManager.HideCursor();
		MasterRenderer.cameras.add(cam);
		g = new Grid(new Vector3i(32, 64, 32));
		MasterRenderer.toRenderObjs.add(g);
		
		for (int x = 0; x < 32; x++) {
			for (int z = 0; z < 32; z++) {
				for (int y = 10; y < 64; y++) {
					g.put(new StaticGridCube(), new Vector3i(x,y,z));
				}
			}
		}
	}
}
