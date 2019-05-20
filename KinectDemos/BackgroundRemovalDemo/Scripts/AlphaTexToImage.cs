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

public class AlphaTexToImage : MonoBehaviour 
{
	[Tooltip("Foreground material applied to the detected users' silhouettes.")]
	public Material foregroundMaterial;

	private RenderTexture texOutput;


	void Start()
	{
		// create the output texture
		KinectManager kinectManager = KinectManager.Instance;
		if(kinectManager && kinectManager.IsInitialized())
		{
			texOutput = new RenderTexture(kinectManager.GetColorImageWidth(), kinectManager.GetColorImageHeight(), 0);
			texOutput.wrapMode = TextureWrapMode.Clamp;
		}
	}

	void OnDestroy()
	{
		// release the output texture
		if(texOutput)
		{
			texOutput.Release();
			texOutput = null;
		}
	}

	void Update () 
	{
		if(texOutput)
		{
			// blit the output texture
			BackgroundRemovalManager backManager = BackgroundRemovalManager.Instance;
			if(backManager && backManager.IsBackgroundRemovalInitialized())
			{
				if(foregroundMaterial)
				{
					foregroundMaterial.SetTexture("_BodyTex", backManager.GetAlphaBodyTex());
					Graphics.Blit(null, texOutput, foregroundMaterial, 0);
				}
				else
				{
					Graphics.Blit(backManager.GetAlphaBodyTex(), texOutput);
				}
			}

			// set the gui texture, if needed
			GUITexture guiTexture = GetComponent<GUITexture>();
			if(guiTexture && guiTexture.texture == null)
			{
				guiTexture.texture = texOutput;
			}
		}
	}

}
