package fabricor.grids;

import java.util.ArrayList;

import org.joml.Matrix4f;
import org.joml.Quaternionf;
import org.joml.Vector3f;
import org.joml.Vector3i;
import org.joml.Vector4f;

import fabricor.rendering.IRenderCube;
import fabricor.rendering.IRenderable;


public class Grid implements IRenderable{

	public Vector3f position=new Vector3f();
	public Quaternionf rotation=new Quaternionf();
	public boolean isStatic=false;
	
	

	public Vector3f getPosition() {
		return position;
	}

	public Quaternionf getRotation() {
		return rotation;
	}

	public boolean isStatic() {
		return isStatic;
	}

	public boolean isBind() {
		return bind;
	}

	public Matrix4f getTransform() {
		return new Matrix4f().translate(position).rotate(rotation);
	}

	private boolean bind = false;

	public boolean shouldBind() {
		boolean result = bind;
		bind = false;
		return result;
	}

	private StaticGridCube[][][] cubes;
	private Vector3i extent;

	public ArrayList<StaticGridCube> getCubes(){
		ArrayList<StaticGridCube> c=new ArrayList<StaticGridCube>();
		for (int x = 0; x < cubes.length; x++) {
			for (int y = 0; y < cubes[x].length; y++) {
				for (int z = 0; z < cubes[x][y].length; z++) {
					c.add(cubes[x][y][z]);
				}
			}
		}
		return c;
	}
	
	public Vector3i getExtent() {
		return extent;
	}

	public Grid(Vector3i extent) {
		this.extent = extent;
		cubes = new StaticGridCube[extent.x][extent.y][extent.z];
	}

	public ArrayList<IRenderCube> getRenderCubes(){
		ArrayList<IRenderCube> rendercubes =new ArrayList<IRenderCube>();
		for (int x = 0; x < cubes.length; x++) {
			for (int y = 0; y < cubes[x].length; y++) {
				for (int z = 0; z < cubes[x][y].length; z++) {
					if(cubes[x][y][z]!=null) {
						IRenderCube cube=getCube(x, y, z);
						if(cube==null)
							continue;
						boolean hasFreeSide=false;
						for (int i = 0; i < cube.getNonOccludedSides().length; i++) {
							if(cube.getNonOccludedSides()[i]) {
								hasFreeSide=true;
								break;
							}
						}
						if(hasFreeSide)
							rendercubes.add(cube);
					}
				}
			}
		}
		
		return rendercubes;
	}
	
	public boolean IsGridEmpty() {
		for (int x = 0; x < cubes.length; x++) {
			for (int y = 0; y < cubes[x].length; y++) {
				for (int z = 0; z < cubes[x][y].length; z++) {
					if(cubes[x][y][z]!=null)
						return false;
				}
			}
		}
		return true;
	}
	
	private IRenderCube getCube(int x,int y,int z) {
		StaticGridCube c=cubes[x][y][z];
		if(c==null)
			return c;
		c.sides[0]=isEmpty(x, y-1, z);
		c.sides[1]=isEmpty(x, y, z-1);
		c.sides[2]=isEmpty(x, y+1, z);
		c.sides[3]=isEmpty(x, y, z+1);
		c.sides[4]=isEmpty(x+1, y, z);
		c.sides[5]=isEmpty(x-1, y, z);
		return c;
	}
	
	public boolean isEmpty(int x,int y,int z) {
		
		if(x<0||y<0||z<0||x>=extent.x||y>=extent.y||z>=extent.z)
			return true;
		
		return cubes[x][y][z]==null;
	}
	
	public void put(StaticGridCube c) {
		Vector4f pos=getTransform().transform(new Vector4f(c.position,1));
		cubes[(int) pos.x][(int) pos.y][(int) pos.z]=c;
		c.position.set((int)pos.x, (int)pos.x, (int)pos.x);
		c.internalLocation.set((int)pos.x, (int)pos.y, (int)pos.z);
		bind=true;
	}
	
	public void put(StaticGridCube c,Vector3i pos) {
		cubes[pos.x][pos.y][pos.z]=c;
		c.position.set(pos.x, pos.y,pos.z);
		c.internalLocation.set((int)pos.x, (int)pos.y, (int)pos.z);
		bind=true;
	}
	
	public void remove(Vector3f position) {
		Vector4f v4 =getTransform().transform(new Vector4f(position,1f));
		cubes[(int) v4.x][(int) v4.y][(int) v4.z]=null;
		bind=true;
	}

}