using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace DeadMosquito.IosGoodies.Example
{
	public class IGHardwareExample : MonoBehaviour
	{
		[SerializeField]
		Slider _lightIntensitySlider;

		void Awake()
		{
#if UNITY_IOS
			_lightIntensitySlider.onValueChanged.AddListener(val =>
			{
				if (!IGFlashlight.HasTorch)
				{
					return;
				}

				IGFlashlight.SetFlashlightIntensity(val);
			});  
#endif
		}
		
#if UNITY_IOS
		bool _torchLightEnabled;

		[UsedImplicitly]
		public void OnEnableFlashlight()
		{
			if (IGFlashlight.HasTorch)
			{
				_torchLightEnabled = !_torchLightEnabled;
				IGFlashlight.EnableFlashlight(_torchLightEnabled);
			}
			else
			{
				Debug.Log("This device does not have a flashlight");
			}
		}
#endif
	}
}