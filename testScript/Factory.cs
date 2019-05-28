using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//本脚本用来产生随机的Aim，在某个区间范围内产生一定数量的Aim
public class Factory : MonoBehaviour
{
    static public int aimNumber = 10;
    static public int aimRange = 30;//确定AimWeight的范围
    static public float judgeDist = 3.0f;//任意两点的欧式距离不能低于此值
    private float[] coX = new float[aimNumber];
    private float[] coY = new float[aimNumber];
    private int sceneNum;
    private bool gameOver;
    private int case1Num = 0;
    public int case1Report = 0;
    private float endTime = 0;
    private bool isTimeRecord = false;

    public string case3requireName = "";
    public bool case3Over = false;

    GameObject originAim;//目标原型
    void Start()
    {
        sceneNum = gameObject.GetComponent<MyPresentationScript>().sceneNum;
        originAim = Object.Instantiate(Resources.Load("Aim0", typeof(GameObject))) as GameObject;
        originAim.SetActive(false);

        switch (sceneNum){
            case 1:
                {
                    /*
                     任务1：给定场景，在指定时间内找到指定的节点，并且修改它们的值到指定值
                    目标：测试探索速度
                    实现：生成指定数量的aim,并且在一部分aim上修改其needChange =  true，并为其指定一个不等于其weight的aimWeight
                     */
                    float probably = 0.5f;//需要修改的概率
                    int number = 0;
                    while (number < aimNumber)
                    {
                        float x = Random.Range(-1 * aimRange, aimRange);
                        float y = Random.Range(-1 * aimRange, aimRange);
                        bool pass = true;
                        for (int i = 0; i < number; i++)
                        {
                            if (calculateDist(x, y, i))
                            {
                                continue;
                            }
                            else
                            {
                                pass = false;
                                break;
                            }
                        }
                        if (pass)
                        {
                            coX[number] = x;
                            coY[number] = y;
                            //创建对象
                            GameObject newObj = GameObject.Instantiate(originAim);
                            //判断是否需要修改
                            float changeNum = Random.Range(0.0f, 1.0f);
                            if(changeNum < probably)
                            {
                                int plus = 1;
                                //判断修改是否为负
                                float isNegative = Random.Range(0.0f, 1.0f);
                                if(isNegative < 0.5)
                                {
                                    plus = -1;
                                }
                                //判断修改的值
                                float value = Random.Range(1.0f, 20.0f);
                                newObj.GetComponent<AimScript>().needChange = true;
                                newObj.GetComponent<AimScript>().aimWeight = (int)value*plus;
                                newObj.GetComponent<AimScript>().myfactory = this;
                                //Factory负责计数，以及维护场景进行
                                case1Num++;
                            }


                            newObj.GetComponent<AimScript>().name = generateName();
                            newObj.SetActive(true);
                            newObj.transform.position = new Vector3(x, y, 0);
                            //
                            number++;
                        }
                    }
                    break;
                }
            case 2:
                {
                    /*
                     任务2：给定场景，找到所有的点，将所有的点加入到多选列表中
                    目标：测试边界
                    场景控制器监控所有的节点，每次实时监控从MyPresentationScript中的multiObjList，当所有的节点都在这个list中时，弹出时间
                     */
                    int number = 0;
                    while (number < aimNumber)
                    {
                        float x = Random.Range(-1 * aimRange, aimRange);
                        float y = Random.Range(-1 * aimRange, aimRange);
                        bool pass = true;
                        for (int i = 0; i < number; i++)
                        {
                            if (calculateDist(x, y, i))
                            {
                                continue;
                            }
                            else
                            {
                                pass = false;
                                break;
                            }
                        }
                        if (pass)
                        {
                            coX[number] = x;
                            coY[number] = y;
                            //创建对象
                            GameObject newObj = GameObject.Instantiate(originAim);
                            newObj.GetComponent<AimScript>().name = generateName();
                            newObj.SetActive(true);
                            newObj.transform.position = new Vector3(x, y, 0);
                            //
                            number++;
                        }
                    }
                    break;
                }
            case 3:
                {
                    /*
 任务3：给定场景，尝试进行指定的聚类操作
    如果聚类成功（名称符合要求），则弹出时间，不然要重新进行聚类操作。
 */
                    string name = "";
                    int number = 0;
                    while (number < aimNumber)
                    {
                        float x = Random.Range(-1 * aimRange, aimRange);
                        float y = Random.Range(-1 * aimRange, aimRange);
                        bool pass = true;
                        for (int i = 0; i < number; i++)
                        {
                            if (calculateDist(x, y, i))
                            {
                                continue;
                            }
                            else
                            {
                                pass = false;
                                break;
                            }
                        }
                        if (pass)
                        {
                            coX[number] = x;
                            coY[number] = y;
                            //创建对象
                            GameObject newObj = GameObject.Instantiate(originAim);
                            newObj.GetComponent<AimScript>().name = generateName();
                            float enter = Random.Range(0.0f, 1.0f);
                            Debug.Log(enter);
                            if(enter<0.5f)
                            {
                                if(enter<0.25f)
                                {
                                    if(name == "")
                                    {
                                        name = newObj.GetComponent<AimScript>().name;
                                    }
                                    else
                                    {
                                        name = name + " " + newObj.GetComponent<AimScript>().name;
                                    }
                                }
                                else
                                {
                                    if (name == "")
                                    {
                                        name = newObj.GetComponent<AimScript>().name;
                                    }
                                    else
                                    {
                                        name = newObj.GetComponent<AimScript>().name + " " + name;
                                    }
                                }
                            }
                            
                            newObj.SetActive(true);
                            newObj.transform.position = new Vector3(x, y, 0);
                            //
                            number++;
                        }
                    }
                    case3requireName = name;
                    break;
                }
            default:
                {
                    break;
                }
        }

    }

    string generateName()
    {
        int number = (int)Random.Range(3.0f, 5.0f);
        string name = "";
        for(int i = 0;i<number;i++)
        {
            int randomChar = (int)Random.Range(0, 26);
            name += (char)(97 + randomChar);
        }
        return name;
    }

    //坐标(x,y)与list中的第i个元素进行比较，范围是否小于judgeDist
    bool calculateDist(float x,float y,int i)
    {
        if((x - coX[i])*(x - coX[i])+ (y - coY[i]) * (y - coY[i]) > judgeDist * judgeDist)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //在指定位置位置创建对象，并且返回其AimScript
    public AimScript createClusterNode(Vector3 pos)
    {
        GameObject newObj = GameObject.Instantiate(originAim);
        newObj.SetActive(true);
        newObj.transform.position = pos;
        return newObj.GetComponent<AimScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if(case1Report == case1Num && sceneNum == 1)
        {
            gameOver = true;
        }
        else if(sceneNum == 2)
        {
            int len = gameObject.GetComponent<MyPresentationScript>().multiObjList.Count;

            Debug.Log(len);
            if (len == aimNumber)
            {
                gameOver = true;
            }
        }
        else if(sceneNum == 3 && case3Over)
        {
            gameOver = true;
        }
    }

    private void OnGUI()
    {
        if (gameOver)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;
            if(!isTimeRecord)
            {
                endTime = Time.realtimeSinceStartup;
                isTimeRecord = true;
            }
            string ans = "Experiment is over, your time is " + endTime + " s";
            GUI.Label(new Rect(Screen.width / 2 - 50, 20, 70, 70), ans, style);
        }

        if(sceneNum == 3)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.alignment = TextAnchor.MiddleCenter;

            GUI.Label(new Rect(Screen.width / 2 - 50, 40, 70, 70), "require name using cluster:"+case3requireName, style);
        }
    }
}


