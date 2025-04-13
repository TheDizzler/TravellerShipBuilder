using UnityEngine;
public interface IMoveable
{
	public DesignObject designObject { get; }
	public Vector3 SnapToGrid(Vector3 pos);
	public void MouseDrag(Vector2 worldPos);
	public bool IsDragging();
	public void EndDrag(Vector2 pos);
	public void ResetToLastPosition();

}
