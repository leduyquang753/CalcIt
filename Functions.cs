using static CalcItUWP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcItUWP.Functions {
	public abstract class Function {
		public string[] names { get; }
		protected static Random random { get; } = new Random();
		public Function(string[] names) {
			this.names = names;
		}
		public abstract double calculate(List<double> arguments, CalculatorEngine engine);
		protected static double total(List<double> arguments) {
			double total = 0;
			foreach (double d in arguments) total += d;
			return total;
		}
	}

	public class Sum: Function {
		public Sum(): base(new string[] { "", "sum", "total" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => total(arguments);
	}

	public class Sin: Function {
		public Sin(): base(new string[] { "sin", "sine" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => Math.Sin(engine.angleUnit.convertToRadians(total(arguments)));
	}

	public class Cos: Function {
		public Cos(): base(new string[] { "cos", "cosine" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => Math.Cos(engine.angleUnit.convertToRadians(total(arguments)));
	}

	public class Tan: Function {
		public Tan(): base(new string[] { "tan", "tangent", "tang", "tg" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double tot = total(arguments);
			if (Math.Cos(tot) == 0) throw new ExpressionInvalidException("divisionByZero");
			return Math.Tan(tot);
		}
	}

	public class Cot: Function {
		public Cot(): base(new string[] { "cot", "cotangent", "cotang", "cotg" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double tot = total(arguments);
			if (Math.Sin(tot) == 0) throw new ExpressionInvalidException("divisionByZero");
			return 1 / Math.Tan(tot);
		}
	}

	public class ArcSin: Function {
		public ArcSin(): base(new string[] { "arcsin", "arcsine", "sin_1", "sine_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double tot = total(arguments);
			if (tot < -1 || tot > 1) throw new ExpressionInvalidException("invalidArcsinArg"); // TODO: Add the number.
			return engine.angleUnit.convertFromRadians(Math.Asin(tot));
		}
	}

	public class ArcCos: Function {
		public ArcCos(): base(new string[] { "arccos", "arccosine", "cos_1", "cosine_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double tot = total(arguments);
			if (tot < -1 || tot > 1) throw new ExpressionInvalidException("invalidArccosArg"); // TODO: Add the number.
			return engine.angleUnit.convertFromRadians(Math.Acos(tot));
		}
	}

	public class ArcTan: Function {
		public ArcTan(): base(new string[] { "arctan", "arctangent", "arctang", "arctg", "tan_1", "tangent_1", "tang_1", "tg_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => engine.angleUnit.convertFromRadians(Math.Atan(total(arguments)));
	}

	public class ArcCot: Function {
		public ArcCot(): base(new string[] { "arccot", "arccotangent", "arccotang", "arccotg", "cot_1", "cotangent_1", "cotang_1", "cotg_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double tot = total(arguments);
			if (tot == 0) return engine.angleUnit.convertFromDegrees(90);
			return engine.angleUnit.convertFromRadians(1 / Math.Atan(tot));
		}
	}

	public class Floor: Function {
		public Floor(): base(new string[] { "floor", "flr" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => Math.Floor(total(arguments));
	}

	public class Abs: Function {
		public Abs(): base(new string[] { "abs", "absolute" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => Math.Abs(total(arguments));
	}

	public class GCD: Function {
		public GCD(): base(new string[] { "gcd", "greatestCommonDivisor", "greatest_common_divisor" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count == 1) return Math.Floor(Math.Abs(arguments[0]));
			double res = Math.Floor(Math.Abs(arguments[0]));
			for (int i = 1; i < arguments.Count; i++) {
				double n = Math.Floor(Math.Abs(arguments[i]));
				while (n != 0) {
					double temp = n;
					n = mod(res, n);
					res = temp;
				}
			}
			return res;
		}
	}

	public class LCM: Function {
		public LCM(): base(new string[] { "lcm", "lowestCommonMultiplier", "lowest_common_multiplier" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count == 1) return Math.Floor(Math.Abs(arguments[0]));
			double res = Math.Floor(Math.Abs(arguments[0]));
			for (int i = 1; i < arguments.Count; i++) {
				double n = Math.Floor(Math.Abs(arguments[i]));
				double t = n;
				double t2 = res;
				while (t2 != 0) {
					double temp = t2;
					t2 = mod(n, t2);
					n = temp;
				}
				res = div(res * t, n);
			}
			return res;
		}
	}

	public class Fact: Function {
		public Fact(): base(new string[] { "fact", "factorial" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			double n = Math.Floor(total);
			if (n < 0) throw new ExpressionInvalidException("invalidFactorialArg");
			total = 1;
			for (double i = 1; i <= n; i += 1) total *= i;
			return total;
		}
	}

	public class Log: Function {
		public Log(): base(new string[] { "log", "logarithm", "logarid" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count == 1) {
				if (arguments[0] <= 0) throw new ExpressionInvalidException("invalidLogInput");
				return Math.Log10(arguments[0]);
			} else {
				if (arguments[0] <= 0) throw new ExpressionInvalidException("invalidLogBase");
				double total = 0;
				for (int i = 1; i < arguments.Count; i++) total += arguments[i];
				if (total <= 0) throw new ExpressionInvalidException("invalidLogInput");
				return Math.Log(total, arguments[0]);
			}
		}
	}

	public class Ln: Function {
		public Ln(): base(new string[] { "logn", "loge", "natural_algorithm", "natural_logarid" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double tot = total(arguments);
			if (tot <= 0) throw new ExpressionInvalidException("invalidLogInput");
			return Math.Log(tot);
		}
	}

	public class Permutation: Function {
		public Permutation(): base(new string[] { "p", "permutation", "permut" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count != 2) throw new ExpressionInvalidException("invalidPermutationNumArgs");
			double n = Math.Floor(arguments[0]);
			double k = Math.Floor(arguments[1]);
			if (n < 0 || k < 0) throw new ExpressionInvalidException("invalidPermutationNegativeArgs");
			if (k > n) return 0;
			k = n - k;
			double res = 1;
			while (k < n) res *= k++;
			return res;
		}
	}

	public class Combination: Function {
		public Combination(): base(new string[] { "c", "combination", "combin" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count != 2) throw new ExpressionInvalidException("invalidCombinationNumArgs");
			double n = Math.Floor(arguments[0]);
			double k = Math.Floor(arguments[1]);
			if (n < 0 || k < 0) throw new ExpressionInvalidException("invalidCombinationNegativeArgs");
			if (k > n) return 0;
			double i = n - k;
			double res = 1;
			while (i < n) res *= ++i;
			i = 0;
			while (i < k) res /= ++i;
			return res;
		}
	}

	public class Round: Function {
		public Round(): base(new string[] { "round", "rnd" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return Math.Round(total, MidpointRounding.AwayFromZero);
		}
	}

	public class DegToRad: Function {
		public DegToRad(): base(new string[] { "dtr", "degToRad", "deg_to_rad", "degreesToRadians", "degrees_to_radians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => degToRad(total(arguments));
	}

	public class RadToDeg: Function {
		public RadToDeg(): base(new string[] { "rtd", "radToDeg", "rad_to_deg", "radiansToDegrees", "radians_to_degrees" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => radToDeg(total(arguments));
	}

	public class DegToGrad: Function {
		public DegToGrad(): base(new string[] { "dtg", "degToGrad", "deg_to_grad", "degreesToGradians", "degrees_to_gradians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => degToGrad(total(arguments));
	}

	public class GradToDeg: Function {
		public GradToDeg(): base(new string[] { "gtd", "gradToDeg", "grad_to_deg", "gradiansToDegrees", "gradians_to_degrees" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => gradToDeg(total(arguments));
	}

	public class GradToRad: Function {
		public GradToRad(): base(new string[] { "gtr", "gradToRad", "grad_to_rad", "gradiansToRadians", "gradians_to_radians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => gradToRad(total(arguments));
	}

	public class RadToGrad: Function {
		public RadToGrad(): base(new string[] { "rtg", "radToGrad", "rad_to_grad", "radiansToGradians", "radians_to_gradians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => radToGrad(total(arguments));
	}

	public class Max: Function {
		public Max(): base(new string[] { "max", "maximum" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double max = 0;
			bool isFirst = true;
			foreach (double num in arguments)
				if (isFirst) {
					isFirst = false;
					max = num;
				} else if (num > max) max = num;
			return max;
		}
	}

	public class Min: Function {
		public Min(): base(new string[] { "min", "minimum" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double min = 0;
			bool isFirst = true;
			foreach (double num in arguments)
				if (isFirst) {
					isFirst = false;
					min = num;
				} else if (num < min) min = num;
			return min;
		}
	}

	public class Average: Function {
		public Average(): base(new string[] { "avg", "average" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => total(arguments) / arguments.Count; // No need to worry about division by zero, there can never be zero arguments.
	}

	public class RandomFunc: Function {
		public RandomFunc(): base(new string[] { "random", "rand" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			switch (arguments.Count) {
				case 1: return random.NextDouble() * arguments[0];
				case 2: return arguments[0] + (arguments[1] - arguments[0]) * random.NextDouble();
				default: throw new ExpressionInvalidException("invalidRandomNumArgs");
			}
		}
	}

	public class RandomInt: Function {
		const double aLittleBitMoreThanOne = 1 + 1E-10;
		public RandomInt(): base(new string[] { "randomInt", "randInt", "randomInteger", "random_integer" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double lower, higher;
			switch (arguments.Count) {
				case 1: lower = 0; higher = arguments[0]; break;
				case 2: lower = arguments[0]; higher = arguments[1]; break;
				default: throw new ExpressionInvalidException("invalidRandomNumArgs");
			}
			if (lower > higher) {
				double temp = lower;
				lower = higher;
				higher = temp;
			}
			lower = roundUp(lower);
			higher = roundDown(higher);
			if (lower > higher) throw new ExpressionInvalidException("invalidRandomNoIntegerBetween");
			return roundDown(lower + random.NextDouble() * (higher - lower + aLittleBitMoreThanOne));
		}
	}

	public class RandomInList: Function {
		public RandomInList(): base(new string[] { "randomInList", "random_in_list", "randInList" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			return arguments[random.Next(arguments.Count)];
		}
	}

	public class IsGreater: Function {
		public IsGreater(): base(new string[] { "isGreater" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count < 2) throw new ExpressionInvalidException("invalidComparisonNumArgs");
			for (int i = 1; i < arguments.Count; i++)
				if (arguments[i] >= arguments[i - 1]) return 0;
			return 1;
		}
	}

	public class IsSmaller: Function {
		public IsSmaller(): base(new string[] { "isSmaller" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count < 2) throw new ExpressionInvalidException("invalidComparisonNumArgs");
			for (int i = 1; i < arguments.Count; i++)
				if (arguments[i] <= arguments[i - 1]) return 0;
			return 1;
		}
	}

	public class IsEqual: Function {
		public IsEqual(): base(new string[] { "isEqual" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count < 2) throw new ExpressionInvalidException("invalidComparisonNumArgs");
			for (int i = 1; i < arguments.Count; i++)
				if (arguments[i] != arguments[0]) return 0;
			return 1;
		}
	}

	public class If: Function {
		public If(): base(new string[] { "if" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count > 3) throw new ExpressionInvalidException("invalidIfNumArgs");
			while (arguments.Count < 3) arguments.Add(0);
			return arguments[0] > 0 ? arguments[1]: arguments[2];
		}
	}

	public class And: Function {
		public And(): base(new string[] { "and" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			foreach (double num in arguments)
				if (num <= 0) return 0;
			return 1;
		}
	}

	public class Or: Function {
		public Or(): base(new string[] { "or" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			foreach (double num in arguments)
				if (num > 0) return 1;
			return 0;
		}
	}

	public class Not: Function {
		public Not(): base(new string[] { "not" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count != 1) throw new ExpressionInvalidException("invalidNotNumArgs");
			return arguments[0] > 0 ? 0: 1;
		}
	}

	public class AngleToDegrees: Function {
		public AngleToDegrees(): base(new[] { "angle to degrees", "angle_to_degrees", "to degrees", "to_degrees", "to deg" }) { }
		public override double calculate(List<Double> arguments, CalculatorEngine engine) => engine.angleUnit.convertToDegrees(total(arguments));
	}

	public class AngleToRadians: Function {
		public AngleToRadians(): base(new[] { "angle to radians", "angle_to_radians", "to radians", "to_radians", "to rad" }) { }
		public override double calculate(List<Double> arguments, CalculatorEngine engine) => engine.angleUnit.convertToRadians(total(arguments));
	}

	public class AngleToGradians: Function {
		public AngleToGradians(): base(new[] { "angle to gradians", "angle_to_gradians", "to gradians", "to_gradians", "to grad" }) { }
		public override double calculate(List<Double> arguments, CalculatorEngine engine) => engine.angleUnit.convertToGradians(total(arguments));
	}

	public class AngleFromDegrees: Function {
		public AngleFromDegrees(): base(new[] { "angle from degrees", "angle_from_degrees", "from degrees", "from_degrees", "from deg" }) { }
		public override double calculate(List<Double> arguments, CalculatorEngine engine) => engine.angleUnit.convertFromDegrees(total(arguments));
	}

	public class AngleFromRadians: Function {
		public AngleFromRadians(): base(new[] { "angle from radians", "angle_from_radians", "from radians", "from_radians", "from rad" }) { }
		public override double calculate(List<Double> arguments, CalculatorEngine engine) => engine.angleUnit.convertFromRadians(total(arguments));
	}

	public class AngleFromGradians: Function {
		public AngleFromGradians(): base(new[] { "angle from gradians", "angle_from_gradians", "from gradians", "from_gradians", "from grad" }) { }
		public override double calculate(List<Double> arguments, CalculatorEngine engine) => engine.angleUnit.convertFromGradians(total(arguments));
	}

	public class Date: Function {
		public Date(): base(new[] { "date" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count > 6) throw new ExpressionInvalidException("invalidDateNumOfArgs");
			while (arguments.Count < 3) arguments.Add(1);
			while (arguments.Count < 6) arguments.Add(0);
			for (int i = 0; i < 2; i++) arguments[i] = Math.Round(arguments[i], MidpointRounding.AwayFromZero);
			if (arguments[1] < 0.5 || arguments[1] > 12.49999 /* Accounting for rounding errors */) throw new ExpressionInvalidException("invalidDateMonthOutOfRange");
			arguments[2] = arguments[2] - 1 + arguments[3] / 24 + arguments[4] / 1440 + arguments[5] / 86400;
			if (arguments[2] < 0 || arguments[2] >= getMonthDays(arguments[0], (int)arguments[1])) throw new ExpressionInvalidException("invalidDateDayOutOfRange");
			return (arguments[0] - 1) * 365 + div(arguments[0], 4) - div(arguments[0], 100) + div(arguments[0], 400) - (isLeapYear(arguments[0]) && arguments[1] < 2.5 ? 1: 0) + monthPos[(int)arguments[1]] + arguments[2];
		}
	}

	public class Year: Function {
		public Year(): base(new[] { "year", "yr" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => getYearAndDayOfYearFromIndex(total(arguments))[0];
	}

	public class DayOfYear: Function {
		public DayOfYear(): base(new[] { "day of year", "day_of_year" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => getYearAndDayOfYearFromIndex(total(arguments))[1]+1;
	}

	public class Month: Function {
		public Month(): base(new[] { "month", "mth" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => getMonthAndDayOfMonthFromIndex(total(arguments))[0];
	}

	public class Day: Function {
		public Day(): base(new[] { "day" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => Math.Floor(getMonthAndDayOfMonthFromIndex(total(arguments))[1]);
	}

	public class DecimalDay: Function {
		public DecimalDay() : base(new[] { "decimal day", "decimal_day" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => getMonthAndDayOfMonthFromIndex(total(arguments))[1];
	}

	public class Hour: Function {
		public Hour(): base(new[] { "hour", "hr" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) => getHourFromIndex(total(arguments));
	}

	public class Minute: Function {
		public Minute(): base(new[] { "minute", "min" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double index = total(arguments);
			return Math.Truncate(1440 * (mod(index, 1) - getHourFromIndex(index) / 24));
		}
	}

	public class Second: Function {
		public Second(): base(new[] { "second", "sec" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double dec = mod(total(arguments), 1);
			return 86400 * (dec - Math.Truncate(1440 * dec) / 1440);
		}
	}

	public class DayOfWeekMondayFirst: Function {
		public DayOfWeekMondayFirst(): base(new[] { "day of week Monday first", "day_of_week_Monday_first" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double date = Math.Floor(total(arguments));
			return date - Math.Floor(date / 7) * 7 + 1;
		}
	}

	public class DayOfWeekSundayFirst: Function {
		public DayOfWeekSundayFirst(): base(new[] { "day of week Sunday first", "day_of_week_Sunday_first" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double date = Math.Floor(total(arguments)) + 1;
			return date - Math.Floor(date / 7) * 7 + 1;
		}
	}

	public class Time: Function {
		public Time(): base(new[] { "time" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count > 4) throw new ExpressionInvalidException("invalidTimeNumArgs");
			while (arguments.Count < 4) arguments.Add(0);
			return arguments[0] + arguments[1] / 24 + arguments[2] / 1440 + arguments[3] / 86400;
		}
	}
}