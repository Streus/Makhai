using System;

namespace Makhai.Core
{
	/// <summary>
	/// Binds a Stat value between a min and max value
	/// </summary>
	[Serializable]
	public class BoundedStat : Stat
	{
		/// <summary>
		/// The lowest possible value.
		/// </summary>
		public float Min { get; set; }

		/// <summary>
		/// The highest possible value.
		/// </summary>
		public float Max { get; set; }

		public BoundedStat(Stat stat, float min, float max) : this (stat, min) { Max = max; }
		public BoundedStat(Stat stat, float min) : this (stat) { Min = min; }
		public BoundedStat(Stat stat)
		{
			Base = stat.Base;
			Additive = stat.Additive;
			Multiplier = stat.Multiplier;
			Min = 0f;
			Max = 0f;
		}

		/// <summary>
		/// Give the result of (Base + Additive) * Multiplier; Min < Value < Max.
		/// </summary>
		/// <returns></returns>
		public override float GetValue()
		{
			float unboundedVal = base.GetValue ();
			return unboundedVal > Max ? Max 
				: unboundedVal < Min ? Min 
				: unboundedVal;
		}

		public override bool Equals(object obj)
		{
			BoundedStat other = (BoundedStat)obj;
			return base.Equals (obj)
				&& other.Min == Min
				&& other.Max == Max;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		public override string ToString()
		{
			return "(" + Base + " + " + Additive + ") * " + Multiplier + " { " + Min + " <= V <= " + Max + "}" + " = " + GetValue ();
		}

		public override object Clone()
		{
			BoundedStat clone = new BoundedStat ((Stat)base.Clone ());
			clone.Min = Min;
			clone.Max = Max;
			return clone;
		}
	}
}
