namespace WpfControls.TagBox.Converters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;

    using WpfControls.TagBox.Controls;

    public class LengthConverter : TypeConverter
	{
		private static readonly Dictionary<string, double> UnitToPixelConversions;

        public override bool CanConvertFrom(ITypeDescriptorContext typeDescriptorContext, Type sourceType)
        {
            switch (Type.GetTypeCode(sourceType))
            {
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.String:
                    return true;
            }

            return false;
        }

        public override object ConvertFrom(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object source)
		{
			if (source == null)
			{
				var text = string.Format(CultureInfo.CurrentCulture, "Cannot convert from type {0} to type {1}", base.GetType().Name, "null");

				throw new NotSupportedException(text);
			}

			var text2 = source as string;

			if (text2 != null)
			{
				if (string.Compare(text2, "Auto", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					return double.NaN;
				}

				var text3 = text2;
				var num = 1.0;

			    foreach (var unitToPixelConversion in UnitToPixelConversions)
			    {
                    if (text3.EndsWith(unitToPixelConversion.Key, StringComparison.InvariantCultureIgnoreCase))
                    {
                        num = unitToPixelConversion.Value;
                        text3 = text2.Substring(0, text3.Length - unitToPixelConversion.Key.Length);
                        break;
                    }
                }

				try
				{
					return num * Convert.ToDouble(text3, cultureInfo);
				}
				catch (FormatException)
				{
					var text = string.Format(CultureInfo.CurrentCulture, "Cannot convert from type {0} {1} {2}", base.GetType().Name, text2, typeof(double).Name);
					throw new FormatException(text);
				}
			}

            return Convert.ToDouble(source, cultureInfo);
		}

		public override bool CanConvertTo(ITypeDescriptorContext typeDescriptorContext, Type destinationType)
		{
			return TypeConverters.CanConvertTo<double>(destinationType);
		}

		public override object ConvertTo(ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value, Type destinationType)
		{
			object result;
			if (value is double)
			{
				var num = (double)value;
				if (destinationType == typeof(string))
				{
					result = (num.IsNaN() ? "Auto" : Convert.ToString(num, cultureInfo));
					return result;
				}
			}
			result = TypeConverters.ConvertTo(this, value, destinationType);
			return result;
		}

		static LengthConverter()
		{
			// Note: this type is marked as 'beforefieldinit'.
		    var dictionary = new Dictionary<string, double>
		                         {
		                             { "px", 1.0 },
		                             { "in", 96.0 },
		                             { "cm", 37.795275590551178 },
		                             { "pt", 1.3333333333333333 }
		                         };
		    UnitToPixelConversions = dictionary;
		}
	}
}
