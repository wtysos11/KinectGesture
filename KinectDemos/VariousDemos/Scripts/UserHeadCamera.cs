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

public class UserHeadCamera : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. -1 means all players, 0 - the 1st player only, 1 - the 2nd player only, etc.")]
	public int playerIndex = 0;

	[Tooltip("Kinect joint used to control the camera.")]
	public KinectInterop.JointType playerJoint = KinectInterop.JointType.Head;

	[Tooltip("Whether the camera view is mirrored or not.")]
	public bool mirroredView = false;

	[Tooltip("Kinect origin position.")]
	public Vector3 originPosition = Vector3.zero;
	
	[Tooltip("Whether to apply the joint rotation to the camera.")]
	public bool applyJointRotation = false;

	[Tooltip("Initial camera rotation.")]
	public Vector3 initialRotation = Vector3.zero;
	
	[Tooltip("Whether the z-movement is inverted or not.")]
	public bool invertedZMovement = false;
	
	[Tooltip("Smooth factor used for the camera orientation.")]
	public float smoothFactor = 5f;
	

	private KinectManager kinectManager = null;
	private Quaternion initialHeadRot;


	void Start () 
	{
		kinectManager = KinectManager.Instance;
		initialHeadRot = (mirroredView ? Quaternion.Euler(0f, 180f, 0f) : Quaternion.identity) * Quaternion.Euler(initialRotation);
	}
	
	void Update () 
	{
		if(kinectManager && kinectManager.IsInitialized())
		{
			long userId = kinectManager.GetUserIdByIndex(playerIndex);

			if(userId != 0 && kinectManager.IsJointTracked(userId, (int)playerJoint))
			{
				Vector3 headPos = kinectManager.GetJointPosition(userId, (int)playerJoint);
				if(invertedZMovement)
				{
					headPos.z = -headPos.z;
				}

				headPos += originPosition;
				transform.position = headPos + transform.forward * 0.1f;

				if(applyJointRotation)
				{
					Quaternion headRot = kinectManager.GetJointOrientation(userId, (int)playerJoint, !mirroredView);

					Vector3 jointDir = kinectManager.GetJointDirection (userId, (int)playerJoint, mirroredView, invertedZMovement);
					Quaternion invPitchRot = Quaternion.FromToRotation (jointDir, Vector3.up);
					headRot = headRot * invPitchRot;

					transform.rotation = Quaternion.Slerp(transform.rotation, initialHeadRot * headRot, smoothFactor * Time.deltaTime);
				}
			}
		}
	}
}
