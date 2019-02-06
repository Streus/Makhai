using System;
using UnityEngine;

namespace Makhai.ComplexStats
{
	/// <summary>
	/// Represents a compound value of the form (B * M) + A.
	/// </summary>
	[Serializable]
	public class Stat : ICloneable
	{
		#region OPERATORS
		public static bool operator ==(Stat left, Stat right)
		{
			return left.GetValue () == right.GetValue ();
		}

		public static bool operator !=(Stat left, Stat right)
		{
			return left.GetValue () != right.GetValue ();
		}

		public static Stat operator +(Stat left, float right)
		{
			Stat result = (Stat)left.Clone ();
			result.Additive += right;
			return result;
		}

		public static Stat operator -(Stat left, float right)
		{
			Stat result = (Stat)left.Clone ();
			result.Additive -= right;
			return result;
		}

		public static Stat operator *(Stat left, float right)
		{
			Stat result = (Stat)left.Clone ();
			result.Multiplier *= right;
			return result;
		}

		public static Stat operator /(Stat left, float right)
		{
			Stat result = (Stat)left.Clone ();
			result.Multiplier /= right;
			return result;
		}
		#endregion

		/// <summary>
		/// Starting value affected by Multiplier.
		/// </summary>
		public float Base { get { return baseValue; } set { baseValue = value; } }
		[SerializeField]
		private float baseValue;

		/// <summary>
		/// Added to the Base after the Multiplier is applied.
		/// </summary>
		public float Additive { get { return additiveValue; } set { additiveValue = value; } }
		[SerializeField]
		private float additiveValue;

		/// <summary>
		/// Applied to Base before Additive is added.
		/// </summary>
		public float Multiplier { get { return multiplicitiveValue; } set { multiplicitiveValue = value; } }
		[SerializeField]
		private float multiplicitiveValue;

		public Stat() : this (0) { }
		public Stat(float startingBase)
		{
			Base = startingBase;
			Additive = 0;
			Multiplier = 1;
		}

		/// <summary>
		/// Gives the result of (Base * Multiplier) + Additive.
		/// </summary>
		/// <returns></returns>
		public virtual float GetValue()
		{
			return (Base * Multiplier) + Additive;
		}

		/// <summary>
		/// Used in conjunction with Merge().
		/// </summary>
		/// <param name="left">The left operand</param>
		/// <param name="right">The right operand</param>
		/// <returns></returns>
		public delegate float Operator(float left, float right);

		/// <summary>
		/// Creates a new Stat that has the combined stats of the current Stat and some other Stat.
		/// </summary>
		/// <param name="other">The Stat to merge with</param>
		/// <param name="combineMethod">The way to merge the individual members of the Stats (e.g. + or *)</param>
		/// <returns></returns>
		public Stat Combine(Stat other, Operator combineMethod)
		{
			Stat merged = (Stat)Clone ();
			merged.Base = combineMethod(merged.Base, other.Base);
			merged.Additive = combineMethod(merged.Additive, other.Additive);
			merged.Multiplier = combineMethod(merged.Multiplier, other.Multiplier);
			return merged;
		}

		/// <summary>
		/// Combines the members of two Stats additively.
		/// </summary>
		/// <param name="other">The stat to merge with</param>
		/// <returns></returns>
		public Stat Merge(Stat other)
		{
			return Combine (other, (float left, float right) => { return left + right; });
		}

		/// <summary>
		/// Combines the members of this Stat with the inverse of another's members.
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Stat Split(Stat other)
		{
			return Combine (other, (float left, float right) => { return left - right; });
		}

		public override bool Equals(object obj)
		{
			Stat other = (Stat)obj;
			return other.Base == Base 
				&& other.Additive == Additive 
				&& other.Multiplier == Multiplier; 
		}

		public override int GetHashCode()
		{
			return GetValue ().GetHashCode ();
		}

		public override string ToString()
		{
			return "(" + Base + " + " + Additive + ") * " + Multiplier + " = " + GetValue ();
		}

		public virtual object Clone()
		{
			return new Stat (Base)
			{
				Additive = Additive,
				Multiplier = Multiplier
			};
		}
	}
}
