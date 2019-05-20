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

public class GuiWindowScript : MonoBehaviour 
{
	[Tooltip("GUI-Window rectangle in screen coordinates (pixels).")]
	public Rect guiWindowRect = new Rect(-140, 40, 140, 420);
	[Tooltip("GUI-Window skin (optional).")]
	public GUISkin guiSkin;

	// public parameters
	[Tooltip("Reference to the plane below the draggable objects.")]
	public GameObject planeObj = null;
	[Tooltip("Whether the window is currently invisible or not.")]
	public bool hiddenWindow = false;
	
	private bool resetObjectsClicked = false;
	private bool hideWindowClicked = false;
	private bool isGravityOn = true;
	private bool isPlaneOn = true;
	private bool isControlMouseOn = true;

	private string label1Text = string.Empty;
	private string label2Text = string.Empty;


	void Start()
	{
		planeObj = GameObject.Find("Plane");
	}


	private void ShowGuiWindow(int windowID) 
	{
		GUILayout.BeginVertical();

		GUILayout.Space(30);
		isPlaneOn = GUILayout.Toggle(isPlaneOn, "Plane On");
		SetPlaneVisible(isPlaneOn);

		GUILayout.Space(30);
		isGravityOn = GUILayout.Toggle(isGravityOn, "Gravity On");
		SetGravity(isGravityOn);
		
		GUILayout.Space(30);
		isControlMouseOn = GUILayout.Toggle(isControlMouseOn, "Control Mouse");
		SetMouseControl(isControlMouseOn);
		
		GUILayout.FlexibleSpace();
		
		resetObjectsClicked = GUILayout.Button("Reset Objects");
		if(resetObjectsClicked)
		{
			//label1Text = "Resetting objects...";
			ResetObjects(resetObjectsClicked);
		}

		GUILayout.Label(label1Text);

		hideWindowClicked = GUILayout.Button("Hide Options");
		if(hideWindowClicked)
		{
			//label2Text = "Hiding options window...";
			HideWindow(hideWindowClicked);
		}
		
		GUILayout.Label(label2Text);
		GUILayout.EndVertical();
		
		// Make the window draggable.
		GUI.DragWindow();
	}
	
	
	void OnGUI()
	{
		if(!hiddenWindow)
		{
			Rect windowRect = guiWindowRect;
			if(windowRect.x < 0)
				windowRect.x += Screen.width;
			if(windowRect.y < 0)
				windowRect.y += Screen.height;
			
			GUI.skin = guiSkin;
			guiWindowRect = GUI.Window(1, windowRect, ShowGuiWindow, "Options");
		}
	}


	// set gravity on or off
	private void SetGravity(bool gravityOn)
	{
		GrabDropScript compGrabDrop = GetComponent<GrabDropScript>();

		if(compGrabDrop != null && compGrabDrop.useGravity != gravityOn)
		{
			compGrabDrop.useGravity = gravityOn;
		}
	}

	// make plane visible or not
	private void SetPlaneVisible(bool planeOn)
	{
		if(planeObj && planeObj.activeInHierarchy != planeOn)
		{
			planeObj.SetActive(planeOn);
		}
	}

	// turn off or on mouse-cursor control
	private void SetMouseControl(bool controlMouseOn)
	{
		InteractionManager manager = InteractionManager.Instance;

		if(manager && manager.IsInteractionInited())
		{
			if(manager.controlMouseCursor != controlMouseOn)
			{
				manager.controlMouseCursor = controlMouseOn;
			}
		}
	}

	// reset objects if needed
	private void ResetObjects(bool resetObjs)
	{
		if(resetObjs)
		{
			GrabDropScript compGrabDrop = GetComponent<GrabDropScript>();
			
			if(compGrabDrop != null)
			{
				compGrabDrop.resetObjects = true;
			}
		}
	}

	// hide options window
	private void HideWindow(bool hideWin)
	{
		if(hideWin)
		{
			hiddenWindow = true;
		}
	}
	

}
