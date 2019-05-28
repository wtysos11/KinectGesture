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
using System.Collections.Generic;

public class MyPresentationScript : MonoBehaviour
{
    //用于记录的例子
    public class MyClusterType
    {
        ArrayList list = new ArrayList();//store AimScript
        public GameObject gb;
        public AimScript aim;

        public void addElement(AimScript a)
        {
            list.Add(a);
        }

        public void deleteElement(AimScript a)
        {
            list.Remove(a);
        }

        public void addWeight(int delta)//对所有节点进行集合修改
        {
            foreach(AimScript a in list)
            {
                a.changeWeight(delta);
            }
        }
        public int getWeight()
        {
            int sum = 0;
            foreach (AimScript a in list)
            {
                sum += a.weight;
            }
            return sum;
        }
        public string getName()
        {
            string name = "";
            foreach (AimScript a in list)
            {
                if(name == "")
                {
                    name = a.name;
                }
                else
                {
                    name += " " + a.name;
                }
            }
            //如果场景为3的时候，设置胜利条件
            if(gb.GetComponent<MyPresentationScript>().sceneNum == 3)
            {
                if(name == gb.GetComponent<Factory>().case3requireName)
                {
                    gb.GetComponent<Factory>().case3Over = true;
                }
            }
            return name;
        }
        //自爆，将所有内部存储的AimScript恢复，不负责删除旧有节点
        public void deleteMyself()
        {
            foreach(AimScript a in list)
            {
                AimScript newScript = gb.GetComponent<Factory>().createClusterNode(a.lastPos);
                newScript.name = a.name;
                newScript.weight = a.weight;
                newScript.isCluster = a.isCluster;
                newScript.clusterNode = a.clusterNode;

            }
        }

    }
    [Tooltip("Speed of running, when presentation slides change.")]
    public float runningSpeed = 10;
    public bool usingVR = false;
    public int sceneNum = 2;//有三个场景，场景1为在指定时间内修改指定aim的值。场景2为给定指定场景，找到所有的点。场景3为给定场景，进行指定的聚类操作。

    // reference to the gesture listener
    private MyGestureListener gestureListener;
    public GameObject selectedGameobject;
    public ArrayList multiObjList = new ArrayList();
    public ArrayList clusterList = new ArrayList();
    private bool esc = false;
    private float lockFrontMax = -3.0f;
    //方向灵敏度  
    private float sensitivityX = 2F;
    private float sensitivityY = 0.5F;

    //上下最大视角(Y视角)  
    private float minimumY = -30F;
    private float maximumY = 30F;
    private bool front = false;
    private bool back = false;
    void Start()
    {
        // hide mouse cursor
        Cursor.visible = false;
        this.selectedGameobject = null;

        // get the gestures listener
        gestureListener = MyGestureListener.Instance;
        // Make the rigid body not change rotation  
        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        if (rigidbody)
        {
            rigidbody.freezeRotation = true;
        }
    }

    void Update()
    {
        //判断是否选中了某个物体

        //以下为移动判断
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            esc = !esc;
        }
        // dont run Update() if there is no gesture listener
        if (usingVR)
        {
            //Debug.Log("usingVR...\n");
            if (!gestureListener)
                return;

            if (gestureListener.IsRaisingRightHand())
            {
                transform.Translate(0, 0, runningSpeed * Time.deltaTime);
            }
            if (gestureListener.IsRaisingLeftHand())
            {
                transform.Translate(0, 0, -1 * runningSpeed * Time.deltaTime);
            }


            if (gestureListener.getPosX() != 0 || gestureListener.getPosY() != 0)
            {
                //根据鼠标移动的快慢(增量), 获得相机左右旋转的角度(处理X)  
                float indexX = 0, indexY = 0;
                if (gestureListener.getPosX() > 0.1f)
                {
                    indexX = 1;
                }
                else if (gestureListener.getPosX() < -0.1f)
                {
                    indexX = -1;
                }
                else
                {
                    indexX = 0;
                }

                if (gestureListener.getPosY() > 0.1f)
                {
                    indexY = 1;
                }
                else if (gestureListener.getPosY() < -0.1f)
                {
                    indexY = -1;
                }
                else
                {
                    indexY = 0;
                }
                float rotationX = transform.localEulerAngles.y + indexX * sensitivityX;
                //根据鼠标移动的快慢(增量), 获得相机上下旋转的角度(处理Y)  
                float yAngle = transform.localEulerAngles.x;
                if (yAngle > 180)
                {
                    yAngle -= 360.0f;
                }
                float rotationY = yAngle + indexY * sensitivityY;
                rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

                if (!esc) //总体设置一下相机角度
                {
                    transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
                }
            }

            if (gestureListener.IsRightHandPushFront())
            {
                if (!this.selectedGameobject.GetComponent<AimScript>().isMultipleMode)
                {
                    this.selectedGameobject.GetComponent<AimScript>().isMultipleMode = true;
                    multiObjList.Add(this.selectedGameobject);
                }
                else
                {
                    this.selectedGameobject.GetComponent<AimScript>().isMultipleMode = false;
                    multiObjList.Remove(this.selectedGameobject);
                }
            }

            if (gestureListener.IsHandsFrontChest())
            {
                if(this.selectedGameobject == null || !this.selectedGameobject.GetComponent<AimScript>().isCluster)
                {
                    //  记录所有旧有位置的坐标到AimScript中，将其存储，并删除所有旧有的物体，在原地产生新的物体。
                    if (this.multiObjList.Count > 0)
                    {
                        //新建类型，并将原有的数据进行转移
                        MyClusterType cluster = new MyClusterType();
                        cluster.gb = gameObject;
                        foreach (GameObject gobject in multiObjList)
                        {
                            AimScript newScript = new AimScript();
                            newScript.weight = gobject.GetComponent<AimScript>().weight;
                            newScript.name = gobject.GetComponent<AimScript>().name;
                            newScript.lastPos = gobject.transform.position;
                            //如果对面是集群
                            newScript.isCluster = gobject.GetComponent<AimScript>().isCluster;
                            newScript.clusterNode = gobject.GetComponent<AimScript>().clusterNode;
                           // 重现是不继承多选按钮isMultipleMode
                            cluster.addElement(newScript);
                           // 删除原始节点
                            Destroy(gobject);
                        }
                        multiObjList.Clear();
                      //  将集群加入到列表中
                        int clusterNode = this.clusterList.Count;
                        clusterList.Add(cluster);
                      //  创建新的节点，并且记录集群信息
                        AimScript nScript = this.GetComponent<Factory>().createClusterNode(new Vector3(this.transform.position.x, this.transform.position.y, 0));
                        nScript.isCluster = true;
                        nScript.clusterNode = clusterNode;
                        nScript.weight = (clusterList[clusterNode] as MyClusterType).getWeight();
                        nScript.name = (clusterList[clusterNode] as MyClusterType).getName();
                    }

                }
                if (this.selectedGameobject.GetComponent<AimScript>().isCluster)
                {
                 //   找到ClusterNode对应的集群，执行解散命令
                     MyClusterType cluster = clusterList[this.selectedGameobject.GetComponent<AimScript>().clusterNode] as MyClusterType;
                    cluster.deleteMyself();
                    Destroy(this.selectedGameobject);
                }
            }

            if (gestureListener.IsRightElbowUp() && this.selectedGameobject != null)//改变权值
            {
              //  如果目标是一个集群
               if (this.selectedGameobject.GetComponent<AimScript>().isCluster)
               {
                   int clusterNode = this.selectedGameobject.GetComponent<AimScript>().clusterNode;
                    (clusterList[clusterNode] as MyClusterType).addWeight(1); 
                   this.selectedGameobject.GetComponent<AimScript>().weight = (clusterList[clusterNode] as MyClusterType).getWeight();
               }
              // 如果目标是一个单体
               else
               {   
                    this.selectedGameobject.GetComponent<AimScript>().changeWeight(1); 
               }

            }
            //如果是
            else if(gestureListener.IsRightElbowUp() &&this.multiObjList.Count > 0)
            {
                foreach (GameObject gb in multiObjList)
                {
                    gb.GetComponent<AimScript>().changeWeight(1);
                }
            }

            if (gestureListener.IsRightElbowDown() && this.selectedGameobject != null)//改变权值
            {
              //  如果目标是一个集群
               if (this.selectedGameobject.GetComponent<AimScript>().isCluster)
               {
                   int clusterNode = this.selectedGameobject.GetComponent<AimScript>().clusterNode;
                    (clusterList[clusterNode] as MyClusterType).addWeight(-1); 
                   this.selectedGameobject.GetComponent<AimScript>().weight = (clusterList[clusterNode] as MyClusterType).getWeight();
               }
              // 如果目标是一个单体
               else
               {   
                    this.selectedGameobject.GetComponent<AimScript>().changeWeight(-1); 
               }

            }
            //如果是
            else if(gestureListener.IsRightElbowDown() &&this.multiObjList.Count > 0)
            {
                foreach (GameObject gb in multiObjList)
                {
                    gb.GetComponent<AimScript>().changeWeight(-1);
                }
            }
               
        }
        else
        {
            //锁定鼠标，不让它乱移动
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            float sensitivity = 1f;//四向移动灵敏度
            float frontSensitivity = 10f;//前后移动灵敏度

            //四向移动计算
            float movingUpdown = Input.GetAxis("Mouse Y") * runningSpeed * sensitivity;
            float movingLeftright = Input.GetAxis("Mouse X") * runningSpeed * sensitivity;

            float transY = transform.position.y + movingUpdown * Time.deltaTime;
            float transX = transform.position.x + movingLeftright * Time.deltaTime;
            float transZ = transform.position.z;


            //按F使当前选中物体进入多选状态（变黄），将其加入到自身的选择列表中
            if (Input.GetKeyDown(KeyCode.F) && this.selectedGameobject != null)
            {
                if (!this.selectedGameobject.GetComponent<AimScript>().isMultipleMode)
                {
                    this.selectedGameobject.GetComponent<AimScript>().isMultipleMode = true;
                    multiObjList.Add(this.selectedGameobject);
                }
                else
                {
                    this.selectedGameobject.GetComponent<AimScript>().isMultipleMode = false;
                    multiObjList.Remove(this.selectedGameobject);
                }
            }
            //按C聚类。合并所有物体的名称，变为一个名称。将所有物体的权值相加，变为最终权值。删除原有的所有对象，在摄像机的位置创建新的对象。
            if (Input.GetKeyDown(KeyCode.C) && (this.selectedGameobject == null || !this.selectedGameobject.GetComponent<AimScript>().isCluster))
            {
                //记录所有旧有位置的坐标到AimScript中，将其存储，并删除所有旧有的物体，在原地产生新的物体。
                if (this.multiObjList.Count > 0)
                {
                    //新建类型，并将原有的数据进行转移
                    MyClusterType cluster = new MyClusterType();
                    cluster.gb = gameObject;
                    foreach (GameObject gobject in multiObjList)
                    {
                        AimScript newScript = new AimScript();
                        newScript.weight = gobject.GetComponent<AimScript>().weight;
                        newScript.name = gobject.GetComponent<AimScript>().name;
                        newScript.lastPos = gobject.transform.position;
                        //如果对面是集群
                        newScript.isCluster = gobject.GetComponent<AimScript>().isCluster;
                        newScript.clusterNode = gobject.GetComponent<AimScript>().clusterNode;
                        //重现是不继承多选按钮isMultipleMode
                        cluster.addElement(newScript);
                        //删除原始节点
                        Destroy(gobject);
                    }
                    multiObjList.Clear();
                    //将集群加入到列表中
                    int clusterNode = this.clusterList.Count;
                    clusterList.Add(cluster);
                    //创建新的节点，并且记录集群信息
                    AimScript nScript = this.GetComponent<Factory>().createClusterNode(new Vector3(this.transform.position.x, this.transform.position.y, 0));
                    nScript.isCluster = true;
                    nScript.clusterNode = clusterNode;
                    nScript.weight = (clusterList[clusterNode] as MyClusterType).getWeight();
                    nScript.name = (clusterList[clusterNode] as MyClusterType).getName();
                }


            }
            //如果对象已经是一个集群了，再次按下C键就会将集群解散。
            else if (Input.GetKeyDown(KeyCode.C) && this.selectedGameobject.GetComponent<AimScript>().isCluster)
            {
                //找到ClusterNode对应的集群，执行解散命令
                MyClusterType cluster = clusterList[this.selectedGameobject.GetComponent<AimScript>().clusterNode] as MyClusterType;
                cluster.deleteMyself();
                Destroy(this.selectedGameobject);
            }
            //按E前进，按Q后退
            if (Input.GetKeyDown(KeyCode.Q) && !front)
            {
                back = true;
            }
            else if (Input.GetKeyDown(KeyCode.E) && !back)
            {
                front = true;
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                back = false;
            }
            else if (Input.GetKeyUp(KeyCode.E))
            {
                front = false;
            }
            //进行前后的具体的修改（持续修改，抬起按键后才停止）
            if (back)
            {
                transZ -= Time.deltaTime * frontSensitivity;
            }
            if (front)
            {
                transZ += Time.deltaTime * frontSensitivity;
            }

            if (transZ > lockFrontMax)
            {
                transZ = lockFrontMax;
            }

            //鼠标滚轮可以改变指定物体的权重。如果有多选应该进行批量修改
            if (Input.GetAxis("Mouse ScrollWheel") != 0 && this.selectedGameobject != null)
            {
                //如果目标是一个集群
                if (this.selectedGameobject.GetComponent<AimScript>().isCluster)
                {
                    int clusterNode = this.selectedGameobject.GetComponent<AimScript>().clusterNode;
                    if (Input.GetAxis("Mouse ScrollWheel") > 0)
                        (clusterList[clusterNode] as MyClusterType).addWeight(1);
                    else
                        (clusterList[clusterNode] as MyClusterType).addWeight(-1);
                    this.selectedGameobject.GetComponent<AimScript>().weight = (clusterList[clusterNode] as MyClusterType).getWeight();
                }
                //如果目标是一个单体
                else
                {
                    if (Input.GetAxis("Mouse ScrollWheel") > 0)
                    {
                        this.selectedGameobject.GetComponent<AimScript>().changeWeight(1);
                    }
                    else
                    {
                        this.selectedGameobject.GetComponent<AimScript>().changeWeight(-1);
                    }
                }


            }
            else if (Input.GetAxis("Mouse ScrollWheel") != 0 && this.multiObjList.Count > 0)
            {
                int delta = -1;
                if (Input.GetAxis("Mouse ScrollWheel") > 0)
                {
                    delta = 1;
                }
                foreach (GameObject gb in multiObjList)
                {
                    gb.GetComponent<AimScript>().changeWeight(delta);
                }
            }
            transform.position = new Vector3(transX, transY, transZ);

        }


         

    }

}
