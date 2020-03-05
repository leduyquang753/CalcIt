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
	}

	public class Sum: Function {
		public Sum(): base(new string[] { "", "sum", "total" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return total;
		}
	}

	public class Sin: Function {
		public Sin(): base(new string[] { "sin", "sine" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return Math.Sin(engine.angleUnit.convertToRadians(total));
		}
	}

	public class Cos: Function {
		public Cos(): base(new string[] { "cos", "cosine" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return Math.Cos(engine.angleUnit.convertToRadians(total));
		}
	}

	public class Tan: Function {
		public Tan(): base(new string[] { "tan", "tangent", "tang", "tg" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			total = engine.angleUnit.convertToRadians(total);
			if (Math.Cos(total) == 0) throw new ExpressionInvalidException("divisionByZero");
			return Math.Tan(total);
		}
	}

	public class Cot: Function {
		public Cot(): base(new string[] { "cot", "cotangent", "cotang", "cotg" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			total = engine.angleUnit.convertToRadians(total);
			if (Math.Sin(total) == 0) throw new ExpressionInvalidException("divisionByZero");
			return 1 / Math.Tan(total);
		}
	}

	public class ArcSin: Function {
		public ArcSin(): base(new string[] { "arcsin", "arcsine", "sin_1", "sine_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			if (total < -1 || total > 1) throw new ExpressionInvalidException("invalidArcsinArg"); // TODO: Add the number.
			return engine.angleUnit.convertFromRadians(Math.Asin(total));
		}
	}

	public class ArcCos: Function {
		public ArcCos(): base(new string[] { "arccos", "arccosine", "cos_1", "cosine_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			if (total < -1 || total > 1) throw new ExpressionInvalidException("invalidArccosArg"); // TODO: Add the number.
			return engine.angleUnit.convertFromRadians(Math.Acos(total));
		}
	}

	public class ArcTan: Function {
		public ArcTan(): base(new string[] { "arctan", "arctangent", "arctang", "arctg", "tan_1", "tangent_1", "tang_1", "tg_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return engine.angleUnit.convertFromRadians(Math.Atan(total));
		}
	}

	public class ArcCot: Function {
		public ArcCot(): base(new string[] { "arccot", "arccotangent", "arccotang", "arccotg", "cot_1", "cotangent_1", "cotang_1", "cotg_1" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			if (total == 0) return engine.angleUnit.convertFromRadians(Math.PI/2);
			return engine.angleUnit.convertFromRadians(1 / Math.Atan(total));
		}
	}

	public class Floor: Function {
		public Floor(): base(new string[] { "floor", "flr" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return Math.Floor(total);
		}
	}

	public class Abs: Function {
		public Abs(): base(new string[] { "abs", "absolute" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return Math.Abs(total);
		}
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
			double total = 0;
			foreach (double argument in arguments) total += argument;
			if (total <= 0) throw new ExpressionInvalidException("invalidLogInput");
			return Math.Log(total);
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
		public DegToRad() : base(new string[] { "dtr", "degToRad", "deg_to_rad", "degreesToRadians", "degrees_to_radians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return degToRad(total);
		}
	}

	public class RadToDeg: Function {
		public RadToDeg(): base(new string[] { "rtd", "radToDeg", "rad_to_deg", "radiansToDegrees", "radians_to_degrees" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return radToDeg(total);
		}
	}

	public class DegToGrad: Function {
		public DegToGrad(): base(new string[] { "dtg", "degToGrad", "deg_to_grad", "degreesToGradians", "degrees_to_gradians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return degToGrad(total);
		}
	}

	public class GradToDeg: Function {
		public GradToDeg(): base(new string[] { "gtd", "gradToDeg", "grad_to_deg", "gradiansToDegrees", "gradians_to_degrees" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return gradToDeg(total);
		}
	}

	public class GradToRad: Function {
		public GradToRad(): base(new string[] { "gtr", "gradToRad", "grad_to_rad", "gradiansToRadians", "gradians_to_radians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return gradToRad(total);
		}
	}

	public class RadToGrad: Function {
		public RadToGrad(): base(new string[] { "rtg", "radToGrad", "rad_to_grad", "radiansToGradians", "radians_to_gradians" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return radToGrad(total);
		}
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
		public Min() : base(new string[] { "min", "minimum" }) { }
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
		public Average() : base(new string[] { "avg", "average" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			double total = 0;
			foreach (double argument in arguments) total += argument;
			return total / arguments.Count;
		}
	}

	public class RandomFunc: Function {
		public RandomFunc(): base(new string[] { "random", "rand" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			switch (arguments.Count) {
				case 1: return random.NextDouble()*arguments[0];
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
		public RandomInList() : base(new string[] { "randomInList", "random_in_list", "randInList" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			return arguments[random.Next(arguments.Count)];
		}
	}

	public class IsGreater: Function {
		public IsGreater(): base(new string[] { "isGreater" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count < 2) throw new ExpressionInvalidException("invalidComparisonNumArgs");
			for (int i = 1; i < arguments.Count; i++)
				if (arguments[i] <= arguments[i - 1]) return 0;
			return 1;
		}
	}

	public class IsSmaller: Function {
		public IsSmaller(): base(new string[] { "isSmaller" }) { }
		public override double calculate(List<double> arguments, CalculatorEngine engine) {
			if (arguments.Count < 2) throw new ExpressionInvalidException("invalidComparisonNumArgs");
			for (int i = 1; i < arguments.Count; i++)
				if (arguments[i] >= arguments[i - 1]) return 0;
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
			return arguments[0] > 0 ? arguments[1] : arguments[2];
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
			return arguments[0] > 0 ? 0 : 1;
		}
	}
}