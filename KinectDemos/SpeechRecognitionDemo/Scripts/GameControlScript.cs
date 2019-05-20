/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
using System.Collections;

public class GameControlScript : MonoBehaviour 
{
	[Tooltip("Prefab used to create the scene fence.")]
	public GameObject cratePrefab;

	[Tooltip("GUI-Window rectangle in screen coordinates (pixels).")]
	public Rect guiWindowRect = new Rect(10, 40, 200, 300);

	[Tooltip("GUI-Window skin (optional).")]
	public GUISkin guiSkin;
	
	
	void Start () 
	{
		Quaternion quatRot90 = Quaternion.Euler(new Vector3(0, 90, 0));
		GameObject newObj = null;
		
		for(int i = -50; i <= 50; i++)
		{
			newObj = (GameObject)GameObject.Instantiate(cratePrefab, new Vector3(i, 0.32f, 50), Quaternion.identity);
			newObj.transform.parent = transform;

			newObj = (GameObject)GameObject.Instantiate(cratePrefab, new Vector3(i, 0.32f, -50), Quaternion.identity);
			newObj.transform.parent = transform;

			newObj = (GameObject)GameObject.Instantiate(cratePrefab, new Vector3(50, 0.32f, i), quatRot90);
			newObj.transform.parent = transform;

			newObj = (GameObject)GameObject.Instantiate(cratePrefab, new Vector3(-50, 0.32f, i), quatRot90);
			newObj.transform.parent = transform;
		}
	}

	private void ShowGuiWindow(int windowID) 
	{
		GUILayout.BeginVertical();

		GUILayout.Label("");
		GUILayout.Label("<b>* FORWARD / GO AHEAD</b>");
		GUILayout.Label("<b>* BACK / GO BACK</b>");
		GUILayout.Label("<b>* TURN LEFT</b>");
		GUILayout.Label("<b>* TURN RIGHT</b>");
		GUILayout.Label("<b>* RUN</b>");
		GUILayout.Label("<b>* JUMP</b>");
		GUILayout.Label("<b>* STOP</b>");
		GUILayout.Label("<b>* HELLO / WAVE</b>");
		GUILayout.Label("<i>For more audio commands\nlook at the grammar file.</i>");
		
		GUILayout.EndVertical();
		
		// Make the window draggable.
		GUI.DragWindow();
	}
	
	void OnGUI()
	{
		GUI.skin = guiSkin;
		guiWindowRect = GUI.Window(0, guiWindowRect, ShowGuiWindow, "Audio Commands");
	}
	
}
