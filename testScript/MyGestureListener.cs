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
using System;
//using Windows.Kinect;

public class MyGestureListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
    [Tooltip("GUI-Text to display gesture-listener messages and gesture information.")]
    public GUIText gestureInfo;

    // singleton instance of the class
    private static MyGestureListener instance = null;

    // internal variables to track if progress message has been displayed
    private bool progressDisplayed;
    private float progressGestureTime;

    // whether the needed gesture has been detected or not
    private bool raisingRightHand = false;
    private bool raisingLeftHand = false;
    private bool pushFront = false;

    private float posX = 0.0f;
    private float posY = 0.0f;

    /// <summary>
    /// Gets the singleton ModelGestureListener instance.
    /// </summary>
    /// <value>The ModelGestureListener instance.</value>
    public static MyGestureListener Instance
    {
        get
        {
            return instance;
        }
    }

    public bool IsPushFront()
    {
        if (pushFront)
        {
            pushFront = false;
            return true;
        }

        return false;
    }

    public bool IsRaisingRightHand()
    {
        if(raisingRightHand)
        {
            return true;
        }
        return false;
    }
    public bool IsRaisingLeftHand()
    {
        if (raisingLeftHand)
        {
            return true;
        }
        return false;
    }

    public float getPosX()
    {
        return posX;
    }

    public float getPosY()
    {
        return posY;
    }
    /// <summary>
    /// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    public void UserDetected(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only
        KinectManager manager = KinectManager.Instance;
        if (!manager || (userId != manager.GetPrimaryUserID()))
            return;

        // detect these user specific gestures
        //manager.DetectGesture(userId, KinectGestures.Gestures.PushFront);
        manager.DetectGesture(userId, KinectGestures.Gestures.RaisingRightHand);
        manager.DetectGesture(userId, KinectGestures.Gestures.RaisingLeftHand);
        manager.DetectGesture(userId, KinectGestures.Gestures.PushFront);

        if (gestureInfo != null)
        {
            gestureInfo.GetComponent<GUIText>().text = "Zoom-in, zoom-out or wheel to rotate the model. Raise hand to reset it.";
        }
    }

    /// <summary>
    /// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    public void UserLost(long userId, int userIndex)
    {
        // the gestures are allowed for the primary user only
        KinectManager manager = KinectManager.Instance;
        if (!manager || (userId != manager.GetPrimaryUserID()))
            return;

        if (gestureInfo != null)
        {
            gestureInfo.GetComponent<GUIText>().text = string.Empty;
        }
    }

    /// <summary>
    /// Invoked when a gesture is in progress.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="progress">Gesture progress [0..1]</param>
    /// <param name="joint">Joint type</param>
    /// <param name="screenPos">Normalized viewport position</param>
    public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  float progress, KinectInterop.JointType joint, Vector3 screenPos)
    {
        // the gestures are allowed for the primary user only
        KinectManager manager = KinectManager.Instance;
        if (!manager || (userId != manager.GetPrimaryUserID()))
            return;
        if (gesture == KinectGestures.Gestures.PushFront)
        {
            posX = screenPos.x;
            posY = screenPos.y;
        }

        if (gesture == KinectGestures.Gestures.RaisingRightHand)
            raisingRightHand = true;

        if (gesture == KinectGestures.Gestures.RaisingLeftHand)
            raisingLeftHand = true;
    }

    /// <summary>
    /// Invoked if a gesture is completed.
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="joint">Joint type</param>
    /// <param name="screenPos">Normalized viewport position</param>
    public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint, Vector3 screenPos)
    {
        if (gesture == KinectGestures.Gestures.RaisingRightHand)
            raisingRightHand = false;
        if (gesture == KinectGestures.Gestures.RaisingLeftHand)
            raisingLeftHand = false;

        if (gesture == KinectGestures.Gestures.PushFront)
        {
            posX = 0.0f;
            posY = 0.0f;
        }
        return true;
    }

    /// <summary>
    /// Invoked if a gesture is cancelled.
    /// </summary>
    /// <returns>true</returns>
    /// <c>false</c>
    /// <param name="userId">User ID</param>
    /// <param name="userIndex">User index</param>
    /// <param name="gesture">Gesture type</param>
    /// <param name="joint">Joint type</param>
    public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture,
                                  KinectInterop.JointType joint)
    {
        // the gestures are allowed for the primary user only
        KinectManager manager = KinectManager.Instance;
        if (!manager || (userId != manager.GetPrimaryUserID()))
            return false;
        if (gesture == KinectGestures.Gestures.RaisingRightHand)
            raisingRightHand = false;
        if (gesture == KinectGestures.Gestures.RaisingLeftHand)
            raisingLeftHand = false;

        if (gestureInfo != null && progressDisplayed)
        {
            progressDisplayed = false;
            gestureInfo.GetComponent<GUIText>().text = "Zoom-in, zoom-out or wheel to rotate the model. Raise hand to reset it."; ;
        }

        return true;
    }


    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
        {
            progressDisplayed = false;
            gestureInfo.GetComponent<GUIText>().text = string.Empty;

            Debug.Log("Forced progress to end.");
        }
    }

}
