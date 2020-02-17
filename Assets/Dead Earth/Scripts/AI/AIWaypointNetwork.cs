using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Display Mode that the Custom Inspector of an AIWaypointNetwork
// component can be in
public enum PathDisplayMode { None, Connections, Paths }

// -------------------------------------------------------------------
// CLASS	:	AIWaypointNetwork
// DESC		:	包含节点list和自定义视图
// ------------------------------------------------------------------
public class AIWaypointNetwork : MonoBehaviour 
{		
	[HideInInspector]												
	public PathDisplayMode DisplayMode = PathDisplayMode.Connections;	// Current Display Mode
	[HideInInspector]	
	public int UIStart 	= 0;											// Start wayopoint index for Paths mode
	[HideInInspector]
	public int UIEnd	= 0;											// End waypoint index for Paths mode

	// 节点引用Transform列表
	public List<Transform> Waypoints   = new List<Transform>();

}
