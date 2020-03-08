using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

using CalcItUWP.Functions;
using CalcItUWP.Operands;
using static CalcItUWP.AngleUnits;

namespace CalcItUWP {
	public class CalculatorEngine {
		private static DotlessMultiplication dotlessMulOp = new DotlessMultiplication();
		private static Dictionary<string, string> braceMap = new Dictionary<string, string>();
		private const int positiveInfinity = 99999;
		private const int negativeInfinity = -99999;

		static CalculatorEngine() {
			braceMap.Add("(", ")");
			braceMap.Add("{", "}");
			braceMap.Add("[", "]");
			braceMap.Add("<", ">");
		}

		private Dictionary<string, Operand> operandMap { get; } = new Dictionary<string, Operand>();
		private Dictionary<string, double> variableMap { get; } = new Dictionary<string, double>();
		private Dictionary<string, Function> functionMap { get; } = new Dictionary<string, Function>();

		public bool
			decimalDot = false,
			enforceDecimalSeparator = false,
			thousandDot = false,
			mulAsterisk = false,
			enforceMulDiv = false,
			zeroUndefinedVars = false;

		public AngleUnit angleUnit = AngleUnits.DEGREE;

		private double
			ans = 0,
			preAns = 0;

		public CalculatorEngine() {
			// Register every operand.
			foreach (Operand operand in new Operand[] {
				new Plus(),
				new Minus(),
				new Multiply(),
				new Divide(),
				new Exponentiation(),
				new Root(),
				new OpeningBrace(),
				new ClosingBrace()
			}) registerOperand(operand);

			// Register every function.
			foreach (Function function in new Function[] {
				new Sum(),
				new Sin(),
				new Cos(),
				new Tan(),
				new Cot(),
				new ArcSin(),
				new ArcCos(),
				new ArcTan(),
				new ArcCot(),
				new Floor(),
				new Abs(),
				new GCD(),
				new LCM(),
				new Fact(),
				new Log(),
				new Ln(),
				new Permutation(),
				new Combination(),
				new Round(),
				new DegToRad(),
				new RadToDeg(),
				new DegToGrad(),
				new GradToDeg(),
				new GradToRad(),
				new RadToGrad(),
				new Max(),
				new Min(),
				new Average(),
				new RandomFunc(),
				new RandomInt(),
				new RandomInList(),
				new IsGreater(),
				new IsSmaller(),
				new IsEqual(),
				new If(),
				new And(),
				new Or(),
				new Not(),
				new AngleToDegrees(),
				new AngleToRadians(),
				new AngleToGradians(),
				new AngleFromDegrees(),
				new AngleFromRadians(),
				new AngleFromGradians(),
				new Date(),
				new Year(),
				new DayOfYear(),
				new Month(),
				new Day(),
				new DecimalDay(),
				new Hour(),
				new Minute(),
				new Second(),
				new GetDayOfWeekMondayFirst(),
				new GetDayOfWeekSundayFirst()
			}) registerFunction(function);
		}

		/// <summary>
		/// Registers an operand to the engine. If there is a registered operand with some characters overlapping the operand being added, the one being will override.
		/// </summary>
		/// <param name="op">The operand to register.</param>
		public void registerOperand(Operand op) {
			foreach (string key in op.characters) operandMap[key] = op;
		}

		/// <summary>
		/// Registers a function to the engine. If there is a registered function with some names overlapping the operand being added, the one being will override.
		/// </summary>
		/// <param name="op">The function to register.</param>
		public void registerFunction(Function func) {
			foreach (string key in func.names) functionMap[lowercaseAndRemoveWhitespace(key)] = func;
		}

		private bool isDigit(char c) => c >= '0' && c <= '9';

		private bool isChar(char c) => Char.IsLetter(c) || c == '_';

		private bool areBracesMatch(string opening, string closing) => closing == braceMap.GetValueOrDefault(opening, null);

		private void performBacktrackCalculation(Stack<double> NS, Stack<double> TNS, Stack<Operand> OS, Stack<Operand> TOS, bool shouldCalculateAll = false) {
			if (OS.Count == 0) return;
			Operand currentOperand = OS.Pop();
			double currentNumber = NS.Pop();
			int lastPriority = positiveInfinity;
			while (shouldCalculateAll || !(currentOperand is OpeningBrace)) {
				if (shouldCalculateAll && currentOperand is OpeningBrace) {
					while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
					lastPriority = positiveInfinity;
					if (OS.Count != 0) currentOperand = OS.Pop(); else {
						while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
						NS.Push(currentNumber);
						return;
					}
				}
				if (currentOperand.priority != lastPriority)
					while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
				if (currentOperand.reversed) currentNumber = currentOperand.calculate(NS.Pop(), currentNumber, this); else {
					TNS.Push(currentNumber);
					TOS.Push(currentOperand);
					currentNumber = NS.Pop();
				}
				lastPriority = currentOperand.priority;
				if (OS.Count != 0) currentOperand = OS.Pop(); else {
					while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
					NS.Push(currentNumber);
					return;
				}
			}
			while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
			NS.Push(currentNumber);
			OS.Push(currentOperand);
		}

		private void performBacktrackSameLevelCalculation(Stack<double> NS, Stack<double> TNS, Stack<Operand> OS, Stack<Operand> TOS) {
			if (OS.Count == 0) return;
			Operand currentOperand = OS.Pop();
			double currentNumber = NS.Pop();
			int lastPriority = currentOperand.priority;
			while (!(currentOperand is OpeningBrace)) {
				if (currentOperand.priority != lastPriority) {
					while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
					NS.Push(currentNumber);
					OS.Push(currentOperand);
					return;
				}
				if (currentOperand.reversed) currentNumber = currentOperand.calculate(NS.Pop(), currentNumber, this); else {
					TNS.Push(currentNumber);
					TOS.Push(currentOperand);
					currentNumber = NS.Pop();
				}
				lastPriority = currentOperand.priority;
				if (OS.Count != 0) currentOperand = OS.Pop(); else {
					while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
					NS.Push(currentNumber);
					return;
				}
			}
			while (TOS.Count != 0) currentNumber = TOS.Pop().calculate(currentNumber, TNS.Pop(), this);
			NS.Push(currentNumber);
			OS.Push(currentOperand);
		}

		private double processNumberToken(ref bool negativity, ref bool hadNegation, ref bool isVariable, ref bool hadComma, ref string stringIn, int position, Stack<double> NS, Stack<Operand> OS) {
			bool percent = false;
			if (stringIn[stringIn.Length-1] == '%') {
				percent = true;
				stringIn = stringIn.Substring(0, stringIn.Length-1);
			}
			double result;
			if (isVariable) result = getVariableInternal(stringIn, position); else {
				stringIn = stringIn.Replace(",", ".");
				result = double.Parse(stringIn);
			}
			if (percent) result /= 100;
			if (negativity) {
				NS.Push(-1);
				OS.Push(dotlessMulOp);
			}
			negativity = false;
			hadNegation = false;
			hadComma = false;
			stringIn = "";
			return result;
		}

		private bool isDecimalSeparator(char c) => decimalDot ? enforceDecimalSeparator ? c == '.' : c == '.' || c == ',' : c == ',';

		private double performCalculation(string input) {
			try {
				Stack<double>
					NS = new Stack<double>(),
					TNS = new Stack<double>();
				Stack<Operand>
					OS = new Stack<Operand>(),
					TOS = new Stack<Operand>();
				Stack<Bracelet> BS = new Stack<Bracelet>();
				bool
					status = false, // true: previous was number/closing brace; false: previous was operand/opening brace.
					negativity = false,
					hadNegation = false,
					isVariable = false,
					hadClosingBrace = false,
					hadComma = false;
				string currentToken = "";
				char thousandSeparator = decimalDot ? '.' : ',';
				Operand currentOperand;
				Function currentFunction;
				Bracelet currentBracelet;
				for (int i = 0; i < input.Length; i++) {
					char c = input[i];
					if (thousandDot && c == thousandSeparator) {
						if (status && !isVariable) continue; else throw new ExpressionInvalidException("unexpectedThousandSeparator", i+1);
					} else if (c == '-' && !status) {
						negativity = !negativity;
						hadNegation = true;
					} else if (c == '%') {
						if (!status || currentToken[currentToken.Length - 1] == '%') throw new ExpressionInvalidException("unexpectedPercent", i+1); else currentToken += c;
					} else if (c == ';') {
						if (BS.Count != 0) {
							if (status) {
								if (currentToken.Length != 0) NS.Push(processNumberToken(ref negativity, ref hadNegation, ref isVariable, ref hadComma, ref currentToken, i, NS, OS));
								performBacktrackCalculation(NS, TNS, OS, TOS);
								BS.Peek().addArgument(NS.Pop());
								status = false;
								hadClosingBrace = false;
							} else if (OS.Peek() is OpeningBrace) {
								BS.Peek().addArgument(0);
								status = false;
								hadClosingBrace = false;
							} else throw new ExpressionInvalidException("unexpectedSemicolon", i+1);
						} else throw new ExpressionInvalidException("unexpectedSemicolon", i+1);
					} else if (isDecimalSeparator(c)) {
						if (currentToken.Length == 0) {
							if (hadClosingBrace) {
								while (OS.Count != 0 && dotlessMulOp.priority < OS.Peek().priority) performBacktrackSameLevelCalculation(NS, TNS, OS, TOS);
								OS.Push(dotlessMulOp);
								hadClosingBrace = false;
							}
							currentToken = "0,";
							status = true;
							isVariable = false;
							hadComma = true;
						} else if (status) {
							if (isVariable || hadComma) throw new ExpressionInvalidException("unexpectedDecimalSeparator", i+1);
							currentToken += c;
							hadComma = true;
						} else { };
					} else if (isDigit(c)) {
						if (currentToken.Length == 0) {
							if (hadClosingBrace) {
								while (OS.Count != 0 && dotlessMulOp.priority < OS.Peek().priority) performBacktrackSameLevelCalculation(NS, TNS, OS, TOS);
								OS.Push(dotlessMulOp);
								hadClosingBrace = false;
							}
							currentToken = c.ToString();
							status = true;
							isVariable = false;
						} else if (status) {
							if (isVariable) currentToken += c;
							else if (currentToken[currentToken.Length - 1] == '%') throw new ExpressionInvalidException("unexpectedDigit", i+1);
							else currentToken += c;
						} else {
							currentToken = c.ToString();
							status = true;
							isVariable = false;
						}
					} else if (isChar(c)) {
						if (hadClosingBrace || currentToken.Length != 0 && !isVariable) {
							if (currentToken.Length != 0 && !isVariable) NS.Push(processNumberToken(ref negativity, ref hadNegation, ref isVariable, ref hadComma, ref currentToken, i, NS, OS));
							while (OS.Count != 0 && dotlessMulOp.priority < OS.Peek().priority) performBacktrackSameLevelCalculation(NS, TNS, OS, TOS);
							OS.Push(dotlessMulOp);
							hadClosingBrace = false;
						}
						currentToken += c;
						isVariable = true;
						status = true;
						hadClosingBrace = false;
					} else if ((currentOperand = operandMap.GetValueOrDefault(c.ToString(), null)) == null) throw new ExpressionInvalidException("unknownSymbol", i+1);
					else {
						if (currentOperand is OpeningBrace) {
							if (hadClosingBrace || currentToken.Length != 0 && !isVariable) {
								if (currentToken.Length != 0 && !isVariable) NS.Push(processNumberToken(ref negativity, ref hadNegation, ref isVariable, ref hadComma, ref currentToken, i, NS, OS));
								while (OS.Count != 0 && dotlessMulOp.priority < OS.Peek().priority) performBacktrackSameLevelCalculation(NS, TNS, OS, TOS);
								OS.Push(dotlessMulOp);
								hadClosingBrace = false;
							}
							if ((currentFunction = functionMap.GetValueOrDefault(currentToken, null)) == null) throw new ExpressionInvalidException("unknownFunction", i, new[] { currentToken });
							OS.Push(currentOperand);
							BS.Push(new Bracelet(c.ToString(), currentFunction, this));
							status = false;
							currentToken = "";
						} else if (currentOperand is ClosingBrace) {
							if (status) if (BS.Count == 0) throw new ExpressionInvalidException("unexpectedClosingBrace", i + 1);
								else if (areBracesMatch(BS.Peek().opening, c.ToString())) {
									if (currentToken.Length != 0) NS.Push(processNumberToken(ref negativity, ref hadNegation, ref isVariable, ref hadComma, ref currentToken, i, NS, OS));
									performBacktrackCalculation(NS, TNS, OS, TOS);
									OS.Pop();
									(currentBracelet = BS.Pop()).addArgument(NS.Pop());
									NS.Push(currentBracelet.getResult());
									status = true;
									hadClosingBrace = true;
								} else throw new ExpressionInvalidException("unmatchingBraces", i + 1);
							else if (OS.Count == 0) {
								NS.Push(0);
								status = true;
								hadClosingBrace = true;
							} else if (OS.Peek() is OpeningBrace) {
								if (BS.Count != 0 && !areBracesMatch(BS.Peek().opening, c.ToString())) throw new ExpressionInvalidException("unmatchingBraces", i + 1);
								OS.Pop();
								(currentBracelet = BS.Pop()).addArgument(0);
								NS.Push(currentBracelet.getResult());
								status = true;
								hadClosingBrace = true;
							} else throw new ExpressionInvalidException("unexpectedClosingBrace", i + 1);
						} else {
							if (status) {
								if (enforceMulDiv) switch (c) {
										case '.':
										case ':':
											if (mulAsterisk) throw new ExpressionInvalidException("unknownSymbol", i+1);
											break;
										case '*':
										case '/':
											if (!mulAsterisk) throw new ExpressionInvalidException("unknownSymbol", i+1);
											break;
									}
								if (currentToken.Length != 0) NS.Push(processNumberToken(ref negativity, ref hadNegation, ref isVariable, ref hadComma, ref currentToken, i, NS, OS));
								else if (hadNegation) throw new ExpressionInvalidException("unexpectedOperand", i);
								while (OS.Count != 0 && currentOperand.priority < OS.Peek().priority) performBacktrackSameLevelCalculation(NS, TNS, OS, TOS);
								OS.Push(currentOperand);
								status = false;
								hadClosingBrace = false;
							} else if (c == '+') hadNegation = true; else throw new ExpressionInvalidException("unexpectedOperand", i+1);
						}
					}
				}
				if (status) {
					if (currentToken.Length != 0) NS.Push(processNumberToken(ref negativity, ref hadNegation, ref isVariable, ref hadComma, ref currentToken, input.Length - 1, NS, OS));
					else if (hadNegation) throw new ExpressionInvalidException("trailingSign");
					else { };
				} else throw new ExpressionInvalidException("unexpectedEnd");
				while (BS.Count != 0) {
					performBacktrackCalculation(NS, TNS, OS, TOS);
					currentBracelet = BS.Pop();
					currentBracelet.addArgument(NS.Pop());
					NS.Push(currentBracelet.getResult());
				}
				performBacktrackCalculation(NS, TNS, OS, TOS, true);
				return NS.Pop();
			} catch (OverflowException) {
				throw new ExpressionInvalidException("numberOutOfRange");
			}
		}

		private string lowercaseAndRemoveWhitespace(string stringIn) => stringIn.Replace(" ", "").Replace("\t", "").Replace("\n", "").ToLower();

		public double calculate(string expression) {
			string trimmedExpression = lowercaseAndRemoveWhitespace(expression);
			List<string> toAssign = new List<string>();
			int ps;
			int position = 0;
			while (true) {
				switch (ps = trimmedExpression.IndexOf('=')) {
					case -1: goto exit;
					case 0: throw new ExpressionInvalidException("unexpectedEqual", Utils.getIndexWithWhitespace(expression, position+1));
					default:
						string s = trimmedExpression.Substring(0, ps);
						if (s == "ans" || s == "preAns") throw new ExpressionInvalidException("reservedVariable", Utils.getIndexWithWhitespace(expression, position + ps));
						if (isDigit(s[0])) throw new ExpressionInvalidException("invalidVariable", Utils.getIndexWithWhitespace(expression, position + ps), new[] { s }); 
						foreach (char c in s) if (!isChar(c) && !isDigit(c)) throw new ExpressionInvalidException("nonAlphanumericVariableName", Utils.getIndexWithWhitespace(expression, position + ps), new[] { s });
						toAssign.Add(s);
						break;
				}
				trimmedExpression = trimmedExpression.Substring(ps + 1);
				position += ps + 1;
			}
		exit:
			if (trimmedExpression.Length == 0) throw new ExpressionInvalidException("nothingToCalculate", Utils.getIndexWithWhitespace(expression, position));
			if (trimmedExpression == "!") {
				foreach (string s in toAssign) variableMap.Remove(s);
				preAns = ans;
				ans = 0;
				return 0;
			}
			double oldAns = ans;
			try {
				ans = performCalculation(trimmedExpression);
			} catch (ExpressionInvalidException e) { // Handle and rethrow the exception to properly position the error in the expression with whitespace.
				throw new ExpressionInvalidException(e, position + Utils.getIndexWithWhitespace(expression, position + e.position));
			}
			foreach (string s in toAssign) variableMap[s] = ans;
			preAns = oldAns;
			return ans;
		}

		public string getVariableString(string name) {
			name = lowercaseAndRemoveWhitespace(name);
			if (name.Length == 0) return Utils.getString("getVarString/emptyVariableName");
			if (isDigit(name[0])) return Utils.getString("getVarString/invalidVariableName");
			foreach (char c in name) if (!isDigit(c) && !isChar(c)) return Utils.getString("getVarString/invalidVariableName");
			switch (name) {
				case "ans": return Utils.formatNumber(ans, this);
				case "preans": return Utils.formatNumber(preAns, this);
			}
			double p;
			return variableMap.TryGetValue(name, out p) ? Utils.formatNumber(p, this) : zeroUndefinedVars ? "0" : Utils.getString("getVarString/variableNotSet");
		}

		private double getVariableInternal(string var, int position) {
			string name = lowercaseAndRemoveWhitespace(var);
			switch (name) {
				case "ans": return ans;
				case "preans": return preAns;
			}
			double p;
			if (variableMap.TryGetValue(name, out p)) return p;
			else if (zeroUndefinedVars) return 0;
			else throw new ExpressionInvalidException("variableNotSet", position, new[] { var });
		}
	}

	class Bracelet {
		public string opening;
		public Function functionAssigned;
		public List<double> arguments { get; } = new List<double>();
		private CalculatorEngine engine { get; }

		public Bracelet(string openingIn, Function functionIn, CalculatorEngine engineIn) {
			opening = openingIn;
			functionAssigned = functionIn;
			engine = engineIn;
		}

		public void addArgument(double argumentIn) => arguments.Add(argumentIn);

		public double getResult() => functionAssigned.calculate(arguments, engine);
	}
}
