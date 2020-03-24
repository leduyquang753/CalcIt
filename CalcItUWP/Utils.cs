using CalcItCore;
using System;
using Windows.ApplicationModel.Resources;

namespace CalcItUWP {
	class Utils {
		public static ResourceLoader resourceLoader = null;

		public static string getString(string key) {
			if (resourceLoader == null) throw new InvalidOperationException("Resources have not been loaded.");
			return resourceLoader.GetString(key);
		}

		private static string[] getVariableErrorStringMap = new[] { "emptyVariableName", "invalidVariableName", "variableNotSet" };

		public static string getVariableString(CalculatorEngine engine, string name) {
			string numOutOfRange = Utils.getString("getVarString/numberOutOfRange");
			try {
				return CoreUtils.formatNumber(engine.getVariable(name), engine) ?? numOutOfRange;
			} catch (GetVariableException e) {
				return Utils.getString("getVarString/" + getVariableErrorStringMap[(int)e.type]);
			}
		}

		public static string formatError(string key, object[] arguments = null) {
			return arguments == null ? getString("error/" + key) : String.Format(getString("error/" + key), arguments);
		}
	}
}
