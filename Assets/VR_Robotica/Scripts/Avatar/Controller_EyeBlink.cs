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
	public class Controller_EyeBlink : MonoBehaviour
	{
		public GameObject MeshWithEyeLidShapes;

		public int eyelidTopLeft;
		public int eyelidTopRight;
		public int eyelidBotLeft;
		public int eyelidBotRight;

		private SkinnedMeshRenderer _skinnedMeshRenderer;
		private bool _isBlinking;

		// Use this for initialization
		void Start()
		{
			_skinnedMeshRenderer = MeshWithEyeLidShapes.GetComponent<SkinnedMeshRenderer>();
			StartCoroutine(blinkCycle());
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
						yield return new WaitForSeconds(0.25f);
						yield return SingleBlink();
					}
				}

				//Activate blink again in a random range of 2 to 15 seconds for a *natural* look
				yield return new WaitForSeconds(Random.Range(1.0f, 8.0f));
			}
		}

		public IEnumerator SingleBlink()
		{
			if (!_isBlinking)
			{
				adjustEyelids(100);

				_isBlinking = true;

				// wait for a moment while closed
				yield return new WaitForSeconds(0.1f);

				// now open *mostly*
				adjustEyelids(15);

				yield return new WaitForSeconds(0.05f);

				// open completely!
				adjustEyelids(0);
				_isBlinking = false;
			}
			yield return null;
		}

		private void adjustEyelids(float amount)
		{
			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidTopLeft, amount);
			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidTopRight, amount);

			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidBotLeft, amount);
			_skinnedMeshRenderer.SetBlendShapeWeight(eyelidBotRight, amount);
		}
	}
}