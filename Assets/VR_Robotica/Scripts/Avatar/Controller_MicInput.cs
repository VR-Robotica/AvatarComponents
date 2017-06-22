using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.VR_Robotica.Avatars
{
	public class Controller_MicInput : MonoBehaviour
	{
		public bool RealtimeOutput	= true;
		public bool IsWorking		= true;

		private AudioSource	_audioSource;
		private bool		_lastValueOfIsWorking;
		private bool		_lastValueOfRealtimeOutput;
		private float		_lastVolume = 0;

		void Start()
		{
			_audioSource = GetComponent<AudioSource>();
			if(_audioSource == null)
			{
				_audioSource = this.gameObject.AddComponent<AudioSource>();
			}

			if (IsWorking)
			{
				WorkStart();
			}
		}


		void Update()
		{
			CheckIfIsWorkingChanged();
			CheckIfRealtimeOutputChanged();
		}

		public void CheckIfIsWorkingChanged()
		{
			if (_lastValueOfIsWorking != IsWorking)
			{
				if (IsWorking)
				{
					WorkStart();
				}
				else
				{
					WorkStop();
				}
			}

			_lastValueOfIsWorking = IsWorking;
		}

		public void CheckIfRealtimeOutputChanged()
		{
			if (_lastValueOfRealtimeOutput != RealtimeOutput)
			{
				DisableSound(RealtimeOutput);
			}

			_lastValueOfRealtimeOutput = RealtimeOutput;
		}

		public void DisableSound(bool SoundOn)
		{
			if (SoundOn)
			{
				if (_lastVolume > 0)
				{
					_audioSource.volume = _lastVolume;
				}
				else
				{
					_audioSource.volume = 1f;
				}
			}
			else
			{
				_lastVolume = _audioSource.volume;
				_audioSource.volume = 0f;
			}
		}

		private void WorkStart()
		{
			_audioSource.clip = Microphone.Start(null, true, 10, 44100);
			_audioSource.loop = true;
			while (!(Microphone.GetPosition(null) > 0))
			{
				_audioSource.Play();
			}
		}

		private void WorkStop()
		{
			Microphone.End(null);
		}

	}
}