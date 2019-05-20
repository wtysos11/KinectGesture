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


public class SceneMeshVisualizer : MonoBehaviour
{
	[Tooltip("Minimum tracked distance from the sensor, in meters.")]
	public float minDistance = 1f;
	
	[Tooltip("Maximum tracked distance from the sensor, in meters.")]
	public float maxDistance = 3f;
	
	[Tooltip("Maximum left and right distance from the sensor, in meters.")]
	public float maxLeftRight = 2f;
	
	[Tooltip("Whether the mesh is facing the player or not.")]
	public bool mirroredScene = true;
	
	[Tooltip("Kinect origin position.")]
	public Vector3 originPosition = Vector3.zero;
	
	[Tooltip("Whether the z-movement is inverted or not.")]
	public bool invertedZMovement = false;
	
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

	private int colorWidth = 0;
	private int colorHeight = 0;
	
	private int depthWidth = 0;
	private int depthHeight = 0;

	private int sampledWidth = 0;
	private int sampledHeight = 0;

	private int minDepth = 0;
	private int maxDepth = 0;

	private Vector3 sceneMeshPos = Vector3.zero;

	private byte[] vertexType;
	private int[] vertexIndex;


    void Start()
    {
		manager = KinectManager.Instance;

		if (manager != null)
        {
			sensorData = manager.GetSensorData();

			minDepth = Mathf.RoundToInt(minDistance * 1000f);
			maxDepth = Mathf.RoundToInt(maxDistance * 1000f);

			colorWidth = manager.GetColorImageWidth();
			colorHeight = manager.GetColorImageHeight();
			
			depthWidth = manager.GetDepthImageWidth();
			depthHeight = manager.GetDepthImageHeight();
			
			sampledWidth = depthWidth / sampleSize;
			sampledHeight = depthHeight / sampleSize;

			kinectToWorld = manager.GetKinectToWorldMatrix();

			if(sensorData.depth2SpaceCoords == null)
			{
				sensorData.depth2SpaceCoords = new Vector3[depthWidth * depthHeight];
			}

			sceneMeshPos = transform.position;
			if(!mirroredScene)
			{
				sceneMeshPos.x = -sceneMeshPos.x;
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
			GetComponent<Renderer>().material.mainTexture = manager.GetUsersClrTex();
		}

		// update the mesh
		UpdateMesh();
    }
    
    private void UpdateMesh()
    {
		if(sensorData.depthImage != null && sensorData.depth2SpaceCoords != null && 
		   sensorData.depth2ColorCoords != null)
		{
			int vCount = 0, tCount = 0;
			EstimateSceneVertices(out vCount, out tCount);

			vertices = new Vector3[vCount];
			uvs = new Vector2[vCount];
			triangles = new int[6 * tCount];

			int index = 0, vIndex = 0, tIndex = 0, xyIndex = 0;
			for (int y = 0; y < depthHeight; y += sampleSize)
			{
				int xyStartIndex = xyIndex;

				for (int x = 0; x < depthWidth; x += sampleSize)
				{
					Vector3 vSpacePos = sensorData.depth2SpaceCoords[xyIndex];

					if(vertexType[index] != 0 &&
					   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z))
					{
						if(!mirroredScene)
						{
							vSpacePos.x = -vSpacePos.x;
						}

						vSpacePos = kinectToWorld.MultiplyPoint3x4(vSpacePos);  // convert to world coords
						vertices[vIndex] = vSpacePos - sceneMeshPos;

						Vector2 vColorPos = sensorData.depth2ColorCoords[xyIndex];
						if(!float.IsInfinity(vColorPos.x) && !float.IsInfinity(vColorPos.y))
						{
							uvs[vIndex] = new Vector2(Mathf.Clamp01(vColorPos.x / colorWidth), Mathf.Clamp01(vColorPos.y / colorHeight));
						}

						vIndex++;

						if(vertexType[index] == 3)
						{
							if(mirroredScene)
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

	// estimates which and how many sampled vertices are valid
	private void EstimateSceneVertices(out int count1, out int count3)
	{
		System.Array.Clear(vertexType, 0, vertexType.Length);

		Vector3[] vSpacePos = new Vector3[4];
		int rowIndex = 0;

		for (int y = 0; y < sampledHeight - 1; y++)
		{
			int pixIndex = rowIndex;

			for (int x = 0; x < sampledWidth - 1; x++)
			{
				if(IsSceneSampleValid(x, y, ref vSpacePos[0]) && IsSceneSampleValid(x + 1, y, ref vSpacePos[1]) &&
				   IsSceneSampleValid(x, y + 1, ref vSpacePos[2]) && IsSceneSampleValid(x + 1, y + 1, ref vSpacePos[3]))
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

	// checks whether this sample block is valid for the scene
	private bool IsSceneSampleValid(int x, int y, ref Vector3 vSpacePos)
	{
		int pixelIndex = y * sampleSize * depthWidth + x * sampleSize;

		int depth = sensorData.depthImage[pixelIndex];
		vSpacePos = sensorData.depth2SpaceCoords[pixelIndex];

		if(depth >= minDepth && depth <= maxDepth &&
		   !float.IsInfinity(vSpacePos.x) && !float.IsInfinity(vSpacePos.y) && !float.IsInfinity(vSpacePos.z) &&
		   (maxLeftRight < 0f || (vSpacePos.x >= -maxLeftRight && vSpacePos.x <= maxLeftRight)))
		{
			return true;
		} 

		return false;
	}

}
