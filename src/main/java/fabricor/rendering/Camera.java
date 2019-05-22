package fabricor.rendering;

import org.joml.Matrix4f;
import org.joml.Quaternionf;
import org.joml.Vector3f;

public class Camera {
	
	public Vector3f position=new Vector3f();
	public Quaternionf rotation=new Quaternionf();
	
	public float fov=80;
	
	public Matrix4f getTransform() {
		return new Matrix4f().translate(position).rotate(rotation);
	}

	public float getFov() {
		return fov;
	}
	
	
	
	
}
