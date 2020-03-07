using System;
using Windows.ApplicationModel.Resources;

namespace CalcItUWP {
	public class Utils {
		private const double
			degInRad = Math.PI / 180,
			radInDeg = 180 / Math.PI,
			degInGrad = 10 / 9,
			gradInDeg = 0.9,
			gradInRad = Math.PI / 200,
			radInGrad = 200 / Math.PI;
		public static ResourceLoader resourceLoader = null;

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

		public static double mod(double dividend, double divisor) => dividend - Math.Floor(dividend / divisor) * dividend;

		public static string getString(string key) {
			if (resourceLoader == null) throw new InvalidOperationException("Resources have not been loaded.");
			return resourceLoader.GetString(key);
		}

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
					toReturn = getFormattedNumberInternal(number * Math.Pow(10, -exponent), engine, mulSign);
					return exponent == 0 ? toReturn : toReturn + mulSign + "10^" + exponent;
				}
				string formatted = getFormattedNumberInternal(number, engine, mulSign);
				toReturn = "";
				int digitCount = -1;
				char decimalSeparator = engine.decimalDot ? '.' : ',';
				foreach (char c in formatted)
				{
					if (c == decimalSeparator)
					{
						toReturn += c;
						digitCount = 0;
					}
					else if (c == mulSign)
					{
						toReturn += c;
						digitCount = -1;
					}
					else
					{
						if (digitCount == -1)
						{
							toReturn += c;
							continue;
						}
						else if (digitCount == 10) continue;
						else
						{
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
	}
}
