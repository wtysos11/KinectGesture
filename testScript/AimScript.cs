using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimScript : MonoBehaviour
{
    public Transform camera;//世界坐标中的位置
    public Camera mainCamera;//这个好像主要是用了一下它里面的一个函数，没有设计其它的东西

    public string name = "wty";//该节点的名称
    public int weight = 1;//记录该节点的权重
    
    public bool isCluster = false;//记录这个节点是否是一个簇
    public int clusterNode = -1;//记录如果这是一个簇，那么它在簇的列表中的下标

    public bool isMultipleMode = false;//

    private GameObject myObject;
    private float height;
    public Vector3 lastPos;

    private bool selected = false;
    // Start is called before the first frame update
    void Start()
    {
        camera = GameObject.Find("Camera").transform;
        mainCamera = Camera.main;
        transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        myObject = this.gameObject;
        height = myObject.GetComponent<Collider>().bounds.size.y * transform.localScale.y;
    }

    //批量修改权值
    public void changeWeight(int delta)
    {
        //如果不是集群，则直接修改
        if(!isCluster)
        {
            weight += delta;
        }
        //如果是集群，则需要访问到原始的列表，并进行操作
        else
        {
            MyPresentationScript.MyClusterType cluster = camera.gameObject.GetComponent<MyPresentationScript>().clusterList[clusterNode] as MyPresentationScript.MyClusterType;
            cluster.addWeight(delta);
        }
    }

    //判断摄像机是否在判定范围之内，如果在，则选中
    bool inRange()
    {
        float dist = 1f;
        if((transform.position.x - camera.position.x)*(transform.position.x - camera.position.x) + (transform.position.y - camera.position.y) * (transform.position.y - camera.position.y) < dist * dist)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(inRange())
        {
            if(!isMultipleMode)
                transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            else
                transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
            selected = true;
            camera.gameObject.GetComponent<MyPresentationScript>().selectedGameobject = myObject;
        }
        else
        {
            if(!isMultipleMode)
                transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            else
                transform.gameObject.GetComponent<MeshRenderer>().material.color = Color.yellow;
            if (selected)
            {
                selected = false;
                camera.gameObject.GetComponent<MyPresentationScript>().selectedGameobject = null;
            }
        }
    }
    //显示名字和权重
    private void OnGUI()
    {
        //得到NPC头顶在3D世界中的坐标
        //默认NPC坐标点在脚底下，所以这里加上npcHeight它模型的高度即可
        Vector3 worldPosition = new Vector3(transform.position.x, transform.position.y + height, transform.position.z);
        //根据NPC头顶的3D坐标换算成它在2D屏幕中的坐标
        Vector2 position = mainCamera.WorldToScreenPoint(worldPosition);
        //得到真实NPC头顶的2D坐标
        position = new Vector2(position.x, Screen.height - position.y);

        //计算NPC名称的宽高
        Vector2 weightSizes = GUI.skin.label.CalcSize(new GUIContent(this.weight.ToString()));
        Vector2 nameSize = GUI.skin.label.CalcSize(new GUIContent(this.name));

        GUI.color = Color.red;
        GUI.skin.label.fontSize = 18;//字体大小
                                     //绘制NPC名称
        GUI.Label(new Rect(position.x - (weightSizes.x / 2), position.y - weightSizes.y, weightSizes.x, weightSizes.y), weight.ToString());
        GUI.Label(new Rect(position.x - (nameSize.x / 2), position.y + 2*nameSize.y, nameSize.x, nameSize.y), name);
    }
}
