package fabricor.logic;

import org.joml.Vector2f;
import org.joml.Vector3f;
import org.joml.Vector3i;
import org.lwjgl.glfw.GLFW;

import fabricor.grids.Grid;
import fabricor.grids.StaticGridCube;
import fabricor.logic.input.InputManager;
import fabricor.logic.physics.PhysicsWorld;
import fabricor.main.Main;
import fabricor.rendering.Camera;
import fabricor.rendering.MasterRenderer;
import fabricor.util.Mathf;

public class LogicHandler {
	
	public PhysicsWorld pworld=new PhysicsWorld(500);
	
	Camera cam=new Camera();
	
	float xrot=0,yrot=0;
	
	
	private long lastUpdate=-1;
	public void Update() {
		if(lastUpdate==-1) {
			lastUpdate=System.nanoTime();
		}
		float delta=(float)(System.nanoTime()-lastUpdate)/1000000000;
		lastUpdate=System.nanoTime();
		InputManager.UpdateInput();
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_ESCAPE).isReleased())
			GLFW.glfwSetWindowShouldClose(Main.getWindow(), true);
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_TAB).isPressed())
			InputManager.ToggleCursor();
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_F).isPressed()) {
			Grid box = new Grid(new Vector3i(1, 1, 1));
			box.position.y=-10;
			box.position.x=20f;
			box.position.z=20f;
			MasterRenderer.toRenderObjs.add(box);
			box.put(new StaticGridCube(), new Vector3i(0,0,0));
			
			pworld.addGrid(box);
		}
		
		Vector2f mouse=InputManager.getMouseDelta();
		
		xrot-=mouse.y;
		yrot+=mouse.x;
		
		xrot=Mathf.Clamp(xrot, -90, 90);
		
		cam.rotation.identity();
		cam.rotation.rotateAxis((float) Math.toRadians(yrot), new Vector3f(0,1,0));
		cam.rotation.rotateAxis((float) Math.toRadians(xrot), new Vector3f(1,0,0));
		
		if(InputManager.getKeyboard(GLFW.GLFW_KEY_W).isDown()) {
			cam.position.add(new Vector3f(0,0,10*delta).rotate(cam.rotation));
		}
		
		pworld.Update(delta);
	}

	public void init() {
		InputManager.HideCursor();
		MasterRenderer.cameras.add(cam);
		cam.position.z=-10;
		
		Grid ground = new Grid(new Vector3i(50, 1, 50));
		ground.position.y=10;
		ground.rotation.rotateAxis((float)Math.toRadians(0), new Vector3f(0,0,1));
		MasterRenderer.toRenderObjs.add(ground);
		
		for (int x = 0; x < 50; x++) {
			for (int z = 0; z < 50; z++) {
				for (int y = 0; y < 1; y++) {
					ground.put(new StaticGridCube(), new Vector3i(x,y,z));
				}
			}
		}
		ground.isStatic=true;
		pworld.addGrid(ground);
		
	}
}
