namespace DeadMosquito.IosGoodies.Example
{
	using System;
	using JetBrains.Annotations;
	using UnityEngine;
	using UnityEngine.UI;

	public class IGDialogsExample : MonoBehaviour
	{
#if UNITY_IOS
		public Text dateText;
		public Text timeText;
		public Text dateAndTimeText;
		public Text countdownTimeText;

		[UsedImplicitly]
		public void OnRequestReviewDialog()
		{
			IGAppStore.RequestReview();
		}

		[UsedImplicitly]
		public void OnShowConfirmationDialog()
		{
			IGDialogs.ShowOneBtnDialog("العالم العربي", "Message", "Confirm", () => Debug.Log("Button clicked!"));
		}

		[UsedImplicitly]
		public void OnShowTwoButtonDialog()
		{
			IGDialogs.ShowTwoBtnDialog("Title", "My awesome message!",
				"Confirm", () => Debug.Log("Confirm button clicked!"),
				"Cancel", () => Debug.Log("Cancel clicked!"));
		}

		[UsedImplicitly]
		public void OnShowThreeButtonDialog()
		{
			IGDialogs.ShowThreeBtnDialog("Title", "My awesome message!",
				"Option 1", () => Debug.Log("Option 1 button clicked!"),
				"Option 2", () => Debug.Log("Option 2 button clicked!"),
				"Cancel", () => Debug.Log("Cancel clicked!")
			);
		}

		#region date_time_picker

		[UsedImplicitly]
		public void OnShowDatePickerNow()
		{
			IGDateTimePicker.ShowDatePicker(OnDateSelected,
				() => Debug.Log("Picking date was cancelled"));
		}

		[UsedImplicitly]
		public void OnShowDatePicker()
		{
			var year = 1991;
			var month = 8; // August
			var day = 11;
			IGDateTimePicker.ShowDatePicker(year, month, day,
				OnDateSelected,
				() => Debug.Log("Picking date was cancelled"));
		}
		
		[UsedImplicitly]
		public void OnSHowDatePickerWIthRestrains()
		{
			var currentYear = 2015;
			var currentMonth = 4;
			var currentDay = 10;
			var minYear = 2014;
			var minMonth = 1;
			var minDay = 1;
			var maxYear = 2016;
			var maxMonth = 5;
			var maxDay = 3;
			IGDateTimePicker.ShowDatePickerWithRestrains(currentYear, currentMonth, currentDay, OnDateSelected,
				() => Debug.Log("Picking date was cancelled"), minYear, minMonth, minDay, maxYear, maxMonth, maxDay);
		}

		[UsedImplicitly]
		public void OnShowTimePickerNow()
		{
			IGDateTimePicker.ShowTimePicker(OnTimeSelected,
				() => Debug.Log("Picking time was cancelled"));
		}

		[UsedImplicitly]
		public void OnShowTimePicker()
		{
			var hourOfDay = 15;
			var minute = 42;
			IGDateTimePicker.ShowTimePicker(hourOfDay, minute,
				OnTimeSelected,
				() => Debug.Log("Picking time was cancelled"));
		}

		[UsedImplicitly]
		public void OnShowTimePickerWithRestrains()
		{
			var currentHour = 10;
			var currentMinute = 30;
			var minHour = 3;
			var minMinute = 17;
			var maxHour = 18;
			var maxMinute = 40;
			IGDateTimePicker.ShowTimePickerWithRestrains(currentHour, currentMinute, OnTimeSelected,
				() => Debug.Log("Picking time was cancelled"), minHour, minMinute, maxHour, maxMinute);
		}

		[UsedImplicitly]
		public void OnShowDateAndTimePickerNow()
		{
			IGDateTimePicker.ShowDateAndTimePicker(OnDateAndTimeTimeSelected,
				() => Debug.Log("Picking date and time was cancelled"));
		}

		[UsedImplicitly]
		public void OnShowDateAndTimePicker()
		{
			var year = 1991;
			var month = 8; // August
			var day = 11;
			var hourOfDay = 15;
			var minute = 42;
			IGDateTimePicker.ShowDateAndTimePicker(year, month, day, hourOfDay, minute,
				OnDateAndTimeTimeSelected,
				() => Debug.Log("Picking date and time was cancelled"));
		}

		[UsedImplicitly]
		public void OnShowDateTimePickerWithRestrains()
		{
			var currentYear = 2015;
			var currentMonth = 4;
			var currentDay = 10;
			var minYear = 2014;
			var minMonth = 1;
			var minDay = 1;
			var maxYear = 2016;
			var maxMonth = 5;
			var maxDay = 3;
			var currentHour = 10;
			var currentMinute = 30;
			var minHour = 3;
			var minMinute = 17;
			var maxHour = 18;
			var maxMinute = 40;
			IGDateTimePicker.ShowDateTimePickerWithRestrains(
				currentYear, currentMonth, currentDay, currentHour, currentMinute, 
				OnDateSelected, () => Debug.Log("Picking date and time was cancelled"), 
				minYear, minMonth, minDay, minHour, minMinute, maxYear, maxMonth, maxDay, maxHour, maxMinute);
		}

		[UsedImplicitly]
		public void OnShowCountdownTimer()
		{
			IGDateTimePicker.ShowCountDownTimer(OnCountDownTimeSelected,
				() => Debug.Log("Picking date and time was cancelled"));
		}

		void OnDateSelected(DateTime date)
		{
			Debug.Log(string.Format("Date selected: year: {0}, month: {1}, day {2}",
				date.Year, date.Month, date.Day));
			var pickedDate = date.ToString("yyyy MMMMM dd");
			dateText.text = string.Format("Date Picker\n{0}", pickedDate);
		}

		void OnTimeSelected(DateTime time)
		{
			Debug.Log(string.Format("Time selected: hour: {0}, minute: {1}",
				time.Hour, time.Minute));
			var pickedTime = time.ToString("hh:mm");
			timeText.text = string.Format("Time Picker\n{0}", pickedTime);
		}

		void OnDateAndTimeTimeSelected(DateTime dateTime)
		{
			Debug.Log(string.Format("Date & Time selected: year: {0}, month: {1}, day {2}, hour: {3}, minute: {4}",
				dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute));

			var pickedDate = dateTime.ToString("G");
			dateAndTimeText.text = string.Format("Date & Time Picker\n{0}", pickedDate);
		}

		void OnCountDownTimeSelected(DateTime countdownTime)
		{
			Debug.Log(string.Format("Countdown time selected: hour: {0}, minute: {1}",
				countdownTime.Hour, countdownTime.Minute));
			var pickedTime = string.Format("{0}:{1}", countdownTime.Hour, countdownTime.Minute);
			countdownTimeText.text = string.Format("Time Picker\n{0}", pickedTime);
		}

		#endregion

		static readonly string[] ActionSheetOptions = {"Option 1", "Option 2", "Option 3"};
		static readonly string[] ActionSheetMoreOptions = {"Option 1", "Option 2", "Option 3", "Extra 1", "Extra 2"};
		
		[UsedImplicitly]
		public void OnShowActionSheet()
		{
			IGActionSheet.ShowActionSheet("Title", "Cancel", () => Debug.Log("Cancel Clicked"),
				ActionSheetOptions, index => Debug.Log(ActionSheetOptions[index] + " Clicked"));
		}

		[UsedImplicitly]
		public void OnShowActionSheetWithDestructiveButton()
		{
			IGActionSheet.ShowActionSheet("Title",
				"Cancel", () => Debug.Log("Cancel Clicked"),
				"Destroy All!", () => Debug.Log("Destroy All Clicked"),
				ActionSheetMoreOptions, index => Debug.Log(ActionSheetMoreOptions[index] + " Clicked"));
		}

#endif
	}
}