/*
http://www.cgsoso.com/forum-211-1.html

CG搜搜 Unity3d 每日Unity3d插件免费更新 更有VIP资源！

CGSOSO 主打游戏开发，影视设计等CG资源素材。

插件如若商用，请务必官网购买！

daily assets update for try.

U should buy the asset from home store if u use it in your project!
*/

using UnityEngine;
//using Windows.Kinect;

using System.Collections;
using System.Collections.Generic;

/// <summary>
/// KinectGestures is utility class that processes programmatic Kinect gestures
/// </summary>
public class KinectGestures : MonoBehaviour
{

	/// <summary>
	/// This interface needs to be implemented by all Kinect gesture listeners
	/// </summary>
    /// Kinect gesture的监听者需要实现的接口，通过这个来获取进行的手势的信息。
	public interface GestureListenerInterface
	{
		/// <summary>
		/// Invoked when a new user is detected. Here you can start gesture tracking by invoking KinectManager.DetectGesture()-function.
		/// </summary>
		/// <param name="userId">User ID</param>
		/// <param name="userIndex">User index</param>
		void UserDetected(long userId, int userIndex);
		
		/// <summary>
		/// Invoked when a user gets lost. All tracked gestures for this user are cleared automatically.
		/// </summary>
		/// <param name="userId">User ID</param>
		/// <param name="userIndex">User index</param>
		void UserLost(long userId, int userIndex);
		
		/// <summary>
		/// Invoked when a gesture is in progress.
		/// </summary>
		/// <param name="userId">User ID</param>
		/// <param name="userIndex">User index</param>
		/// <param name="gesture">Gesture type</param>
		/// <param name="progress">Gesture progress [0..1]</param>
		/// <param name="joint">Joint type</param>
		/// <param name="screenPos">Normalized viewport position</param>
		void GestureInProgress(long userId, int userIndex, Gestures gesture, float progress, 
		                       KinectInterop.JointType joint, Vector3 screenPos);

		/// <summary>
		/// Invoked if a gesture is completed.
		/// </summary>
		/// <returns><c>true</c>, if the gesture detection must be restarted, <c>false</c> otherwise.</returns>
		/// <param name="userId">User ID</param>
		/// <param name="userIndex">User index</param>
		/// <param name="gesture">Gesture type</param>
		/// <param name="joint">Joint type</param>
		/// <param name="screenPos">Normalized viewport position</param>
		bool GestureCompleted(long userId, int userIndex, Gestures gesture,
		                      KinectInterop.JointType joint, Vector3 screenPos);

		/// <summary>
		/// Invoked if a gesture is cancelled.
		/// </summary>
		/// <returns><c>true</c>, if the gesture detection must be retarted, <c>false</c> otherwise.</returns>
		/// <param name="userId">User ID</param>
		/// <param name="userIndex">User index</param>
		/// <param name="gesture">Gesture type</param>
		/// <param name="joint">Joint type</param>
		bool GestureCancelled(long userId, int userIndex, Gestures gesture, 
		                      KinectInterop.JointType joint);
	}
	


//枚举类型Gestures，用来指代使用的手势。
	/// <summary>
	/// The gesture types.
	/// </summary>
	public enum Gestures
	{
		None = 0,
		RightElbowDown,//右臂的小臂垂直大臂向下是Inprogress,当手的位置在y轴高于指定高度，则动作完成
        RightElbowUp,//右臂的小臂垂直大臂向上是Inprogress,当手的位置在y轴低于指定高度，则动作完成
        HandsFrontChest,//双手并拢放于胸前是Inprogress, 任何一只手离肩部中心较远，则完成
        RightHandPushFront,//右手向前伸是InProgress,放下时是完成
        PushFront,
        RaisingRightHand,//右手上举是InProgress，放下时完成
        RaisingLeftHand,//左手上举是InProgress，放下时完成
        RaiseRightHand,
		RaiseLeftHand,
		Psi,
		Tpose,
		Stop,
		Wave,
//		Click,
		SwipeLeft,
		SwipeRight,
		SwipeUp,
		SwipeDown,
//		RightHandCursor,
//		LeftHandCursor,
		ZoomIn,
		ZoomOut,
		Wheel,
		Jump,
		Squat,
		Push,
		Pull,
		ShoulderLeftFront,
		ShoulderRightFront,
		LeanLeft,
		LeanRight,
		KickLeft,
		KickRight,
		Run,
		

		UserGesture1 = 101,
		UserGesture2 = 102,
		UserGesture3 = 103,
		UserGesture4 = 104,
		UserGesture5 = 105,
		UserGesture6 = 106,
		UserGesture7 = 107,
		UserGesture8 = 108,
		UserGesture9 = 109,
		UserGesture10 = 110,
	}
	
	
	/// <summary>
	/// Gesture data structure.
	/// </summary>
	public struct GestureData
	{
		public long userId;
		public Gestures gesture;
		public int state;
		public float timestamp;
		public int joint;
		public Vector3 jointPos;
		public Vector3 screenPos;
		public float tagFloat;
		public Vector3 tagVector;
		public Vector3 tagVector2;
		public float progress;
		public bool complete;
		public bool cancelled;
		public List<Gestures> checkForGestures;
		public float startTrackingAtTime;
	}
	

	// Gesture related constants, variables and functions
	protected int leftHandIndex;
	protected int rightHandIndex;
		
	protected int leftElbowIndex;
	protected int rightElbowIndex;
		
	protected int leftShoulderIndex;
	protected int rightShoulderIndex;
	
	protected int hipCenterIndex;
	protected int shoulderCenterIndex;

	protected int leftHipIndex;
	protected int rightHipIndex;

	protected int leftKneeIndex;
	protected int rightKneeIndex;
	
	protected int leftAnkleIndex;
	protected int rightAnkleIndex;


	/// <summary>
	/// Gets the list of gesture joint indexes.
	/// </summary>
	/// <returns>The needed joint indexes.</returns>
	/// <param name="manager">The KinectManager instance</param>
	public virtual int[] GetNeededJointIndexes(KinectManager manager)
	{
		leftHandIndex = manager.GetJointIndex(KinectInterop.JointType.HandLeft);
		rightHandIndex = manager.GetJointIndex(KinectInterop.JointType.HandRight);
		
		leftElbowIndex = manager.GetJointIndex(KinectInterop.JointType.ElbowLeft);
		rightElbowIndex = manager.GetJointIndex(KinectInterop.JointType.ElbowRight);
		
		leftShoulderIndex = manager.GetJointIndex(KinectInterop.JointType.ShoulderLeft);
		rightShoulderIndex = manager.GetJointIndex(KinectInterop.JointType.ShoulderRight);
		
		hipCenterIndex = manager.GetJointIndex(KinectInterop.JointType.SpineBase);
		shoulderCenterIndex = manager.GetJointIndex(KinectInterop.JointType.SpineShoulder);

		leftHipIndex = manager.GetJointIndex(KinectInterop.JointType.HipLeft);
		rightHipIndex = manager.GetJointIndex(KinectInterop.JointType.HipRight);

		leftKneeIndex = manager.GetJointIndex(KinectInterop.JointType.KneeLeft);
		rightKneeIndex = manager.GetJointIndex(KinectInterop.JointType.KneeRight);
		
		leftAnkleIndex = manager.GetJointIndex(KinectInterop.JointType.AnkleLeft);
		rightAnkleIndex = manager.GetJointIndex(KinectInterop.JointType.AnkleRight);
		
		int[] neededJointIndexes = {
			leftHandIndex, rightHandIndex, leftElbowIndex, rightElbowIndex, leftShoulderIndex, rightShoulderIndex,
			hipCenterIndex, shoulderCenterIndex, leftHipIndex, rightHipIndex, leftKneeIndex, rightKneeIndex, 
			leftAnkleIndex, rightAnkleIndex
		};

		return neededJointIndexes;
	}
	
//设置gestureData的具体信息，joint,jointPos,timestamp
	protected void SetGestureJoint(ref GestureData gestureData, float timestamp, int joint, Vector3 jointPos)
	{
		gestureData.joint = joint;
		gestureData.jointPos = jointPos;
		gestureData.timestamp = timestamp;
		gestureData.state++;
	}
	
	protected void SetGestureCancelled(ref GestureData gestureData)
	{
		gestureData.state = 0;
		gestureData.progress = 0f;
		gestureData.cancelled = true;
	}

//state=1的情况下被调用，检查动作是否完成。
//如果isInPose，代表动作完成了；不然，动作没有完成，调用SetGestureCancelled
/*有对于延时动作的处理。提供durationToComplete，如果时间没有达到要求，则不会直接完成*/
	protected void CheckPoseComplete(ref GestureData gestureData, float timestamp, Vector3 jointPos, bool isInPose, float durationToComplete)
	{
		if(isInPose)//动作完成了
		{
			float timeLeft = timestamp - gestureData.timestamp;
			gestureData.progress = durationToComplete > 0f ? Mathf.Clamp01(timeLeft / durationToComplete) : 1.0f;
	
			if(timeLeft >= durationToComplete)
			{
				gestureData.timestamp = timestamp;
				gestureData.jointPos = jointPos;
				gestureData.state++;
				gestureData.complete = true;
			}
		}
		else//没有完成
		{
			SetGestureCancelled(ref gestureData);
		}
	}
	
	protected void SetScreenPos(long userId, ref GestureData gestureData, ref Vector3[] jointsPos, ref bool[] jointsTracked)
	{
		Vector3 handPos = jointsPos[rightHandIndex];
		bool calculateCoords = false;
		
		if(gestureData.joint == rightHandIndex)
		{
			if(jointsTracked[rightHandIndex] /**&& jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex]*/)
			{
				calculateCoords = true;
			}
		}
		else if(gestureData.joint == leftHandIndex)
		{
			if(jointsTracked[leftHandIndex] /**&& jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex]*/)
			{
				handPos = jointsPos[leftHandIndex];
				calculateCoords = true;
			}
		}
		
		if(calculateCoords)
		{
			if(jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && 
				jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex])
			{
				Vector3 shoulderToHips = jointsPos[shoulderCenterIndex] - jointsPos[hipCenterIndex];
				Vector3 rightToLeft = jointsPos[rightShoulderIndex] - jointsPos[leftShoulderIndex];
				
				gestureData.tagVector2.x = rightToLeft.x; // * 1.2f;
				gestureData.tagVector2.y = shoulderToHips.y; // * 1.2f;
				
				if(gestureData.joint == rightHandIndex)
				{
					gestureData.tagVector.x = jointsPos[rightShoulderIndex].x - gestureData.tagVector2.x / 2;
					gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
				}
				else
				{
					gestureData.tagVector.x = jointsPos[leftShoulderIndex].x - gestureData.tagVector2.x / 2;
					gestureData.tagVector.y = jointsPos[hipCenterIndex].y;
				}
			}
	
			if(gestureData.tagVector2.x != 0 && gestureData.tagVector2.y != 0)
			{
				Vector3 relHandPos = handPos - gestureData.tagVector;
				gestureData.screenPos.x = Mathf.Clamp01(relHandPos.x / gestureData.tagVector2.x);
				gestureData.screenPos.y = Mathf.Clamp01(relHandPos.y / gestureData.tagVector2.y);
			}
			
		}
	}
	//计算zoom参数的公式
	protected void SetZoomFactor(long userId, ref GestureData gestureData, float initialZoom, ref Vector3[] jointsPos, ref bool[] jointsTracked)
	{
		Vector3 vectorZooming = jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
		
		if(gestureData.tagFloat == 0f || gestureData.userId != userId)
		{
			gestureData.tagFloat = 0.5f; // this is 100%
		}

		float distZooming = vectorZooming.magnitude;
		gestureData.screenPos.z = initialZoom + (distZooming / gestureData.tagFloat);
	}
	
	protected void SetWheelRotation(long userId, ref GestureData gestureData, Vector3 initialPos, Vector3 currentPos)
	{
		float angle = Vector3.Angle(initialPos, currentPos) * Mathf.Sign(currentPos.y - initialPos.y);
		gestureData.screenPos.z = angle;
	}

    protected void SetHandDiff(long userId, ref GestureData gestureData, ref Vector3[] jointsPos, ref bool[] jointsTracked)
    {
        Vector3 handDiff = jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
        gestureData.screenPos.x = handDiff.x;
        gestureData.screenPos.y = handDiff.y;
    }

    // estimate the next state and completeness of the gesture
    // 检查下一阶段和手势的完成情况
    // 使用gestureData.gesture来判断手势，使用gestureData.state来判断手势状态（检测/完成）
    // 核心函数，使用了jointsPos和jointsTracked的数据判断手势是否检测到，同时在检测到手势的时候判断手势是否结束
    // 值得一提的是左右是镜像的，也就是说用户的右手在实现上是程序里的左边
    // 一个问题是x、y、z的坐标轴到底是哪个
    /// <summary>
    /// estimate the state and progress of the given gesture.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="gestureData">Gesture-data structure</param>
    /// <param name="timestamp">Current time</param>
    /// <param name="jointsPos">Joints-position array</param>
    /// <param name="jointsTracked">Joints-tracked array</param>
    public virtual void CheckForGesture(long userId, ref GestureData gestureData, float timestamp, ref Vector3[] jointsPos, ref bool[] jointsTracked)
	{
		if(gestureData.complete)
			return;
		
		float bandSize = (jointsPos[shoulderCenterIndex].y - jointsPos[hipCenterIndex].y);
		float gestureTop = jointsPos[shoulderCenterIndex].y + bandSize * 1.2f / 3f;
		float gestureBottom = jointsPos[shoulderCenterIndex].y - bandSize * 1.8f / 3f;
		float gestureRight = jointsPos[rightHipIndex].x;
		float gestureLeft = jointsPos[leftHipIndex].x;
		
		//Debug.Log("root judging:"+(jointsPos[shoulderCenterIndex].z - jointsPos[leftHandIndex].z));
		switch(gestureData.gesture)
		{
			case Gestures.RightElbowDown:
				switch (gestureData.state)
                {
                    case 0:
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
                               Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.05f &&//右肘与右肩有非常相近的z值和y值
                               Mathf.Abs(jointsPos[rightElbowIndex].z - jointsPos[rightShoulderIndex].z) < 0.1f &&
                               jointsPos[rightElbowIndex].y - jointsPos[rightHandIndex].y > 0.15f//右手与右肘在y轴上的距离不能小于0.35f
                               )
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;
                    case 1:  // gesture complete
                             // isInPose作为是否完成或正在完成的参数传入（传入为true的时候动作会完成）
                             //当右大臂不保持当前动作或者 
                             //当右手在y轴上与右肘的距离小于0.25，则动作完成
                        bool isInPose = (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] )&&
                               (Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) > 0.25f ||
                               Mathf.Abs(jointsPos[rightElbowIndex].z - jointsPos[rightShoulderIndex].z) > 0.25f ||
                               jointsPos[rightElbowIndex].y - jointsPos[rightHandIndex].y < 0.25f);

                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex])
                            SetHandDiff(userId, ref gestureData, ref jointsPos, ref jointsTracked);

                        if (isInPose)
                        {
                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                        }
                        // check if right shoulder is still left of the right hip (leaning left)
                        break;
                }
                break;
           
             case Gestures.RightElbowUp:
                switch (gestureData.state)
                {
                    case 0:
                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
                               Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.05f &&//右肘与右肩有非常相近的z值和y值
                               Mathf.Abs(jointsPos[rightElbowIndex].z - jointsPos[rightShoulderIndex].z) < 0.1f &&
                               jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y > 0.25f//右手与右肘在y轴上的距离不能小于0.35f
                               )
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;
                    case 1:  // gesture complete
                             // isInPose作为是否完成或正在完成的参数传入（传入为true的时候动作会完成）
                             //当右大臂不保持当前动作或者 
                             //当右手在y轴上与右肘的距离小于0.25，则动作完成
                        bool isInPose = (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] )&&
                               (Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) > 0.25f ||
                               Mathf.Abs(jointsPos[rightElbowIndex].z - jointsPos[rightShoulderIndex].z) > 0.25f ||
                               jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y < 0.25f);

                        if (jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex])
                            SetHandDiff(userId, ref gestureData, ref jointsPos, ref jointsTracked);

                        if (isInPose)
                        {
                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                        }
                        // check if right shoulder is still left of the right hip (leaning left)
                        break;
                }
                break;
            case Gestures.HandsFrontChest:
                switch(gestureData.state)
                {
                    case 0:
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] &&  jointsTracked[shoulderCenterIndex] &&
                               Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[shoulderCenterIndex].y) < 0.15f&&//右手与左手竖直方向和水平方向都与肩部中心很近
                               Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[shoulderCenterIndex].x) < 0.15f &&
                               Mathf.Abs(jointsPos[rightHandIndex].z - jointsPos[shoulderCenterIndex].z) < 0.15f &&
                               Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[shoulderCenterIndex].y) < 0.15f &&//左手与左手竖直方向和水平方向都与肩部中心很近
                               Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[shoulderCenterIndex].x) < 0.15f &&
                               Mathf.Abs(jointsPos[leftHandIndex].z - jointsPos[shoulderCenterIndex].z) < 0.15f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;
                    case 1:  // gesture complete
                             // isInPose作为是否完成或正在完成的参数传入（传入为true的时候动作会完成）
                             // 任何一只手在任何方向上偏离肩部中心超过0.3则认为动作完成
                        bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[shoulderCenterIndex] &&
                               (Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[shoulderCenterIndex].y) > 0.3f ||
                               Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[shoulderCenterIndex].x) > 0.3f ||
                               Mathf.Abs(jointsPos[rightHandIndex].z - jointsPos[shoulderCenterIndex].z) > 0.3f ||
                               Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[shoulderCenterIndex].y) > 0.3f ||
                               Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[shoulderCenterIndex].x) > 0.3f ||
                               Mathf.Abs(jointsPos[leftHandIndex].z - jointsPos[shoulderCenterIndex].z) > 0.3f);

                        if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[shoulderCenterIndex])
                            SetHandDiff(userId, ref gestureData, ref jointsPos, ref jointsTracked);

                        if (isInPose)
                        {
                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                        }
                        // check if right shoulder is still left of the right hip (leaning left)
                        break;
                }
                break;
            case Gestures.RightHandPushFront:
                switch (gestureData.state)
                {
                    case 0:  // gesture detection - phase 1
                             //右手纵轴上大于臀部小于肩膀，则认为进入初始状态
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] &&
                               jointsPos[rightHandIndex].y >= jointsPos[hipCenterIndex].y &&
                               jointsPos[rightHandIndex].y <= jointsPos[shoulderCenterIndex].y && 
                               jointsPos[shoulderCenterIndex].z - jointsPos[rightHandIndex].z > 0.55f&&
                                jointsPos[leftHandIndex].y < jointsPos[hipCenterIndex].y)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;

                    case 1:  // gesture complete
                             // isInPose作为是否完成或正在完成的参数传入（传入为true的时候动作会完成）
                             // 如果手部与身体的长度小于一定范畴，调用
                        bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] &&
                               (jointsPos[leftHandIndex].y >= jointsPos[hipCenterIndex].y ||
                                jointsPos[shoulderCenterIndex].z - jointsPos[rightHandIndex].z < 0.35f);

                        if (jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex])
                            SetHandDiff(userId, ref gestureData, ref jointsPos, ref jointsTracked);

                        if (isInPose)
                        {
                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                        }
                        // check if right shoulder is still left of the right hip (leaning left)
                        break;
                }
                break;
            case Gestures.PushFront:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
					//左手纵轴上大于臀部小于肩膀，则认为进入初始状态
						if(jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] &&
				   			jointsPos[leftHandIndex].y >= jointsPos[hipCenterIndex].y &&
				   			jointsPos[leftHandIndex].y <=  jointsPos[shoulderCenterIndex].y &&jointsPos[shoulderCenterIndex].z - jointsPos[leftHandIndex].z > 0.45f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture complete
					// isInPose作为是否完成或正在完成的参数传入（传入为false的时候动作会取消）
					// 如果手部与身体的长度小于一定范畴，调用
						bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] &&
				   			jointsPos[leftHandIndex].y >= jointsPos[hipCenterIndex].y &&
				   			jointsPos[leftHandIndex].y <=  jointsPos[shoulderCenterIndex].y && jointsPos[shoulderCenterIndex].z - jointsPos[leftHandIndex].z < 0.35f;

                        if(jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex])
                            SetHandDiff(userId, ref gestureData, ref jointsPos, ref jointsTracked);

                        if (isInPose)
                        {
                            Vector3 jointPos = jointsPos[gestureData.joint];
                            CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
                        }
                        // check if right shoulder is still left of the right hip (leaning left)
                        break;
				}
				break;
            case Gestures.RaisingRightHand://一直举着右手
                switch (gestureData.state)
                {
                    case 0:  // gesture detection - phase 1
                        //右手上举，进入状态2
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.55f &&
                               (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }

                        break;

                    case 1:  // gesture - phase 2
                        bool isInRange = true;//如果右手的范围过于超过，则取消动作
                        if (isInRange)
                        {
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) < -0.1f &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // cancel the gesture
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;
            case Gestures.RaisingLeftHand://一直举着左手
                switch (gestureData.state)
                {
                    case 0:  // gesture detection - phase 1
                        //右手上举，进入状态2
                        if (jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.55f &&
                               (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f)
                        {
                            SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
                            gestureData.progress = 0.5f;
                        }

                        break;

                    case 1:  // gesture - phase 2
                        bool isInRange = true;//如果左手的范围过于超过，则取消动作
                        if (isInRange)
                        {
                            bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
                            (jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < -0.1f &&
                            (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f;

                            if (isInPose)
                            {
                                Vector3 jointPos = jointsPos[gestureData.joint];
                                SetZoomFactor(userId, ref gestureData, 1.0f, ref jointsPos, ref jointsTracked);
                                CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
                            }
                        }
                        else
                        {
                            // cancel the gesture
                            SetGestureCancelled(ref gestureData);
                        }
                        break;
                }
                break;
            // check for RaiseRightHand
            case Gestures.RaiseRightHand:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
					//检测RaiseRightHand是否启动。首先检查关节：右手、左手、左肩
					//检查手势位置，右手.y>0.3f+左肩.y且左手.y<左肩.y
                    //启动，左手低于左肩，右手高于左肩
						if(jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f &&
				   			(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
                            gestureData.progress = 0.5f;
                        }
                        break;
							
					case 1:  // gesture complete
					// isInPose作为是否完成或正在完成的参数传入（传入为false的时候动作会取消）
					//检查 右手.y>0.1f+左肩.y且左手.y<左肩.y
                    //完成：左手低于左肩，右手低于左肩
						bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[leftShoulderIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.1f &&
							(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0f;

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// check for RaiseLeftHand
			case Gestures.RaiseLeftHand:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
					// 左手.y>0.1f+右肩且右手.y<右肩.y
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
				   			(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
						}
						break;
							
					case 1:  // gesture complete
					//  左手.y>0.1f+右肩.y且右手.y<右肩.y
						bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[rightShoulderIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.1f &&
							(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0f;

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// check for Psi
			case Gestures.Psi:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
					//右手.y>0.1f+肩中点.y且左手.y>0.1f+肩中点.y
						if(jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[shoulderCenterIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[shoulderCenterIndex].y) > 0.1f &&
					       (jointsPos[leftHandIndex].y - jointsPos[shoulderCenterIndex].y) > 0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						break;
							
					case 1:  // gesture complete
						bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[leftHandIndex] && jointsTracked[shoulderCenterIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[shoulderCenterIndex].y) > 0.1f &&
							(jointsPos[leftHandIndex].y - jointsPos[shoulderCenterIndex].y) > 0.1f;

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// check for Tpose
			case Gestures.Tpose:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
					       Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.07f
					       Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
					   	   jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
					  	   Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
					       Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						break;
						
					case 1:  // gesture complete
						bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[rightShoulderIndex] &&
								Mathf.Abs(jointsPos[rightElbowIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
							    Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightShoulderIndex].y) < 0.1f &&  // 0.7f
							    jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[leftShoulderIndex] &&
								Mathf.Abs(jointsPos[leftElbowIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f &&
							    Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftShoulderIndex].y) < 0.1f;
						
						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
						break;
				}
				break;
				
			// check for Stop
			case Gestures.Stop:
				switch(gestureData.state)
				{
					case 0:  // gesture detection
						if(jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.2f &&
				   		   (jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
					       (jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.2f &&
				           (jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
						}
						break;
							
					case 1:  // gesture complete
						bool isInPose = (gestureData.joint == rightHandIndex) ?
							(jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
							(jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) < 0.2f &&
				 			(jointsPos[rightHandIndex].x - jointsPos[rightHipIndex].x) >= 0.4f) :
							(jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
							(jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) < 0.2f &&
						 	(jointsPos[leftHandIndex].x - jointsPos[leftHipIndex].x) <= -0.4f);

						Vector3 jointPos = jointsPos[gestureData.joint];
						CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.PoseCompleteDuration);
						break;
				}
				break;

			// check for Wave
            /*
             多段动作：
                第一段：满足目标，SetGestureJoint，并且设gestureData.progress = 0.3f
                第二段：要在规定时间内完成指定动作(timestamp - gestureData.timestamp<limit)，如果完成，则更新timestamp,state和progress；没有完成则调用SetGestureCancelled
             */
			case Gestures.Wave:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f &&
					       (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.3f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
					            (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.3f;
						}
						break;
				
					case 1:  // gesture - phase 2
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f && 
								(jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) < -0.05f :
								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
								(jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) > 0.05f;
				
							if(isInPose)
							{
								gestureData.timestamp = timestamp;
								gestureData.state++;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
									
					case 2:  // gesture phase 3 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > 0.1f && 
								(jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0.05f :
								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > 0.1f &&
								(jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < -0.05f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

//			// check for Click
//			case Gestures.Click:
//				switch(gestureData.state)
//				{
//					case 0:  // gesture detection - phase 1
//						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
//					       (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f)
//						{
//							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
//							gestureData.progress = 0.3f;
//
//							// set screen position at the start, because this is the most accurate click position
//							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
//						}
//						else if(jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
//					            (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f)
//						{
//							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
//							gestureData.progress = 0.3f;
//
//							// set screen position at the start, because this is the most accurate click position
//							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
//						}
//						break;
//				
//					case 1:  // gesture - phase 2
////						if((timestamp - gestureData.timestamp) < 1.0f)
////						{
////							bool isInPose = gestureData.joint == rightHandIndex ?
////								jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
////								//(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f && 
////								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.08f &&
////								(jointsPos[rightHandIndex].z - gestureData.jointPos.z) < -0.05f :
////								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
////								//(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
////								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.08f &&
////								(jointsPos[leftHandIndex].z - gestureData.jointPos.z) < -0.05f;
////				
////							if(isInPose)
////							{
////								gestureData.timestamp = timestamp;
////								gestureData.jointPos = jointsPos[gestureData.joint];
////								gestureData.state++;
////								gestureData.progress = 0.7f;
////							}
////							else
////							{
////								// check for stay-in-place
////								Vector3 distVector = jointsPos[gestureData.joint] - gestureData.jointPos;
////								isInPose = distVector.magnitude < 0.05f;
////
////								Vector3 jointPos = jointsPos[gestureData.joint];
////								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, Constants.ClickStayDuration);
////							}
////						}
////						else
//						{
//							// check for stay-in-place
//							Vector3 distVector = jointsPos[gestureData.joint] - gestureData.jointPos;
//							bool isInPose = distVector.magnitude < 0.05f;
//
//							Vector3 jointPos = jointsPos[gestureData.joint];
//							CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, KinectInterop.Constants.ClickStayDuration);
////							SetGestureCancelled(gestureData);
//						}
//						break;
//									
////					case 2:  // gesture phase 3 = complete
////						if((timestamp - gestureData.timestamp) < 1.0f)
////						{
////							bool isInPose = gestureData.joint == rightHandIndex ?
////								jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
////								//(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f && 
////								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.08f &&
////								(jointsPos[rightHandIndex].z - gestureData.jointPos.z) > 0.05f :
////								jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
////								//(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
////								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.08f &&
////								(jointsPos[leftHandIndex].z - gestureData.jointPos.z) > 0.05f;
////
////							if(isInPose)
////							{
////								Vector3 jointPos = jointsPos[gestureData.joint];
////								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
////							}
////						}
////						else
////						{
////							// cancel the gesture
////							SetGestureCancelled(ref gestureData);
////						}
////						break;
//				}
//				break;

			// check for SwipeLeft
			case Gestures.SwipeLeft:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
//						if(jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
//					       (jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) > -0.05f &&
//					       (jointsPos[rightHandIndex].x - jointsPos[rightElbowIndex].x) > 0f)
//						{
//							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
//							gestureData.progress = 0.5f;
//						}
						if(jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
						   jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
				   			jointsPos[rightHandIndex].x <= gestureRight && jointsPos[rightHandIndex].x > gestureLeft)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.1f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
//							bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[rightElbowIndex] &&
//								Mathf.Abs(jointsPos[rightHandIndex].y - jointsPos[rightElbowIndex].y) < 0.1f && 
//								Mathf.Abs(jointsPos[rightHandIndex].y - gestureData.jointPos.y) < 0.08f && 
//								(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < -0.15f;
//
//							if(isInPose)
//							{
//								Vector3 jointPos = jointsPos[gestureData.joint];
//								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
//							}

							bool isInPose = jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
									jointsPos[rightHandIndex].x <= gestureLeft;
							
							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
							else if(jointsPos[rightHandIndex].x <= gestureRight)
							{
								float gestureSize = gestureRight - gestureLeft;
								gestureData.progress = gestureSize > 0.01f ? (gestureRight - jointsPos[rightHandIndex].x) / gestureSize : 0f;
							}

						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for SwipeRight
			case Gestures.SwipeRight:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
//						if(jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
//				            (jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) > -0.05f &&
//				            (jointsPos[leftHandIndex].x - jointsPos[leftElbowIndex].x) < 0f)
//						{
//							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
//							gestureData.progress = 0.5f;
//						}

						if(jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
						   jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
				   			jointsPos[leftHandIndex].x >= gestureLeft && jointsPos[leftHandIndex].x < gestureRight)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.1f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
//							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[leftElbowIndex] &&
//								Mathf.Abs(jointsPos[leftHandIndex].y - jointsPos[leftElbowIndex].y) < 0.1f &&
//								Mathf.Abs(jointsPos[leftHandIndex].y - gestureData.jointPos.y) < 0.08f && 
//								(jointsPos[leftHandIndex].x - gestureData.jointPos.x) > 0.15f;
//
//							if(isInPose)
//							{
//								Vector3 jointPos = jointsPos[gestureData.joint];
//								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
//							}

							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
									jointsPos[leftHandIndex].x >= gestureRight;
							
							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
							else if(jointsPos[leftHandIndex].x >= gestureLeft)
							{
								float gestureSize = gestureRight - gestureLeft;
								gestureData.progress = gestureSize > 0.01f ? (jointsPos[leftHandIndex].x - gestureLeft) / gestureSize : 0f;
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for SwipeUp
			case Gestures.SwipeUp:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < -0.0f &&
					       (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.15f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < -0.0f &&
					            (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.15f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) > 0.05f && 
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.1f :
								jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) > 0.05f && 
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.1f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for SwipeDown
			case Gestures.SwipeDown:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftShoulderIndex] &&
					       (jointsPos[rightHandIndex].y - jointsPos[leftShoulderIndex].y) >= 0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightShoulderIndex] &&
					            (jointsPos[leftHandIndex].y - jointsPos[rightShoulderIndex].y) >= 0.05f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) < -0.15f && 
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) <= 0.1f :
								jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) < -0.15f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) <= 0.1f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

//			// check for RightHandCursor
//			case Gestures.RightHandCursor:
//				switch(gestureData.state)
//				{
//					case 0:  // gesture detection - phase 1 (perpetual)
//						if(jointsTracked[rightHandIndex] && jointsTracked[rightHipIndex] &&
//							//(jointsPos[rightHandIndex].y - jointsPos[rightHipIndex].y) > -0.1f)
//				   			(jointsPos[rightHandIndex].y - jointsPos[hipCenterIndex].y) >= 0f)
//						{
//							gestureData.joint = rightHandIndex;
//							gestureData.timestamp = timestamp;
//							gestureData.jointPos = jointsPos[rightHandIndex];
//
//							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
//							gestureData.progress = 0.7f;
//						}
//						else
//						{
//							// cancel the gesture
//							//SetGestureCancelled(ref gestureData);
//							gestureData.progress = 0f;
//						}
//						break;
//				
//				}
//				break;
//
//			// check for LeftHandCursor
//			case Gestures.LeftHandCursor:
//				switch(gestureData.state)
//				{
//					case 0:  // gesture detection - phase 1 (perpetual)
//						if(jointsTracked[leftHandIndex] && jointsTracked[leftHipIndex] &&
//							//(jointsPos[leftHandIndex].y - jointsPos[leftHipIndex].y) > -0.1f)
//							(jointsPos[leftHandIndex].y - jointsPos[hipCenterIndex].y) >= 0f)
//						{
//							gestureData.joint = leftHandIndex;
//							gestureData.timestamp = timestamp;
//							gestureData.jointPos = jointsPos[leftHandIndex];
//
//							SetScreenPos(userId, ref gestureData, ref jointsPos, ref jointsTracked);
//							gestureData.progress = 0.7f;
//						}
//						else
//						{
//							// cancel the gesture
//							//SetGestureCancelled(ref gestureData);
//							gestureData.progress = 0f;
//						}
//						break;
//				
//				}
//				break;

			// check for ZoomIn
			case Gestures.ZoomIn:
				Vector3 vectorZoomOut = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
				float distZoomOut = vectorZoomOut.magnitude;
			
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
				   			jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
				   			jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
						   distZoomOut < 0.3f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.tagVector = Vector3.right;
							gestureData.tagFloat = 0f;
							gestureData.progress = 0.3f;
						}
						break;
				
					case 1:  // gesture phase 2 = zooming
						if((timestamp - gestureData.timestamp) < 1.0f)
						{
							float angleZoomOut = Vector3.Angle(gestureData.tagVector, vectorZoomOut) * Mathf.Sign(vectorZoomOut.y - gestureData.tagVector.y);
							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
									jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
								distZoomOut < 1.5f && Mathf.Abs(angleZoomOut) < 20f;

							if(isInPose)
							{
								SetZoomFactor(userId, ref gestureData, 1.0f, ref jointsPos, ref jointsTracked);
								gestureData.timestamp = timestamp;
								gestureData.progress = 0.7f;
							}
//							else
//							{
//								// cancel the gesture
//								SetGestureCancelled(ref gestureData);
//							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for ZoomOut
			case Gestures.ZoomOut:
				Vector3 vectorZoomIn = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
				float distZoomIn = vectorZoomIn.magnitude;
				
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
						   jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
						   jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
						   distZoomIn >= 0.7f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.tagVector = Vector3.right;
							gestureData.tagFloat = distZoomIn;
							gestureData.progress = 0.3f;
						}
						break;
				
					case 1:  // gesture phase 2 = zooming
						if((timestamp - gestureData.timestamp) < 1.0f)
						{
							float angleZoomIn = Vector3.Angle(gestureData.tagVector, vectorZoomIn) * Mathf.Sign(vectorZoomIn.y - gestureData.tagVector.y);
							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
									jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
									jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
								distZoomIn >= 0.2f && Mathf.Abs(angleZoomIn) < 20f;

							if(isInPose)
							{
								SetZoomFactor(userId, ref gestureData, 0.0f, ref jointsPos, ref jointsTracked);
								gestureData.timestamp = timestamp;
								gestureData.progress = 0.7f;
							}
//							else
//							{
//								// cancel the gesture
//								SetGestureCancelled(ref gestureData);
//							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for Wheel
			case Gestures.Wheel:
				Vector3 vectorWheel = (Vector3)jointsPos[rightHandIndex] - jointsPos[leftHandIndex];
				float distWheel = vectorWheel.magnitude;

//				Debug.Log(string.Format("{0}. Dist: {1:F1}, Tag: {2:F1}, Diff: {3:F1}", gestureData.state,
//				                        distWheel, gestureData.tagFloat, Mathf.Abs(distWheel - gestureData.tagFloat)));

				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
						   jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
						   jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
						   distWheel >= 0.3f && distWheel < 0.7f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.tagVector = Vector3.right;
							gestureData.tagFloat = distWheel;
							gestureData.progress = 0.3f;
						}
						break;
				
					case 1:  // gesture phase 2 = zooming
						if((timestamp - gestureData.timestamp) < 0.5f)
						{
							float angle = Vector3.Angle(gestureData.tagVector, vectorWheel) * Mathf.Sign(vectorWheel.y - gestureData.tagVector.y);
							bool isInPose = jointsTracked[leftHandIndex] && jointsTracked[rightHandIndex] && jointsTracked[hipCenterIndex] && jointsTracked[shoulderCenterIndex] && jointsTracked[leftHipIndex] && jointsTracked[rightHipIndex] &&
								jointsPos[leftHandIndex].y >= gestureBottom && jointsPos[leftHandIndex].y <= gestureTop &&
								jointsPos[rightHandIndex].y >= gestureBottom && jointsPos[rightHandIndex].y <= gestureTop &&
								distWheel >= 0.3f && distWheel < 0.7f && 
								Mathf.Abs(distWheel - gestureData.tagFloat) < 0.1f;

							if(isInPose)
							{
								//SetWheelRotation(userId, ref gestureData, gestureData.tagVector, vectorWheel);
								gestureData.screenPos.z = angle;  // wheel angle
								gestureData.timestamp = timestamp;
								gestureData.tagFloat = distWheel;
								gestureData.progress = 0.7f;
							}
//							else
//							{
//								// cancel the gesture
//								SetGestureCancelled(ref gestureData);
//							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;
			
			// check for Jump
			case Gestures.Jump:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[hipCenterIndex] && 
							(jointsPos[hipCenterIndex].y > 0.6f) && (jointsPos[hipCenterIndex].y < 1.2f))
						{
							SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = jointsTracked[hipCenterIndex] &&
								(jointsPos[hipCenterIndex].y - gestureData.jointPos.y) > 0.15f && 
								Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for Squat
			case Gestures.Squat:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[hipCenterIndex] && 
							(jointsPos[hipCenterIndex].y <= 0.7f))
						{
							SetGestureJoint(ref gestureData, timestamp, hipCenterIndex, jointsPos[hipCenterIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = jointsTracked[hipCenterIndex] &&
								(jointsPos[hipCenterIndex].y - gestureData.jointPos.y) < -0.15f && 
								Mathf.Abs(jointsPos[hipCenterIndex].x - gestureData.jointPos.x) < 0.2f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for Push
			case Gestures.Push:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
				   			(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
				   			Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
				   			(jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.2f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
								(jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.2f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[rightHandIndex].z - gestureData.jointPos.z) < -0.2f :
								jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[leftHandIndex].z - gestureData.jointPos.z) < -0.2f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for Pull
			case Gestures.Pull:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1
						if(jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
						   (jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
						   Mathf.Abs(jointsPos[rightHandIndex].x - jointsPos[rightShoulderIndex].x) < 0.2f &&
						   (jointsPos[rightHandIndex].z - jointsPos[leftElbowIndex].z) < -0.3f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightHandIndex, jointsPos[rightHandIndex]);
							gestureData.progress = 0.5f;
						}
						else if(jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
						        (jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
						        Mathf.Abs(jointsPos[leftHandIndex].x - jointsPos[leftShoulderIndex].x) < 0.2f &&
						        (jointsPos[leftHandIndex].z - jointsPos[rightElbowIndex].z) < -0.3f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftHandIndex, jointsPos[leftHandIndex]);
							gestureData.progress = 0.5f;
						}
						break;
				
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 1.5f)
						{
							bool isInPose = gestureData.joint == rightHandIndex ?
								jointsTracked[rightHandIndex] && jointsTracked[leftElbowIndex] && jointsTracked[rightShoulderIndex] &&
								(jointsPos[rightHandIndex].y - jointsPos[leftElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[rightHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[rightHandIndex].z - gestureData.jointPos.z) > 0.25f :
								jointsTracked[leftHandIndex] && jointsTracked[rightElbowIndex] && jointsTracked[leftShoulderIndex] &&
								(jointsPos[leftHandIndex].y - jointsPos[rightElbowIndex].y) > -0.1f &&
								Mathf.Abs(jointsPos[leftHandIndex].x - gestureData.jointPos.x) < 0.2f &&
								(jointsPos[leftHandIndex].z - gestureData.jointPos.z) > 0.25f;

							if(isInPose)
							{
								Vector3 jointPos = jointsPos[gestureData.joint];
								CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;

			// check for ShoulderLeftFron
			case Gestures.ShoulderLeftFront:
				switch(gestureData.state)
				{
				case 0:  // gesture detection - phase 1
					if(jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[leftHipIndex] &&
				   	   (jointsPos[rightShoulderIndex].z - jointsPos[leftHipIndex].z) < 0f &&
				       (jointsPos[rightShoulderIndex].z - jointsPos[leftShoulderIndex].z) > -0.15f)
					{
						SetGestureJoint(ref gestureData, timestamp, rightShoulderIndex, jointsPos[rightShoulderIndex]);
						gestureData.progress = 0.5f;
					}
					break;
					
				case 1:  // gesture phase 2 = complete
					if((timestamp - gestureData.timestamp) < 1.5f)
					{
						bool isInPose = jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[leftHipIndex] &&
								(jointsPos[rightShoulderIndex].z - jointsPos[leftShoulderIndex].z) < -0.2f;
						
						if(isInPose)
						{
							Vector3 jointPos = jointsPos[gestureData.joint];
							CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
						}
					}
					else
					{
						// cancel the gesture
						SetGestureCancelled(ref gestureData);
					}
					break;
				}
				break;
				
			// check for ShoulderRightFront
			case Gestures.ShoulderRightFront:
				switch(gestureData.state)
				{
				case 0:  // gesture detection - phase 1
					if(jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] &&
					   (jointsPos[leftShoulderIndex].z - jointsPos[rightHipIndex].z) < 0f &&
					   (jointsPos[leftShoulderIndex].z - jointsPos[rightShoulderIndex].z) > -0.15f)
					{
						SetGestureJoint(ref gestureData, timestamp, leftShoulderIndex, jointsPos[leftShoulderIndex]);
						gestureData.progress = 0.5f;
					}
					break;
					
				case 1:  // gesture phase 2 = complete
					if((timestamp - gestureData.timestamp) < 1.5f)
					{
						bool isInPose = jointsTracked[leftShoulderIndex] && jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] &&
								(jointsPos[leftShoulderIndex].z - jointsPos[rightShoulderIndex].z) < -0.2f;
						
						if(isInPose)
						{
							Vector3 jointPos = jointsPos[gestureData.joint];
							CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
						}
					}
					else
					{
						// cancel the gesture
						SetGestureCancelled(ref gestureData);
					}
					break;
				}
				break;

			// check for LeanLeft
			case Gestures.LeanLeft:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1  (right shoulder is left of the right hip, means leaning left)
						if(jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] &&
						   (jointsPos[rightShoulderIndex].x - jointsPos[rightHipIndex].x) < 0f)
						{
							SetGestureJoint(ref gestureData, timestamp, rightShoulderIndex, jointsPos[rightShoulderIndex]);
							gestureData.progress = 0.3f;
						}
						break;
					
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 0.5f)
						{
							// check if right shoulder is still left of the right hip (leaning left)
							bool isInPose = jointsTracked[rightShoulderIndex] && jointsTracked[rightHipIndex] &&
								(jointsPos[rightShoulderIndex].x - jointsPos[rightHipIndex].x) < 0f;
							
							if(isInPose)
							{
								// calculate lean angle
								Vector3 vSpineLL = jointsPos[shoulderCenterIndex] - jointsPos[hipCenterIndex];
								gestureData.screenPos.z = Vector3.Angle(Vector3.up, vSpineLL);
								
								gestureData.timestamp = timestamp;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;
				
				// check for LeanRight
			case Gestures.LeanRight:
				switch(gestureData.state)
				{
					case 0:  // gesture detection - phase 1 (left shoulder is right of the left hip, means leaning right)
						if(jointsTracked[leftShoulderIndex] && jointsTracked[leftHipIndex] &&
						   (jointsPos[leftShoulderIndex].x - jointsPos[leftHipIndex].x) > 0f)
						{
							SetGestureJoint(ref gestureData, timestamp, leftShoulderIndex, jointsPos[leftShoulderIndex]);
							gestureData.progress = 0.3f;
						}
						break;
					
					case 1:  // gesture phase 2 = complete
						if((timestamp - gestureData.timestamp) < 0.5f)
						{
							// check if left shoulder is still right of the left hip (leaning right)
							bool isInPose = jointsTracked[leftShoulderIndex] && jointsTracked[leftHipIndex] &&
								(jointsPos[leftShoulderIndex].x - jointsPos[leftHipIndex].x) > 0f;
							
							if(isInPose)
							{
								// calculate lean angle
								Vector3 vSpineLR = jointsPos[shoulderCenterIndex] - jointsPos[hipCenterIndex];
								gestureData.screenPos.z = Vector3.Angle(Vector3.up, vSpineLR);
								
								gestureData.timestamp = timestamp;
								gestureData.progress = 0.7f;
							}
						}
						else
						{
							// cancel the gesture
							SetGestureCancelled(ref gestureData);
						}
						break;
				}
				break;
				
			// check for KickLeft
			case Gestures.KickLeft:
				switch(gestureData.state)
				{
				case 0:  // gesture detection - phase 1
					if(jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[leftHipIndex] &&
					   (jointsPos[leftAnkleIndex].z - jointsPos[leftHipIndex].z) < 0f &&
					   (jointsPos[leftAnkleIndex].z - jointsPos[rightAnkleIndex].z) > -0.2f)
					{
						SetGestureJoint(ref gestureData, timestamp, leftAnkleIndex, jointsPos[leftAnkleIndex]);
						gestureData.progress = 0.5f;
					}
					break;
					
				case 1:  // gesture phase 2 = complete
					if((timestamp - gestureData.timestamp) < 1.5f)
					{
						bool isInPose = jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[leftHipIndex] &&
							(jointsPos[leftAnkleIndex].z - jointsPos[rightAnkleIndex].z) < -0.4f;
						
						if(isInPose)
						{
							Vector3 jointPos = jointsPos[gestureData.joint];
							CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
						}
					}
					else
					{
						// cancel the gesture
						SetGestureCancelled(ref gestureData);
					}
					break;
				}
				break;
			
			// check for KickRight
			case Gestures.KickRight:
				switch(gestureData.state)
				{
				case 0:  // gesture detection - phase 1
					if(jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[rightHipIndex] &&
					   (jointsPos[rightAnkleIndex].z - jointsPos[rightHipIndex].z) < 0f &&
					   (jointsPos[rightAnkleIndex].z - jointsPos[leftAnkleIndex].z) > -0.2f)
					{
						SetGestureJoint(ref gestureData, timestamp, rightAnkleIndex, jointsPos[rightAnkleIndex]);
						gestureData.progress = 0.5f;
					}
					break;
					
				case 1:  // gesture phase 2 = complete
					if((timestamp - gestureData.timestamp) < 1.5f)
					{
						bool isInPose = jointsTracked[leftAnkleIndex] && jointsTracked[rightAnkleIndex] && jointsTracked[rightHipIndex] &&
							(jointsPos[rightAnkleIndex].z - jointsPos[leftAnkleIndex].z) < -0.4f;
						
						if(isInPose)
						{
							Vector3 jointPos = jointsPos[gestureData.joint];
							CheckPoseComplete(ref gestureData, timestamp, jointPos, isInPose, 0f);
						}
					}
					else
					{
						// cancel the gesture
						SetGestureCancelled(ref gestureData);
					}
					break;
				}
				break;
		
			case Gestures.Run:
				switch(gestureData.state)
				{
				case 0:  // gesture detection - phase 1
					// check if the left knee is up
					if(jointsTracked[leftKneeIndex] && jointsTracked[rightKneeIndex] &&
					   (jointsPos[leftKneeIndex].y - jointsPos[rightKneeIndex].y) > 0.1f)
					{
						SetGestureJoint(ref gestureData, timestamp, leftKneeIndex, jointsPos[leftKneeIndex]);
						gestureData.progress = 0.3f;
					}
					break;
					
				case 1:  // gesture complete
					if((timestamp - gestureData.timestamp) < 1.0f)
					{
						// check if the right knee is up
						bool isInPose = jointsTracked[rightKneeIndex] && jointsTracked[leftKneeIndex] &&
							(jointsPos[rightKneeIndex].y - jointsPos[leftKneeIndex].y) > 0.1f;
						
						if(isInPose)
						{
							// go to state 2
							gestureData.timestamp = timestamp;
							gestureData.progress = 0.7f;
							gestureData.state = 2;
						}
					}
					else
					{
						// cancel the gesture
						SetGestureCancelled(ref gestureData);
					}
					break;
					
				case 2:  // gesture complete
					if((timestamp - gestureData.timestamp) < 1.0f)
					{
						// check if the left knee is up again
						bool isInPose = jointsTracked[leftKneeIndex] && jointsTracked[rightKneeIndex] &&
							(jointsPos[leftKneeIndex].y - jointsPos[rightKneeIndex].y) > 0.1f;
						
						if(isInPose)
						{
							// go back to state 1
							gestureData.timestamp = timestamp;
							gestureData.progress = 0.8f;
							gestureData.state = 1;
						}
					}
					else
					{
						// cancel the gesture
						SetGestureCancelled(ref gestureData);
					}
					break;
				}
				break;
				
			// here come more gesture-cases
		}
	}

}
