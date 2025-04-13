using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;

public interface ICreateable
{
	public DesignObject designObject { get; }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="createdObject"></param>
	/// <returns>The edit mode that we want to default to after the object is created.</returns>
	public EditMode Create(Vector3 pos, out DesignObject createdObject);
	public void Delete();
}
