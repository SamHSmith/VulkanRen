package fabricor.logic.physics;

import org.ode4j.ode.DWorld;

import fabricor.grids.Grid;


public class PhysicsWorld {
	DWorld dworld;
//	DynamicsWorld dworld;
//	CollisionConfiguration collisionConf;
//	CollisionDispatcher collisionDispatch;

//	private HashMap<Grid, RigidBody> grids = new HashMap<Grid, RigidBody>();

	public PhysicsWorld(float radius) {
		
		dworld= DWorl
		
//		collisionConf = new DefaultCollisionConfiguration();
//		collisionDispatch = new CollisionDispatcher(collisionConf);
//		AxisSweep3 broadphase = new AxisSweep3(new javax.vecmath.Vector3f(-radius, -radius, -radius),
//				new javax.vecmath.Vector3f(radius, radius, radius));
//		SequentialImpulseConstraintSolver solver = new SequentialImpulseConstraintSolver();
//
//		dworld = new DiscreteDynamicsWorld(collisionDispatch, broadphase, solver, collisionConf);
//		dworld.getSolverInfo().numIterations = 4;
//
//		dworld.setGravity(new Vector3f(0, 10, 0));

	}

	public void Update(float delta) {

//		dworld.stepSimulation(delta);
//
//		for (Grid g : grids.keySet()) {
//			MotionState ms = grids.get(g).getMotionState();
//			Transform t = new Transform();
//			ms.getWorldTransform(t);
//			g.position.x = t.origin.x;
//			g.position.y = t.origin.y;
//			g.position.z = t.origin.z;
//			float[] m = new float[9];
//
//			org.joml.Matrix3f mat = new org.joml.Matrix3f(t.basis.m00, t.basis.m01, t.basis.m02, t.basis.m10,
//					t.basis.m11, t.basis.m12, t.basis.m20, t.basis.m21, t.basis.m22);
//			mat.invert();
//			g.rotation.setFromNormalized((Matrix3fc) mat);
//
//		}
	}

	public void addGrid(Grid g) {
//		float mass = 0;
//		CompoundShape shape = new CompoundShape();
//		for (StaticGridCube cube : g.getCubes()) {
//			Transform transform = new Transform();
//			transform.setIdentity();
//			transform.origin.set(cube.internalLocation.x, cube.internalLocation.y, cube.internalLocation.z);
//			System.out.println(cube.internalLocation);
//			shape.addChildShape(transform, new BoxShape(new Vector3f(0.5f, 0.5f, 0.5f)));
//			mass++;
//		}
//		if (g.isStatic)
//			mass = 0;
//
//		Vector3f inertia = new Vector3f();
//		shape.calculateLocalInertia(mass, inertia);
//		Transform transform = new Transform();
//		transform.setIdentity();
//		transform.origin.set(g.getPosition().x, g.getPosition().y, g.getPosition().z);
//		transform.basis.set(new Quat4f(g.getRotation().x, g.getRotation().y, g.getRotation().z, g.getRotation().w));
//		System.out.println(transform.basis);
//		MotionState ms = new DefaultMotionState(transform);
//		RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(mass, ms, shape, inertia);
//		RigidBody rb = new RigidBody(info);
//		rb.setRestitution(0.1f);
//		rb.setFriction(0.5f);
//		grids.put(g, rb);
//		dworld.addRigidBody(rb);
	}

}
