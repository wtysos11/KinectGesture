using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//本脚本用来产生随机的Aim，在某个区间范围内产生一定数量的Aim
public class Factory : MonoBehaviour
{
    static public int aimNumber = 10;
    static public int aimRange = 30;//确定产生Aim的范围
    static public float judgeDist = 3.0f;//任意两点的欧式距离不能低于此值
    private float[] coX = new float[aimNumber];
    private float[] coY = new float[aimNumber];

    GameObject originAim;//目标原型
    void Start()
    {
        originAim = Object.Instantiate(Resources.Load("Aim0", typeof(GameObject))) as GameObject;
        originAim.SetActive(false);
        //产生aimNumber个随机坐标，这些坐标之间的距离大于judgeDist。
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
                Debug.Log(x.ToString()+ y.ToString()+ number.ToString());
                //创建对象
                GameObject newObj = GameObject.Instantiate(originAim);
                newObj.SetActive(true);
                newObj.transform.position = new Vector3(x, y, 0);
                //
                number++;
            }
        }
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

    }
}


