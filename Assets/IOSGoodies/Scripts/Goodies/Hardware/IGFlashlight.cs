#if UNITY_IOS
using System.Runtime.InteropServices;
using DeadMosquito.IosGoodies.Internal;
using JetBrains.Annotations;
using UnityEngine;

namespace DeadMosquito.IosGoodies
{
	/// <summary>
	///     Class to interact with flashlight
	/// </summary>
	[PublicAPI]
	public static class IGFlashlight
	{
		/// <summary>
		/// 	Indicates whether current device has a flashlight
		/// </summary>
		/// <returns><code>true</code> if current device has a flashlight, <code>false</code> otherwise</returns>
		[PublicAPI]
		public static bool HasTorch
		{
			get
			{
				if (IGUtils.IsIosCheck())
				{
					return false;
				}

				return _goodiesDeviceHasFlashlight();
			}
		}

		/// <summary>
		///     Toggle the flashlight
		/// </summary>
		/// <param name="enable">Whether to enable or disable the flashlight</param>
		[PublicAPI]
		public static void EnableFlashlight(bool enable)
		{
			if (IGUtils.IsIosCheck())
			{
				return;
			}

			_goodiesEnableFlashlight(enable);
		}

		/// <summary>
		/// Enables flashlight with the provided intensity
		/// </summary>
		/// <param name="intensity">Intensity of the flashlight to set. Clamped between 0 and 1</param>
		[PublicAPI]
		public static void SetFlashlightIntensity(float intensity)
		{
			intensity = Mathf.Clamp01(intensity);

			if (IGUtils.IsIosCheck())
			{
				return;
			}

			_goodiesSetFlashlightLevel(intensity);
		}

		[DllImport("__Internal")]
		static extern void _goodiesEnableFlashlight(bool enable);

		[DllImport("__Internal")]
		static extern void _goodiesSetFlashlightLevel(float val);

		[DllImport("__Internal")]
		static extern bool _goodiesDeviceHasFlashlight();
	}
}
#endif