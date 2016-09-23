namespace WpfControls.TagBox.Controls
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NumericExtensions
	{
		[StructLayout(LayoutKind.Explicit)]
		private struct NanUnion
		{
			[FieldOffset(0)]
			internal double FloatingValue;
			[FieldOffset(0)]
			internal readonly ulong IntegerValue;
		}

		public static bool IsZero(this double value)
		{
			return Math.Abs(value) < 2.2204460492503131E-15;
		}

		public static bool IsNaN(this double value)
		{
			var nanUnion = new NanUnion
			{
				FloatingValue = value
			};

			var num = nanUnion.IntegerValue & 18442240474082181120uL;
			bool result;

			if (num != 9218868437227405312uL && num != 18442240474082181120uL)
			{
				result = false;
			}
			else
			{
				var num2 = nanUnion.IntegerValue & 4503599627370495uL;
				result = (num2 != 0uL);
			}

			return result;
		}

		public static bool IsGreaterThan(double left, double right)
		{
			return left > right && !AreClose(left, right);
		}

		public static bool IsLessThanOrClose(double left, double right)
		{
			return left < right || AreClose(left, right);
		}

		public static bool AreClose(double left, double right)
		{
			if (left == right)
			{
				return true;
			}

		    var num = (Math.Abs(left) + Math.Abs(right) + 10.0) * 2.2204460492503131E-16;
		    var num2 = left - right;
		    return (-num < num2 && num > num2);
		}
	}
}
