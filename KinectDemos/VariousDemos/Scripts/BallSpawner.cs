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

public class BallSpawner : MonoBehaviour 
{
	[Tooltip("Prefab used to instantiate balls in the scene.")]
    public Transform ballPrefab;

	[Tooltip("Prefab used to instantiate cubes in the scene.")]
	public Transform cubePrefab;
	
	[Tooltip("How many objects do we want to spawn.")]
	public int numberOfObjects = 20;

    private float nextSpawnTime = 0.0f;
    private float spawnRate = 1.5f;
	public int ballsCount = 0;
 	

	void Update () 
	{
        if (nextSpawnTime < Time.time)
        {
            SpawnBalls();
            nextSpawnTime = Time.time + spawnRate;

			spawnRate = Random.Range(0f, 1f);
			//numberOfBalls = Mathf.RoundToInt(Random.Range(1f, 10f));
        }
	}

    void SpawnBalls()
    {
		KinectManager manager = KinectManager.Instance;

		if(ballPrefab && cubePrefab && ballsCount < numberOfObjects &&
		   manager && manager.IsInitialized() && manager.IsUserDetected())
		{
			long userId = manager.GetPrimaryUserID();
			Vector3 posUser = manager.GetUserPosition(userId);

			float xPos = Random.Range(-1.5f, 1.5f);
			float zPos = Random.Range(-1.5f, 1.5f);
			Vector3 spawnPos = new Vector3(posUser.x + xPos, posUser.y, posUser.z + zPos);

			int ballOrCube = Mathf.RoundToInt(Random.Range(0f, 1f));

			Transform ballTransform = Instantiate(ballOrCube > 0 ? ballPrefab : cubePrefab, spawnPos, Quaternion.identity) as Transform;
			ballTransform.GetComponent<Renderer>().material.color = new Color(Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), Random.Range(0.5f, 1f), 1f);
			ballTransform.parent = transform;

			ballsCount++;
		}
    }

}
