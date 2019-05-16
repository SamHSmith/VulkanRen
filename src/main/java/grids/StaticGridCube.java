package grids;

import org.joml.Quaternionf;
import org.joml.Vector3f;

import rendering.IRenderCube;

public class StaticGridCube implements IRenderCube {

	public Vector3f position=new Vector3f();
	public Quaternionf rotation = new Quaternionf();
	
	public StaticGridCube(Vector3f position, Quaternionf rotation) {
		
		this.position = position;
		this.rotation = rotation;
	}
	
	public StaticGridCube() {}

	@Override
	public Vector3f getWorldPosition() {
		// TODO Auto-generated method stub
		return position;
	}

	@Override
	public Quaternionf getWorldRotation() {
		// TODO Auto-generated method stub
		return rotation;
	}

}
