using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.VR_Robotica.Avatars
{
	/// <summary>
	/// This animates random Eye Blinking through coroutines.
	/// 3D: finds the skin mesh renderer and animates the blendshapes denoted in the editor.
	/// 2D: Coming Soon 
	/// </summary>
	public class VRR_EyeBlink_Controller : MonoBehaviour
	{
		[HideInInspector]	public bool ShowDebugLog;
		[Space]
		[HideInInspector]	public EyeStyle AnimatedEye = EyeStyle._3D;
		[Header("3D Properties")]
		[HideInInspector]	public GameObject MeshWithEyeLidShapes;
		[HideInInspector]	public int eyelidTopLeft;
		[HideInInspector]	public int eyelidTopRight;
		[HideInInspector]	public int eyelidBotLeft;
		[HideInInspector]	public int eyelidBotRight;
		[Space]
		[Header("2D Properties")]
		[HideInInspector]	public Material Material_LeftEye;
		[HideInInspector]	public Material Material_RightEye;
		[HideInInspector]	public ImageSource ImageSourceFormat;
		[HideInInspector]	public AtlasAlignment atlasAlignment;
		[HideInInspector]	public bool	 ReverseAnimation;
		[HideInInspector]	public float BetweenFrameRate	= 0.01f;
		[HideInInspector]	public float EyeClosedPause		= 0.1f;
		// if using a Sequence Of Images
		[HideInInspector]	public Texture2D[] BlinkAnimationFrames;
		// if using a Texture Atlas / Sprite Sheet
		[HideInInspector]	public Texture2D BlinkAnimationAtlas;
		// if using a square atlas, define division of rows and columns
		[HideInInspector]	public int SquareAtlasDivision = 1;

		public bool IsSleeping;

		// 3D Ref
		private SkinnedMeshRenderer _skinnedMeshRenderer;
		// 2D Ref
		private int numberOfFrames;
		private float scaleTexture;

		private bool _isBlinking;

		public IEnumerator Create()
		{
			if (AnimatedEye == EyeStyle._3D)
			{
				_skinnedMeshRenderer = MeshWithEyeLidShapes.GetComponent<SkinnedMeshRenderer>();
			}
			else
			if(AnimatedEye == EyeStyle._2D)
			{
				if (ImageSourceFormat == ImageSource.SequenceOfImages)
				{
					numberOfFrames = BlinkAnimationFrames.Length;
				}

				// if we are using a texture atlas (aka sprite sheet)
				if (ImageSourceFormat == ImageSource.TextureAtlas)
				{
					// determine the size of each sprite frame
					checkAtlasShape();
				}
			}

			StartCoroutine(blinkCycle());
			yield return null;
		}

		private IEnumerator blinkCycle()
		{
			float randomNumber;
			while (true)
			{
				//Debug.Log("Blink Cycle...");
				randomNumber = Random.Range(1.0f, 10.0f);

				if (!_isBlinking)
				{
					if (randomNumber < 8.2f)
					{
						// 82% Chance of SINGLE BLINK
						yield return SingleBlink();
					}
					else
					{
						// 18% Chance of DOUBLE BLINK
						yield return SingleBlink();
						yield return new WaitForSeconds(0.35f);
						yield return SingleBlink();
					}
				}

				//Activate blink again in a random range of 5 to 15 seconds for a *natural* look
				yield return new WaitForSeconds(Random.Range(5.0f, 15.0f));
			}
		}

		public IEnumerator SingleBlink()
		{
			if (!_isBlinking)
			{
				switch(AnimatedEye)
				{
					case EyeStyle._3D :
						adjustEyelids(100);

						_isBlinking = true;

						// wait for a moment while closed
						yield return new WaitForSeconds(0.25f);

						// now open *mostly*
						adjustEyelids(15);

						yield return new WaitForSeconds(0.1f);

						// open completely!
						adjustEyelids(0);

						_isBlinking = false;
					break;

					case EyeStyle._2D :
						_isBlinking = true;

						// Start Animation
						if (!ReverseAnimation)
							StartCoroutine(playAnimation("forward"));
						else
							StartCoroutine(playAnimation("reverse"));

						// pause while closed
						yield return new WaitForSeconds(EyeClosedPause);

						// End Animation
						if (!ReverseAnimation)
							StartCoroutine(playAnimation("reverse"));
						else
							StartCoroutine(playAnimation("forward"));

						_isBlinking = false;
						break;
				}
			}
			yield return null;
		}

		public void AvatarSleep()
		{
			StopAllCoroutines();

			switch (AnimatedEye)
			{
				case EyeStyle._3D:

					break;

				case EyeStyle._2D:
					// eye close animation
					if (!ReverseAnimation)
					{
						StartCoroutine(playAnimation("forward"));
					}
					else
					{
						StartCoroutine(playAnimation("reverse"));
					}
					break;
			}

			IsSleeping = true;
		}

		public void AvatarWake()
		{
			IsSleeping = false;
			_isBlinking = true; // set this so blinkCycle doesn't override the eye open animation

			StopAllCoroutines();

			switch (AnimatedEye)
			{
				case EyeStyle._3D:

					break;

				case EyeStyle._2D:
					// eye open animation
					if (!ReverseAnimation)
						StartCoroutine(playAnimation("reverse"));
					else
						StartCoroutine(playAnimation("forward"));
					break;
			}

			StartCoroutine(blinkCycle());
			_isBlinking = false; // reset this so blinkCycle starts working again
		}

		#region 3D FUNCTIONS
		private void adjustEyelids(float amount)
		{
			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidTopLeft, amount);
			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidTopRight, amount);

			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidBotLeft, amount);
			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidBotRight, amount);
		}
		#endregion

		#region 2D FUNCTIONS
		private void checkAtlasShape()
		{
			int frameWidth = BlinkAnimationAtlas.width;
			int frameHeight = BlinkAnimationAtlas.height;

			// determine the shape/layout of the texture atlas...

			// Using a horizontal texture atlas:
			if (frameWidth > frameHeight)
			{
				numberOfFrames = frameWidth / frameHeight;
				scaleTexture = 1 / (numberOfFrames * 1.0f); // multiply int by 1.0f to quickly convert to float
				atlasAlignment = AtlasAlignment.Horizontal;
			}
			else
			// Using a vertical texture atlas:
			if (frameHeight > frameWidth)
			{
				Debug.Log("Using VERTICAL ATLAS for Eyes");
				numberOfFrames = frameHeight / frameWidth;
				scaleTexture = 1 / (numberOfFrames * 1.0f); // multiply int by 1.0f to quickly convert to float
				atlasAlignment = AtlasAlignment.Vertical;
			}
			else
			// Using a square atlas:
			if (frameHeight == frameWidth)
			{
				Debug.Log("Using SQUARE ATLAS for Eyes");
				numberOfFrames = (SquareAtlasDivision * SquareAtlasDivision);
				scaleTexture = 1 / (SquareAtlasDivision * 1.0f);
				atlasAlignment = AtlasAlignment.Square;
			}
			else
			{
				// this should never happen
				Debug.LogError("Can't determine shape of texture atlas");
			}
		}

		private IEnumerator playAnimation(string direction)
		{
			int frame;

			switch (direction)
			{
				case "forward":
					for (frame = 0; frame < numberOfFrames; frame++)
					{
						if (ImageSourceFormat == ImageSource.SequenceOfImages)
							swapTexture(frame);

						if (ImageSourceFormat == ImageSource.TextureAtlas)
							offsetTexture(frame);

						yield return new WaitForSeconds(BetweenFrameRate);
					}
					break;

				case "reverse":
					for (frame = numberOfFrames - 1; frame >= 0; frame--)
					{
						if (ImageSourceFormat == ImageSource.SequenceOfImages)
							swapTexture(frame);

						if (ImageSourceFormat == ImageSource.TextureAtlas)
							offsetTexture(frame);

						yield return new WaitForSeconds(BetweenFrameRate);
					}
					break;
			}
		}

		// When using a sequence of images for your animation...
		private void swapTexture(int frame)
		{
			if (ShowDebugLog) { Debug.Log("Swapping Textures"); }

			// change the material's texture to those from the array
			Material_LeftEye.SetTextureScale("_TopTex", new Vector2(1, 1));
			Material_RightEye.SetTextureScale("_TopTex", new Vector2(1, 1));

			Material_LeftEye.SetTexture("_TopTex", BlinkAnimationFrames[frame]);
			Material_RightEye.SetTexture("_TopTex", BlinkAnimationFrames[frame]);
		}

		// When using a texture atlas for your animation frames...
		private void offsetTexture(int frame)
		{
			if (ShowDebugLog)
				Debug.Log("Off-Setting Texture");

			// make sure the material references are ready before off setting...
			if (Material_LeftEye != null && Material_RightEye != null)
			{
				// change the material's texture to the atlas texture
				Material_LeftEye.SetTexture("_TopTex", BlinkAnimationAtlas);
				Material_RightEye.SetTexture("_TopTex", BlinkAnimationAtlas);


				switch (atlasAlignment)
				{
					case AtlasAlignment.Horizontal:
						if (ShowDebugLog) { Debug.Log("HORIZONTAL ATLAS!!!"); }

						Material_LeftEye.SetTextureScale("_TopTex", new Vector2(scaleTexture, 1));
						Material_RightEye.SetTextureScale("_TopTex", new Vector2(scaleTexture, 1));

						Material_LeftEye.SetTextureOffset("_TopTex", new Vector2(frame * scaleTexture, 0));
						Material_RightEye.SetTextureOffset("_TopTex", new Vector2(frame * scaleTexture, 0));
					break;

					case AtlasAlignment.Vertical:
						if (ShowDebugLog) { Debug.Log("VERTICAL ATLAS!!!"); }

						Material_LeftEye.SetTextureScale("_TopTex", new Vector2(1, scaleTexture));
						Material_LeftEye.SetTextureOffset("_TopTex", new Vector2(0, frame * scaleTexture));

						Material_RightEye.SetTextureScale("_TopTex", new Vector2(1, scaleTexture));
						Material_RightEye.SetTextureOffset("_TopTex", new Vector2(0, frame * scaleTexture));
					break;

					// *** Square Atlases are not yet supported
					case AtlasAlignment.Square:
						if (ShowDebugLog) { Debug.Log("SQUARE ATLAS!!!"); }

						Material_LeftEye.SetTextureScale("_TopTex", new Vector2(scaleTexture, scaleTexture));
						Material_RightEye.SetTextureScale("_TopTex", new Vector2(scaleTexture, scaleTexture));
					break;
				}
			}
		}

		#endregion
	}
}