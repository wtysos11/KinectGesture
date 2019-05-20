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

public class SetBackgroundImage : MonoBehaviour 
{
	[Tooltip("GUI-texture used to display the color camera feed on the scene background.")]
	public GUITexture backgroundImage;

	[Tooltip("Camera that will be set-up to display 3D-models in the Kinect FOV.")]
	public Camera foregroundCamera;

	[Tooltip("Use this setting to minimize the offset between the image and the model overlay.")]
	[Range(-0.1f, 0.1f)]
	public float adjustedCameraOffset = 0f;
	

	// variable to track the current camera offset
	private float currentCameraOffset = 0f;
	// initial camera position
	private Vector3 initialCameraPos = Vector3.zero;


	void Start()
	{
		KinectManager manager = KinectManager.Instance;
		
		if(manager && manager.IsInitialized())
		{
			KinectInterop.SensorData sensorData = manager.GetSensorData();

			if(foregroundCamera != null && sensorData != null && sensorData.sensorInterface != null)
			{
				foregroundCamera.fieldOfView = sensorData.colorCameraFOV;

				initialCameraPos = foregroundCamera.transform.position;
				Vector3 fgCameraPos = initialCameraPos;

				fgCameraPos.x += sensorData.faceOverlayOffset + adjustedCameraOffset;
				foregroundCamera.transform.position = fgCameraPos;
				currentCameraOffset = adjustedCameraOffset;
			}
		}
	}

	void Update()
	{
		KinectManager manager = KinectManager.Instance;
		if(manager && manager.IsInitialized())
		{
			if(backgroundImage && (backgroundImage.texture == null))
			{
				backgroundImage.texture = manager.GetUsersClrTex();
			}

			if(currentCameraOffset != adjustedCameraOffset)
			{
				// update the camera automatically, according to the current sensor height and angle
				KinectInterop.SensorData sensorData = manager.GetSensorData();
				
				if(foregroundCamera != null && sensorData != null)
				{
					Vector3 fgCameraPos = initialCameraPos;
					fgCameraPos.x += sensorData.faceOverlayOffset + adjustedCameraOffset;
					foregroundCamera.transform.position = fgCameraPos;
					currentCameraOffset = adjustedCameraOffset;
				}
			}
		}
	}

}
