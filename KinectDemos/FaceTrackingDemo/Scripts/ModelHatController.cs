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

public class ModelHatController : MonoBehaviour 
{
	[Tooltip("Camera that will be used to overlay the 3D-objects over the background.")]
	public Camera foregroundCamera;
	
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;
	
	[Tooltip("Vertical offset of the hat above the head position (in meters).")]
	public float verticalOffset = 0f;

	[Tooltip("Scale factor for the model.")]
	[Range(0.1f, 2.0f)]
	public float modelScaleFactor = 1f;
	
	[Tooltip("Smooth factor used for hat-model rotation.")]
	public float smoothFactorRotation = 10f;

	[Tooltip("Smooth factor used for hat-model movement.")]
	public float smoothFactorMovement = 0f;
	
	private KinectManager kinectManager;
	private FacetrackingManager faceManager;
	private Quaternion initialRotation;


	void Start () 
	{
		initialRotation = transform.rotation;
		transform.localScale = new Vector3(modelScaleFactor, modelScaleFactor, modelScaleFactor);
	}
	
	void Update () 
	{
		// get the face-tracking manager instance
		if(faceManager == null)
		{
			kinectManager = KinectManager.Instance;
			faceManager = FacetrackingManager.Instance;
		}

		if(kinectManager && kinectManager.IsInitialized() && 
		   faceManager && faceManager.IsTrackingFace() && foregroundCamera)
		{
			// get user-id by user-index
			long userId = kinectManager.GetUserIdByIndex(playerIndex);
			if(userId == 0)
				return;

			// get head position
			Vector3 newPosition = faceManager.GetHeadPosition(userId, true);
			
			// get head rotation
			Quaternion newRotation = initialRotation * faceManager.GetHeadRotation(userId, true);

			// rotational fix, provided by Richard Borys:
			// The added rotation fixes rotational error that occurs when person is not centered in the middle of the kinect
			Vector3 addedRotation = newPosition.z != 0f ? new Vector3(Mathf.Rad2Deg * (Mathf.Tan(newPosition.y) / newPosition.z),
				Mathf.Rad2Deg * (Mathf.Tan(newPosition.x) / newPosition.z), 0) : Vector3.zero;
			
			addedRotation.x = newRotation.eulerAngles.x + addedRotation.x;
			addedRotation.y = newRotation.eulerAngles.y + addedRotation.y;
			addedRotation.z = newRotation.eulerAngles.z + addedRotation.z;
			
			newRotation = Quaternion.Euler(addedRotation.x, addedRotation.y, addedRotation.z);
			// end of rotational fix, provided by Richard Borys

			if(smoothFactorRotation != 0f)
				transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, smoothFactorRotation * Time.deltaTime);
			else
				transform.rotation = newRotation;

			// get the background rectangle (use the portrait background, if available)
			Rect backgroundRect = foregroundCamera.pixelRect;
			PortraitBackground portraitBack = PortraitBackground.Instance;
			
			if(portraitBack && portraitBack.enabled)
			{
				backgroundRect = portraitBack.GetBackgroundRect();
			}
			
			// model position
			newPosition = kinectManager.GetJointPosColorOverlay(userId, (int)KinectInterop.JointType.Head, foregroundCamera, backgroundRect);
			if(newPosition == Vector3.zero)
			{
				// hide the model behind the camera
				newPosition.z = -10f;
			}
			
			if(verticalOffset != 0f)
			{
				// add the vertical offset
				Vector3 dirHead = new Vector3(0, verticalOffset, 0);
				dirHead = transform.InverseTransformDirection(dirHead);
				newPosition += dirHead;
			}

			// go to the new position
			if(smoothFactorMovement != 0f && transform.position.z >= 0f)
				transform.position = Vector3.Lerp(transform.position, newPosition, smoothFactorMovement * Time.deltaTime);
			else
				transform.position = newPosition;

			// scale the model if needed
			if(transform.localScale.x != modelScaleFactor)
			{
				transform.localScale = new Vector3(modelScaleFactor, modelScaleFactor, modelScaleFactor);
			}
		}
		else
		{
			// hide the model behind the camera
			if(transform.position.z >= 0f)
			{
				transform.position = new Vector3(0f, 0f, -10f);
			}
		}
	}

}
