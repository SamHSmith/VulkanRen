package fabricor.rendering;

import org.joml.Quaternionf;
import org.joml.Vector3f;

public interface IRenderCube {

	public Vector3f getWorldPosition();
	public Quaternionf getWorldRotation();
	
}
