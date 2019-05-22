package fabricor.logic.input;

public class Key {
	boolean pressed=false;
	boolean down=false;
	boolean released=false;
	
	public boolean isPressed() {
		return pressed;
	}
	public boolean isDown() {
		return down;
	}
	public boolean isReleased() {
		return released;
	}
	@Override
	public String toString() {
		return "Key [pressed=" + pressed + ", down=" + down + ", released=" + released + "]";
	}
}
