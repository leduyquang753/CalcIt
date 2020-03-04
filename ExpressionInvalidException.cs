using System;

namespace CalcItUWP {
	class ExpressionInvalidException: Exception {
		public int position { get; }

		public ExpressionInvalidException(String key, int position = -1, string[] messageArguments = null):
			base(messageArguments == null ? Utils.getString("error/" + key) : String.Format(Utils.getString("error/" + key), messageArguments)) {
			this.position = position;
		}
	}
}
