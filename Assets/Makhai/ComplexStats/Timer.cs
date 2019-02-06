using System;
using UnityEngine;

namespace Makhai.ComplexStats
{
	/// <summary>
	/// A simple timer that counts down from a max value (allows for mirroring to count up).
	/// </summary>
	[Serializable]
	public class Timer
	{
		#region INSTANCE_VARS

		/// <summary>
		/// Starts at Max and counts down to 0.
		/// </summary>
		public float Value { get { return value; } private set { this.value = value; } }
		[SerializeField]
		private float value;

		/// <summary>
		/// Starts at 0 and counts up to Max.
		/// </summary>
		public float MirrorValue { get { return Max - Value; } }

		/// <summary>
		/// The maximum timer value.
		/// </summary>
		public float Max { get { return max; } private set { max = value; } }
		[SerializeField]
		private float max;

		// Halts updating of the value if true
		[SerializeField]
		private bool paused;

		public float Percentage { get { return Value / Max; } }
		public float MirrorPercentage { get { return 1 - Percentage; } }
		#endregion

		#region INSTANCE_METHODS

		public Timer() : this (1f) { }
		public Timer(float max)
		{
			Max = max;
			Value = Max;
			paused = false;
		}

		/// <summary>
		/// Updates the value by delta.  Returns true if the value is equal to
		/// zero after the update, false otherwise.
		/// </summary>
		public bool Tick(float delta)
		{
			if(!paused)
				Value -= Math.Min (delta, Value);

			return Value <= 0f;
		}

		/// <summary>
		/// Returns true if the value is equal to zero, false otherwise.
		/// </summary>
		public bool IsCompleted()
		{
			return Value <= 0f;
		}

		/// <summary>
		/// Sets the value back to Max.
		/// </summary>
		public void Reset()
		{
			Value = Max;
		}

		/// <summary>
		/// Sets the value to zero.
		/// </summary>
		public void Complete()
		{
			Value = 0f;
		}

		/// <summary>
		/// Changes the Max, conserving completion percentage value as 
		/// closely as possible by adjusting Value.
		/// </summary>
		/// <param name="newMax"></param>
		public void AdjustMax(float newMax)
		{
			float oldPerc = Percentage;
			Max = newMax;
			Value = Max * oldPerc;
		}

		/// <summary>
		/// Checks if this Timer is paused.
		/// </summary>
		public bool IsPaused()
		{
			return paused;
		}

		/// <summary>
		/// Registers a pause.
		/// </summary>
		public void SetPause(bool val)
		{
			paused = val;
		}

		public override string ToString()
		{
			return Value + " / " + Max;
		}
		#endregion
	}
}
