using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CalcItUWP.Utils;

namespace CalcItUWP.Operands {
	public abstract class Operand {
		public string[] characters { get; }
		public bool reversed { get; }
		public int priority { get; }

		public Operand(string[] characters, int priority, bool reversed = false) {
			this.characters = characters;
			this.reversed = reversed;
			this.priority = priority;
		}

		public abstract double calculate(double val1, double val2, CalculatorEngine engine);
	}

	public class Plus: Operand {
		public Plus(): base(new string[] { "+" }, 1) {}
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			return val1 + val2;
		}
	}

	public class Minus: Operand {
		public Minus(): base(new string[] { "-", "–" }, 1) {}
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			return val1 - val2;
		}
	}

	public class Multiply: Operand {
		public Multiply(): base(new string[] { ".", "*", "·", "×" }, 2) {}
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			return val1 * val2;
		}
	}

	public class Divide: Operand {
		public Divide(): base(new string[] { ":", "/", "÷" }, 2) { }
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			if (val2 == 0) throw new ExpressionInvalidException("divisionByZero");
			return val1 / val2;
		}
	}

	public class Exponentiation: Operand {
		public Exponentiation(): base(new string[] { "^" }, 4, true) { }
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			return Utils.power(val1, val2, engine);
		}
	}

	public class Root: Operand {
		public Root(): base(new string[] { "#" }, 4) { }
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			if (val1 == 0) throw new ExpressionInvalidException("level0Root");
			return Utils.power(val2, 1 / val1, engine);
		}
	}

	public class OpeningBrace: Operand {
		public OpeningBrace() : base(new string[] { "(", "[", "{", "<" }, -2) { }
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			throw new ExpressionInvalidException("braceInvolved");
		}
	}

	public class ClosingBrace: Operand {
		public ClosingBrace() : base(new string[] { ")", "]", "}", ">" }, -2) { }
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			throw new ExpressionInvalidException("braceInvolved");
		}
	}

	public class DotlessMultiplication: Operand {
		public DotlessMultiplication() : base(new string[] { "." }, 3) { }
		public override double calculate(double val1, double val2, CalculatorEngine engine) {
			return val1 * val2;
		}
	}
}
