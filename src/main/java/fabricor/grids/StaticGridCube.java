package fabricor.grids;

import org.joml.Quaternionf;
import org.joml.Vector3f;

import fabricor.rendering.IRenderCube;

public class StaticGridCube implements IRenderCube {
	private static Quaternionf identity=new Quaternionf();
	
	Vector3f position=new Vector3f();
	boolean[] sides=new boolean[6];

	@Override
	public Vector3f getWorldPosition() {
		return position;
	}

	@Override
	public Quaternionf getWorldRotation() {
		return identity;
	}

	@Override
	public boolean[] getNonOccludedSides() {
		return sides;
	}

}