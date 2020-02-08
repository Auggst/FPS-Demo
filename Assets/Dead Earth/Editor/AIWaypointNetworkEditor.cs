using UnityEngine;
using System.Collections;
using UnityEditor;

// ------------------------------------------------------------------------------------
// CLASS	:	AIWaypointNetworkEditor
// DESC		:	Custom Inspector and Scene View Rendering for the AIWaypointNetwork
//				Component
// ------------------------------------------------------------------------------------
[CustomEditor(typeof(AIWaypointNetwork))]
public class AIWaypointNetworkEditor : Editor 
{
	// --------------------------------------------------------------------------------
	// Name	:	OnInspectorGUI (Override)
	// Desc	:	由Unity Editor调用，用于调整Unity Inspector
	// --------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		// 获取目标组件
		AIWaypointNetwork network = (AIWaypointNetwork)target;
	
		//调整显示模式
		network.DisplayMode = (PathDisplayMode)EditorGUILayout.EnumPopup ( "Display Mode", network.DisplayMode );
	
		//如果显示模式为Paths，则显示起始和终止位置输入框
		if (network.DisplayMode==PathDisplayMode.Paths)
		{
			network.UIStart		= EditorGUILayout.IntSlider ( "Waypoint Start" , network.UIStart, 0, network.Waypoints.Count-1);
			network.UIEnd		= EditorGUILayout.IntSlider ( "Waypoint End" , network.UIEnd, 0, network.Waypoints.Count-1);
		}

		// 渲染
		DrawDefaultInspector();
	}


	// --------------------------------------------------------------------------------
	// Name	:	OnSceneGUI
	// Desc	:	当视图重新绘制时调用此方法。用于调整Scene
	// --------------------------------------------------------------------------------
	void OnSceneGUI()
	{
		//获取渲染的组件
		AIWaypointNetwork network = (AIWaypointNetwork)target;

		// 遍历所有的组件并绘制标签
		for(int i=0; i<network.Waypoints.Count;i++)
		{
			if (network.Waypoints[i]!=null)
				Handles.Label ( network.Waypoints[i].position, "Waypoint "+i.ToString ());
		}

		// 如果是Connection模式，绘制所有路径
		if (network.DisplayMode == PathDisplayMode.Connections)
		{
			// 定义一个路径节点的位置数组
			Vector3 [] linePoints = new Vector3[ network.Waypoints.Count+1 ];

			// 循环遍历每一个路径节点再交互
			for(int i=0; i<=network.Waypoints.Count;i++)
			{
				// 计算每一个路径节点的编号，从0开始，循环至0
				int index = i!=network.Waypoints.Count ? i : 0; 

				// 获取每一个路径节点的位置
				if (network.Waypoints[index]!=null)
					linePoints[i] = network.Waypoints[index].position;
				else
					linePoints[i] = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
			}

			// 设置绘制颜色为青绿色
			Handles.color = Color.cyan;

			// 在场景中绘制多边形，顶点为各路径节点
			Handles.DrawPolyLine ( linePoints );
		}
		else
		// 若为Paths模式
		if (network.DisplayMode == PathDisplayMode.Paths)
		{
			// 定义一个导航网格
			UnityEngine.AI.NavMeshPath path 	= new UnityEngine.AI.NavMeshPath();

			// 若UIStart和UIEnd全不为空
			if (network.Waypoints[network.UIStart]!=null && network.Waypoints[network.UIEnd]!=null)
			{
				// 获取起止节点位置
				Vector3 from 		= network.Waypoints[network.UIStart].position;
				Vector3 to			= network.Waypoints[network.UIEnd].position;

				// 计算起止节点之间的路径
				UnityEngine.AI.NavMesh.CalculatePath ( from, to, UnityEngine.AI.NavMesh.AllAreas, path );

				//设置绘制颜色为黄色
				Handles.color = Color.yellow;

				// 渲染路径
				Handles.DrawPolyLine( path.corners );
			}
		}
		
	}

}
