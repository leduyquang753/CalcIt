using System;

namespace CalcItCore {
	public class CoreUtils {
		private const double
			degInRad = Math.PI / 180,
			radInDeg = 180 / Math.PI,
			degInGrad = 10 / 9,
			gradInDeg = 0.9,
			gradInRad = Math.PI / 200,
			radInGrad = 200 / Math.PI;

		///<summary>
		///Power function.
		///</summary>
		public static double power(double baseNum, double exponent, CalculatorEngine engine) {
			if (baseNum == 0) if (exponent > 0) return 0; else throw new ExpressionInvalidException("divisionByZero");
			if (exponent < 0) return 1 / Math.Pow(baseNum, -exponent);
			double roundedExponent = Math.Round(exponent, MidpointRounding.AwayFromZero);
			if (Math.Abs(roundedExponent - exponent) < 1E-11)
				if (baseNum > 0 || mod(roundedExponent, 2) == 0) return Math.Pow(baseNum, roundedExponent); else return -Math.Pow(-baseNum, roundedExponent);
			else if (baseNum > 0) return Math.Pow(baseNum, exponent); else throw new ExpressionInvalidException("unsupportedExponentiation", messageArguments: new[] { formatNumber(baseNum, engine), formatNumber(exponent, engine) });
		}

		public static double degToRad(double degs) => degInRad * degs;

		public static double radToDeg(double rads) => radInDeg * rads;

		public static double degToGrad(double degs) => degInGrad * degs;

		public static double gradToDeg(double grads) => gradInDeg * grads;

		public static double radToGrad(double rads) => radInGrad * rads;

		public static double gradToRad(double grads) => gradInRad * grads;

		public static double div(double dividend, double divisor) => Math.Floor(dividend / divisor);

		public static double mod(double dividend, double divisor) => dividend - Math.Floor(dividend / divisor) * divisor;

		public static double roundUp(double num) => num >= 0 ? Math.Ceiling(num) : Math.Floor(num);

		public static double roundDown(double num) => num >= 0 ? Math.Floor(num) : Math.Ceiling(num);

		private static string getFormattedNumberInternal(double number, CalculatorEngine engine, char mulSign) {
			string toReturn = number.ToString("#,##0.##########").Replace(",", " ").Replace("E", mulSign + "10^");
			if (!engine.decimalDot) toReturn = toReturn.Replace(".", ",");
			if (engine.thousandDot) toReturn = toReturn.Replace(" ", engine.decimalDot ? "," : ".");
			return toReturn;
		}

		public static string formatNumber(double number, CalculatorEngine engine) {
			try {
				char mulSign = engine.mulAsterisk || engine.decimalDot || (!engine.decimalDot && engine.thousandDot) ? '*' : '.';
				string toReturn;
				double log = Math.Log10(Math.Abs(number));
				if (number != 0 && (log <= -7 || log >= 18))
				{
					int exponent = (int)Math.Floor(Math.Log10(Math.Abs(number)) / 3) * 3;
					double displayedNumber = number * Math.Pow(10, -exponent);
					if (Double.IsNaN(displayedNumber)) return null;
					toReturn = getFormattedNumberInternal(displayedNumber, engine, mulSign);
					return exponent == 0 ? toReturn : toReturn + mulSign + "10^" + exponent;
				}
				string formatted = getFormattedNumberInternal(number, engine, mulSign);
				toReturn = "";
				int digitCount = -1;
				char decimalSeparator = engine.decimalDot ? '.' : ',';
				foreach (char c in formatted) {
					if (c == decimalSeparator) {
						toReturn += c;
						digitCount = 0;
					}
					else if (c == mulSign) {
						toReturn += c;
						digitCount = -1;
					} else {
						if (digitCount == -1) {
							toReturn += c;
							continue;
						}
						else if (digitCount == 10) continue;
						else {
							toReturn += c;
							digitCount++;
						}
					}
				}
				return toReturn;
			} catch (OverflowException) {
				return null;
			}
		}

		public static int getIndexWithWhitespace(string text, int indexWithoutWhitespace) {
			int position = -1, oldPosition = -1;
			foreach (char c in text) {
				position++;
				if (c != ' ' && ++oldPosition == indexWithoutWhitespace-1) return position+1;
			}
			return position + 1;
		}

		public static readonly int[] days = new[] { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
		public static double getMonthDays(double year, int month) => month == 2 ? isLeapYear(year) ? 29 : 28 : days[month];
		public static bool divisible(double dividend, double divisor) => mod(dividend, divisor) < 0.5;
		public static bool isLeapYear(double year) => divisible(year, 4) && (!divisible(year, 100) || divisible(year, 400));
		public static readonly int[] monthPos     = new[] { 0, 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365 },
		                             monthPosLeap = new[] { 0, 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366 };

		public static double[] getYearAndDayOfYearFromIndex(double index) {
			int cycles;
			double temp;
			double year = (temp = Math.Floor(index / 146097)) * 400; // Gregorian calendar repeats every 146 097 days, or 400 years.
			if ((int)(index -= temp * 146097) == 146096) return new[] { year + 400, 365 }; // Handle the last day of the cycle, which is the 366th day of the 400th year.
			return new[] { year	+ (cycles = (int)Math.Floor(index / 36524)) * 100 // In each repeat cycle, it repeats every 100 years, or 36 524 days; the only irregular year is the 400th year which is a leap year.
			                    + (cycles = (int)Math.Floor((index -= cycles * 36524) / 1461)) * 4 // In that sub-cycle, it also repeats every 4 years or 1461 days, except the 100th which is not a leap year.
				                + (cycles = (int)Math.Floor((index -= cycles * 1461) / 365)) // In that sub-sub-cycle, it also repeats every year, or 365 days, except the 4th which is a leap year.
				                + (cycles == 4 ? 0 : 1), // Handle the last day of the 4-year cycle.
				cycles == 4 ? 365 : index - cycles * 365
			};
		}

		public static double[] getMonthAndDayOfMonthFromIndex(double index) {
			double[] res = getYearAndDayOfYearFromIndex(index);
			int[] table = isLeapYear(res[0]) ? monthPosLeap : monthPos;
			int i;
			for (i = 0; i < 12; i++) if ((int)Math.Floor(res[1]) < table[i+1]) break;
			return new[] { i, res[1] - table[i] + 1 };
		}

		public static double getHourFromIndex(double index) {
			return Math.Truncate(24 * mod(index, 1));
		}
	}
}
