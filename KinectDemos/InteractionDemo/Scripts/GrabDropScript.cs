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

public class GrabDropScript : MonoBehaviour 
{
	[Tooltip("List of the objects that may be dragged and dropped.")]
	public GameObject[] draggableObjects;

	[Tooltip("Material used to outline the currently selected object.")]
	public Material selectedObjectMaterial;
	
	[Tooltip("Drag speed of the selected object.")]
	public float dragSpeed = 3.0f;

	[Tooltip("Minimum Z-position of the dragged object, when moving forward and back.")]
	public float minZ = 0f;

	[Tooltip("Maximum Z-position of the dragged object, when moving forward and back.")]
	public float maxZ = 5f;

	// public options (used by the Options GUI)
	[Tooltip("Whether the objects obey gravity when released or not. Used by the Options GUI-window.")]
	public bool useGravity = true;
	[Tooltip("Whether the objects should be put in their original positions. Used by the Options GUI-window.")]
	public bool resetObjects = false;

	[Tooltip("GUI-Text used to display information messages.")]
	public GUIText infoGuiText;


	// interaction manager reference
	private InteractionManager manager;
	private bool isLeftHandDrag;

	// currently dragged object and its parameters
	private GameObject draggedObject;
	//private float draggedObjectDepth;
	private Vector3 draggedObjectOffset;
	private Material draggedObjectMaterial;
	private float draggedNormalZ;

	// initial objects' positions and rotations (used for resetting objects)
	private Vector3[] initialObjPos;
	private Quaternion[] initialObjRot;

	// normalized and pixel position of the cursor
	private Vector3 screenNormalPos = Vector3.zero;
	private Vector3 screenPixelPos = Vector3.zero;
	private Vector3 newObjectPos = Vector3.zero;


	void Start()
	{
		// save the initial positions and rotations of the objects
		initialObjPos = new Vector3[draggableObjects.Length];
		initialObjRot = new Quaternion[draggableObjects.Length];

		for(int i = 0; i < draggableObjects.Length; i++)
		{
			initialObjPos[i] = draggableObjects[i].transform.position;
			initialObjRot[i] = draggableObjects[i].transform.rotation;
		}
	}

	void Update() 
	{
        //确保reset操作进行
		if(resetObjects && draggedObject == null)
		{
			// reset the objects as needed
			resetObjects = false;
			ResetObjects ();
		}

		// get the interaction manager instance 工厂模式 manager
		if(manager == null)
		{
			manager = InteractionManager.Instance;
		}
        
		if(manager != null && manager.IsInteractionInited())//manager已经被初始化
		{
			if(draggedObject == null)//如果当前没有抓握对象
			{
				screenNormalPos = Vector3.zero;
				screenPixelPos = Vector3.zero;

				// if there is a hand grip, select the underlying object and start dragging it.
				if(manager.IsLeftHandPrimary())//存在左手
				{
					// if the left hand is primary, check for left hand grip
					if(manager.GetLastLeftHandEvent() == InteractionManager.HandEventType.Grip) //左手有抓握动作
					{
						isLeftHandDrag = true;
						screenNormalPos = manager.GetLeftHandScreenPos();
					}
				}
				else if(manager.IsRightHandPrimary())//存在右手
				{
					// if the right hand is primary, check for right hand grip
					if(manager.GetLastRightHandEvent() == InteractionManager.HandEventType.Grip)//右手有抓握动作
					{
						isLeftHandDrag = false;
						screenNormalPos = manager.GetRightHandScreenPos();
					}
				}
				
				// check if there is an underlying object to be selected
				if(screenNormalPos != Vector3.zero)//screenNormalPos在有抓握动作的时候非零，这时候意味着存在抓握动作
				{
					// convert the normalized screen pos to pixel pos 将屏幕坐标转变为摄像机的像素坐标
					screenPixelPos.x = (int)(screenNormalPos.x * Camera.main.pixelWidth);
					screenPixelPos.y = (int)(screenNormalPos.y * Camera.main.pixelHeight);
					Ray ray = Camera.main.ScreenPointToRay(screenPixelPos);//用这个像素坐标来发射射线
					
					// check if there is an underlying objects
					RaycastHit hit;
					if(Physics.Raycast(ray, out hit))
					{
						foreach(GameObject obj in draggableObjects)
						{
							if(hit.collider.gameObject == obj)
							{
								// an object was hit by the ray. select it and start drgging 如果对象被射线集中，则选择它并开始拖拽
								draggedObject = obj;
								//draggedObjectDepth = draggedObject.transform.position.z - Camera.main.transform.position.z;
								draggedObjectOffset = hit.point - draggedObject.transform.position;
								draggedObjectOffset.z = 0; // don't change z-pos

								draggedNormalZ = (minZ + screenNormalPos.z * (maxZ - minZ)) - 
									draggedObject.transform.position.z; // start from the initial hand-z
								
								// set selection material
								draggedObjectMaterial = draggedObject.GetComponent<Renderer>().material;
								draggedObject.GetComponent<Renderer>().material = selectedObjectMaterial;

								// stop using gravity while dragging object
								draggedObject.GetComponent<Rigidbody>().useGravity = false;
								break;
							}
						}
					}
				}
				
			}
			else//如果当前存在抓握对象
			{
				// continue dragging the object 继续抓握对象（可能是左手或者右手）
				screenNormalPos = isLeftHandDrag ? manager.GetLeftHandScreenPos() : manager.GetRightHandScreenPos();
				
				// convert the normalized screen pos to 3D-world pos
				screenPixelPos.x = (int)(screenNormalPos.x * Camera.main.pixelWidth);
				screenPixelPos.y = (int)(screenNormalPos.y * Camera.main.pixelHeight);
				//screenPixelPos.z = screenNormalPos.z + draggedObjectDepth;
				screenPixelPos.z = (minZ + screenNormalPos.z * (maxZ - minZ)) - draggedNormalZ -
					Camera.main.transform.position.z;

				newObjectPos = Camera.main.ScreenToWorldPoint(screenPixelPos) - draggedObjectOffset;
				draggedObject.transform.position = Vector3.Lerp(draggedObject.transform.position, newObjectPos, dragSpeed * Time.deltaTime);
				
				// check if the object (hand grip) was released
				bool isReleased = isLeftHandDrag ? (manager.GetLastLeftHandEvent() == InteractionManager.HandEventType.Release) :
					(manager.GetLastRightHandEvent() == InteractionManager.HandEventType.Release);
				
				if(isReleased)
				{
					// restore the object's material and stop dragging the object
					draggedObject.GetComponent<Renderer>().material = draggedObjectMaterial;

					if(useGravity)
					{
						// add gravity to the object
						draggedObject.GetComponent<Rigidbody>().useGravity = true;
					}

					draggedObject = null;
				}
			}
		}
	}

	// reset positions and rotations of the objects
	private void ResetObjects()
	{
		for(int i = 0; i < draggableObjects.Length; i++)
		{
			draggableObjects[i].GetComponent<Rigidbody>().useGravity = false;
			draggableObjects[i].GetComponent<Rigidbody>().velocity = Vector3.zero;

			draggableObjects[i].transform.position = initialObjPos[i];
			draggableObjects[i].transform.rotation = initialObjRot[i];
		}
	}
	
	void OnGUI()
	{
		if(infoGuiText != null && manager != null && manager.IsInteractionInited())
		{
			string sInfo = string.Empty;
			
			long userID = manager.GetUserID();
			if(userID != 0)
			{
				if(draggedObject != null)
					sInfo = "Dragging the " + draggedObject.name + " around.";
				else
					sInfo = "Please grab and drag an object around.";
			}
			else
			{
				KinectManager kinectManager = KinectManager.Instance;

				if(kinectManager && kinectManager.IsInitialized())
				{
					sInfo = "Waiting for Users...";
				}
				else
				{
					sInfo = "Kinect is not initialized. Check the log for details.";
				}
			}
			
			infoGuiText.GetComponent<GUIText>().text = sInfo;
		}
	}
	
}
