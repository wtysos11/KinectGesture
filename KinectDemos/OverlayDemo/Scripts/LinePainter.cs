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

public class LinePainter : MonoBehaviour 
{
	[Tooltip("Line renderer used for the line drawing.")]
	public LineRenderer linePrefab;

	[Tooltip("GUI-Text to display information messages.")]
	public GUIText infoText;


	private HandOverlayer handOverlayer = null;
	private List<GameObject> linesDrawn = new List<GameObject>();
	private LineRenderer currentLine;
	private int lineVertexIndex = 2;

	void Start()
	{
		handOverlayer = GetComponent<HandOverlayer>();
	}

	void Update()
	{
		if(Input.GetKeyDown(KeyCode.U))
		{
			// U-key means Undo
			DeleteLastLine();
		}

		// display info message when a user is detected
		KinectManager manager = KinectManager.Instance;
		if(manager && manager.IsInitialized() && manager.IsUserDetected())
		{
			if(infoText)
			{
				infoText.GetComponent<GUIText>().text = "Grip hand to start drawing. Press [U] to undo the last line.";
			}
		}

		
		if(currentLine == null &&
		   (handOverlayer && (handOverlayer.GetLastHandEvent() == InteractionManager.HandEventType.Grip)))
		{
			// start drawing lines
			currentLine = Instantiate(linePrefab).GetComponent<LineRenderer>();
			currentLine.name = "Line" + linesDrawn.Count;
			currentLine.transform.parent = transform;

			Vector3 cursorPos = handOverlayer.GetCursorPos();
			cursorPos.z = Camera.main.nearClipPlane;
			
			Vector3 cursorSpacePos = Camera.main.ViewportToWorldPoint(cursorPos);
			currentLine.SetPosition(0, cursorSpacePos);
			currentLine.SetPosition(1, cursorSpacePos);

			lineVertexIndex = 2;
			linesDrawn.Add(currentLine.gameObject);

			StartCoroutine(DrawLine());
		}
		
		if (currentLine != null &&
		    (handOverlayer != null && (handOverlayer.GetLastHandEvent() == InteractionManager.HandEventType.Release)))
		{
			// end drawing lines
			currentLine = null;
		}
	}

	// undo the last drawn line
	public void DeleteLastLine()
	{
		if (linesDrawn.Count > 0)
		{
			GameObject goLastLine = linesDrawn[linesDrawn.Count-1];

			linesDrawn.RemoveAt(linesDrawn.Count-1);
			Destroy(goLastLine);
		}
	}

	// continue drawing line
	IEnumerator DrawLine()
	{
		while(handOverlayer && (handOverlayer.GetLastHandEvent() == InteractionManager.HandEventType.Grip))
		{
			yield return new WaitForEndOfFrame();

			if (currentLine != null)
			{
				lineVertexIndex++;
				currentLine.SetVertexCount(lineVertexIndex);

				Vector3 cursorPos = handOverlayer.GetCursorPos();
				cursorPos.z = Camera.main.nearClipPlane;

				Vector3 cursorSpacePos = Camera.main.ViewportToWorldPoint(cursorPos);
				currentLine.SetPosition(lineVertexIndex - 1, cursorSpacePos);
			}
		}
	}

}
