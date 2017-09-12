using System;
using System.Globalization;
using System.Text;

namespace Rnd.TeklaStructure.Helper
{
    public static class UnitConverter
    {
        public static string ConvertDecimaltoFraction(this double value)
        {
            var val = (Convert.ToDouble(value) / 25.4) * 16.0;
            var roundedvalue = (Math.Round(val, MidpointRounding.AwayFromZero) / 16.0);
            decimal d = Convert.ToDecimal(roundedvalue).HandleNegative();
            decimal num2 = Math.Truncate(d);
            decimal num3 = d - num2;
            uint valB = 0;
            uint result = 1;
            if (num3 > 0M)
            {
                string s = num3.ToString(System.Globalization.CultureInfo.InvariantCulture).Remove(0, 2);
                uint length = (uint)s.Length;
                valB = (uint)Math.Pow(10.0, (double)length);
                uint.TryParse(s, out result);
                uint num7 = GreatestCommonDivisor(result, valB);
                valB /= num7;
                result /= num7;
            }
            StringBuilder builder = new StringBuilder();
            if (num2 > 0M)
            {
                builder.Append(num2);

            }
            if (num3 > 0M)
            {
                if (result / valB != 1)
                {
                    builder.Append(" ");
                    builder.Append(result);
                    builder.Append("/");
                    builder.Append(valB);
                }
            }
            builder.Append("\"");
            return builder.ToString();
        }
        private static decimal HandleNegative(this decimal value)
        {
            if (value.ToString().Contains("-"))
            {
                return Convert.ToDecimal(value.ToString().Replace("-", ""));
            }
            return value;
        }
        private static uint GreatestCommonDivisor( uint valA, uint valB)
        {
            if ((valA == 0) && (valB == 0))
            {
                return 0;
            }
            if ((valA == 0) && (valB != 0))
            {
                return valB;
            }
            if ((valA != 0) && (valB == 0))
            {
                return valA;
            }
            uint num = valA;
            uint num2 = valB;
            while (num != num2)
            {
                if (num > num2)
                {
                    num -= num2;
                }
                else
                {
                    num2 -= num;
                }
            }
            return num;
        }

        public static double ToDouble(this string value)
        {
            return Convert.ToDouble(value);
        }

        public static int Round0To5(this double value)
        {

            var toIntValue = 0;
            var toStrValue = value.ToString(CultureInfo.InvariantCulture);
            var split = toStrValue.Split('.');


            toIntValue = Convert.ToInt32(split[0]);


            var mod = toIntValue % 10;
            var num = 0;


            if (mod >= 1 && mod <= 4)
            {
                num = 5 - mod;
                return Convert.ToInt32(toIntValue + num);
            }
            if (mod >= 6 && mod <= 9)
            {
                num = 10 - mod;
                return Convert.ToInt32(toIntValue + num);
            }


            return Convert.ToInt32(toIntValue);
        }
    }
}