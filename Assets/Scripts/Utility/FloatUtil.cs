using UnityEngine;

namespace FruitGolf.Utility
{
	/// <summary>
	/// Provides with some utility methods for float numbers
	/// </summary>
	public static class FloatUtil
	{
		/// <summary>
		/// Constant representing 2Pi (a full circle in radians)
		/// </summary>
		public const float Pi2 = Mathf.PI * 2F;
	
		/// <summary>
		/// An epsilon value used for "AlmostEquals" methods.
		/// Values that are less than this epsilon value will be considered "equal"
		/// </summary>
		public const float Epsilon = 0.00001f;
	
		/// <summary>
		/// Returns true if the difference between both numbers is less than the
		/// epsilon value (a threshold)
		/// </summary>
		/// <param name="value1">value to be compared against value2</param>
		/// <param name="value2">value to be compared against value1</param>
		/// <param name="epsilon">The difference threshold to be considered
		/// for the comparison.
		/// If no value is given, the constant Epsilon from this class will be used </param>
		/// <returns>true if the difference between both numbers is less than
		/// the epsilon value (a threshold)</returns>
		public static bool AlmostEquals(float value1, float value2, float epsilon = Epsilon)
		{
			return Mathf.Abs(value1 - value2) < epsilon;
		}
	}
}
