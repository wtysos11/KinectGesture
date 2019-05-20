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

public class KinectPlayerController : MonoBehaviour 
{
	private SpeechManager speechManager;
	private KinectRecorderPlayer saverPlayer;


	void Start()
	{
		saverPlayer = KinectRecorderPlayer.Instance;
	}

	void Update () 
	{
		if(speechManager == null)
		{
			speechManager = SpeechManager.Instance;
		}
		
		if(speechManager != null && speechManager.IsSapiInitialized())
		{
			if(speechManager.IsPhraseRecognized())
			{
				string sPhraseTag = speechManager.GetPhraseTagRecognized();
				
				switch(sPhraseTag)
				{
					case "RECORD":
						if(saverPlayer)
						{
							saverPlayer.StartRecording();
						}
						break;
						
					case "PLAY":
						if(saverPlayer)
						{
							saverPlayer.StartPlaying();
						}
						break;

					case "STOP":
						if(saverPlayer)
						{
							saverPlayer.StopRecordingOrPlaying();
						}
						break;

				}
				
				speechManager.ClearPhraseRecognized();
			}
		}

		// alternatively, use the keyboard
		if(Input.GetButtonDown("Jump"))  // start or stop recording
		{
			if(saverPlayer)
			{
				if(!saverPlayer.IsRecording())
				{
					saverPlayer.StartRecording();
				}
				else
				{
					saverPlayer.StopRecordingOrPlaying();
				}
			}
		}

		if(Input.GetButtonDown("Fire1"))  // start or stop playing
		{
			if(saverPlayer)
			{
				if(!saverPlayer.IsPlaying())
				{
					saverPlayer.StartPlaying();
				}
				else
				{
					saverPlayer.StopRecordingOrPlaying();
				}
			}
		}

	}

}
