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


public class UserMeshVisualizer : MonoBehaviour
{
	[Tooltip("Index of the player, tracked by this component. -1 means all players, 0 - the 1st player only, 1 - the 2nd player only, etc.")]
	public int playerIndex = -1;
	
	[Tooltip("Whether the mesh is facing the player or not.")]
	public bool mirroredMovement = true;
	
	[Tooltip("Kinect origin position.")]
	public Vector3 originPosition = Vector3.zero;
	
	[Tooltip("Whether the z-movement is inverted or not.")]
	public bool invertedZMovement = false;
	
	[Tooltip("Smooth factor used for the camera re-orientation.")]
	public float smoothFactor = 10f;
	
	[Tooltip("Number of pixels per direction in a sample.")]
	private const int sampleSize = 2;
	

    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

	private KinectManager manager = null;

	private KinectInterop.SensorData sensorData = null;
	//private Vector3[] spaceCoords = null;
	private Matrix4x4 kinectToWorld = Matrix4x4.identity;

	private int depthWidth = 0;
	private int depthHeight = 0;

	private int sampledWidth = 0;
	private int sampledHeight = 0;

	private long userId = 0;
	private byte userBodyIndex = 255;
	private Vector3 userMeshPos = Vector3.zero;

	private byte[] vertexType;
	private int[] vertexIndex;


    void Start()
    {
		manager = KinectManager.Instance;

		if (manager != null)
        {
			sensorData = manager.GetSensorData();

			depthWidth = manager.GetDepthImageWidth();
			depthHeight = manager.GetDepthImageHeight();
			
			sampledWidth = depthWidth / sampleSize;
			sampledHeight = depthHeight / sampleSize;

			kinectToWorld = manager.GetKinectToWorldMatrix();
			//spaceCoords = new Vector3[depthWidth * depthHeight];

			if(sensorData.depth2SpaceCoords == null)
			{
				sensorData.depth2SpaceCoords = new Vector3[depthWidth * depthHeight];
			}

			vertexType = new byte[sampledWidth * sampledHeight];
			vertexIndex = new int[sampledWidth * sampledHeight];

			CreateMesh(sampledWidth, sampledHeight);
        }
    }

    private void CreateMesh(int width, int height)
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    void Update()
    {
		if (manager == null || !manager.IsInitialized())
			return;
		
		// get user texture
		if(GetComponent<Renderer>().material.mainTexture == null)
		{
			GetComponent<Renderer>().material.mainTexture = manager.GetUsersLblTex();
		}

		if(playerIndex >= 0)
		{
			userId = manager.GetUserIdByIndex(playerIndex);
			userBodyIndex = (byte)manager.GetBodyIndexByUserId(userId);

			if(userBodyIndex == 255)
				userBodyIndex = 222;

			// get user mesh position
			userMeshPos = manager.GetJointKinectPosition(userId, (int)KinectInterop.JointType.SpineBase);

			if(!mirroredMovement)
			{
				userMeshPos.x = -userMeshPos.x;
			}

			userMeshPos = kinectToWorld.MultiplyPoint3x4(userMeshPos);  // convert to world coords

			// set transform position
			Vector3 newUserPos = manager.GetJointPosition(userId, (int)KinectInterop.JointType.SpineBase) + originPosition;
			
			if(invertedZMovement)
			{
				newUserPos.z = -newUserPos.z;
			}
			
			transform.position = Vector3.Lerp(transform.position, newUserPos, smoothFactor * Time.deltaTime);
		}
		else
		{
			userId = 0;
			userBodyIndex = 255;
			userMeshPos = Vector3.zero;
		}

		// update the mesh
		UpdateMesh();
    }
    
    private void UpdateMesh()
    {
		if(sensorData.depthImage != null && sensorData.bodyIndexImage != null &&
		   //sensorData.sensorInterface.MapDepthFrameToSpaceCoords(sensorData, ref spaceCoords))
		   sensorData.spaceCoordsBufferReady)
		{
			int vCount = 0, tCount = 0;
			EstimateUserVertices(out vCount, out tCount);

			vertices = new Vector3[vCount];
			uvs = new Vector2[vCount];
			triangles = new int[6 * tCount];

			int index = 0, vIndex = 0, tIndex = 0, xyIndex = 0;
			for (int y = 0; y < depthHeight; y += sampleSize)
			{
				int xyStartIndex = xyIndex;

				for (int x = 0; x < depthWidth; x += sampleSize)
				{
					//Vector3 vSpacePos = spaceCoords[xyIndex];
					Vector3 vSpacePos = sensorData.depth2SpaceCoords[xyIndex];

					if(vertexType[index] != 0 &&
					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
					{
						if(!mirroredMovement)
						{
							vSpacePos.x = -vSpacePos.x;
						}

						vSpacePos = kinectToWorld.MultiplyPoint3x4(vSpacePos);  // convert to world coords

						vertices[vIndex] = vSpacePos - userMeshPos;
						uvs[vIndex] = new Vector2((float)x / depthWidth, (float)y / depthHeight);
						vIndex++;

						if(vertexType[index] == 3)
						{
							if(mirroredMovement)
							{
								triangles[tIndex++] = vertexIndex[index];  // top left
								triangles[tIndex++] = vertexIndex[index + 1];  // top right
								triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom left
								
								triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom left
								triangles[tIndex++] = vertexIndex[index + 1];  // top right
								triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom right
							}
							else
							{
								triangles[tIndex++] = vertexIndex[index + 1];  // top left
								triangles[tIndex++] = vertexIndex[index];  // top right
								triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom left
								
								triangles[tIndex++] = vertexIndex[index + sampledWidth + 1];  // bottom left
								triangles[tIndex++] = vertexIndex[index];  // top right
								triangles[tIndex++] = vertexIndex[index + sampledWidth];  // bottom right
							}
						}
					}

					index++;
					xyIndex += sampleSize;
				}

				xyIndex = xyStartIndex + sampleSize * depthWidth;
			}

			// buffer is released
			lock(sensorData.spaceCoordsBufferLock)
			{
				sensorData.spaceCoordsBufferReady = false;
			}

			mesh.Clear();
			mesh.vertices = vertices;
			mesh.uv = uvs;
			//mesh.normals = normals;
			mesh.triangles = triangles;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
		}
    }

//	// gets the average depth of the sample block
//    private ushort GetSampleDepth(int x, int y)
//    {
//		int depthSum = 0, count = 0;
//		int startIndex = y * depthWidth + x;
//
//		//for (int y1 = 0; y1 < SampleSize; y1++)
//        {
//			int pixelIndex = startIndex;
//			
//			//for (int x1 = 0; x1 < SampleSize; x1++)
//            {
//				//if(sensorData.bodyIndexImage[pixelIndex] != 255)
//				{
//					//if(userBodyIndex == 255 || sensorData.bodyIndexImage[pixelIndex] == userBodyIndex)
//					{
//						depthSum += sensorData.depthImage[pixelIndex];
//						count++;
//					}
//				}
//
//				pixelIndex++;
//            }
//
//			pixelIndex += depthWidth;
//        }
//
//		return (ushort)(count > 0 ? (count > 1 ? depthSum / count : depthSum) : 0);
//    }


	// estimates which and how many sampled vertices are valid
	private void EstimateUserVertices(out int count1, out int count3)
	{
		System.Array.Clear(vertexType, 0, vertexType.Length);

		Vector3[] vSpacePos = new Vector3[4];
		int rowIndex = 0;

		for (int y = 0; y < sampledHeight - 1; y++)
		{
			int pixIndex = rowIndex;

			for (int x = 0; x < sampledWidth - 1; x++)
			{
				if(IsUserSampleValid(x, y, ref vSpacePos[0]) && IsUserSampleValid(x + 1, y, ref vSpacePos[1]) &&
				   IsUserSampleValid(x, y + 1, ref vSpacePos[2]) && IsUserSampleValid(x + 1, y + 1, ref vSpacePos[3]))
				{
					if(IsSpacePointsClose(vSpacePos, 0.01f))
					{
						vertexType[pixIndex] = 3;
						
						vertexType[pixIndex + 1] = 1;
						vertexType[pixIndex + sampledWidth] = 1;
						vertexType[pixIndex + sampledWidth + 1] = 1;
					}
				}

				pixIndex++;
			}

			rowIndex += sampledWidth;
		}

		// estimate counts
		count1 = 0;
		count3 = 0;
		
		for(int i = 0; i < vertexType.Length; i++)
		{
			if(vertexType[i] != 0)
			{
				vertexIndex[i] = count1;
				count1++;
			}
			else
			{
				vertexIndex[i] = 0;
			}

			if(vertexType[i] == 3)
			{
				count3++;
			}
		}
	}

	// checks if the space points are closer to each other than the minimum squared distance
	private bool IsSpacePointsClose(Vector3[] vSpacePos, float fMinDistSquared)
	{
		int iPosLength = vSpacePos.Length;

		for(int i = 0; i < iPosLength; i++)
		{
			for(int j = i + 1; j < iPosLength; j++)
			{
				Vector3 vDist = vSpacePos[j] - vSpacePos[i];
				if(vDist.sqrMagnitude > fMinDistSquared)
					return false;
			}
		}

		return true;
	}

	// checks whether this sample block is valid for this user
	private bool IsUserSampleValid(int x, int y, ref Vector3 vSpacePos)
	{
		int startIndex = y * sampleSize * depthWidth + x * sampleSize;

		//for (int y1 = 0; y1 < SampleSize; y1++)
		{
			int pixelIndex = startIndex;
			//vSpacePos = spaceCoords[pixelIndex];
			vSpacePos = sensorData.depth2SpaceCoords[pixelIndex];

			//for (int x1 = 0; x1 < SampleSize; x1++)
			{
				if(userBodyIndex != 255)
				{
					if(sensorData.bodyIndexImage[pixelIndex] == userBodyIndex &&
					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
					{
						return true;
					}
				}
				else
				{
					if(sensorData.bodyIndexImage[pixelIndex] != 255 &&
					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
					{
						return true;
					}
				}

				pixelIndex++;
			}

			startIndex += depthWidth;
		}
		
		return false;
	}

}
