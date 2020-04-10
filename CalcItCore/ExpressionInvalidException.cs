using System;

namespace CalcItCore {
	public class ExpressionInvalidException: Exception {
		public int position { get; }
		public object[] messageArguments { get; }

		public ExpressionInvalidException(String key, int position = -1, object[] messageArguments = null):
			base(key) {
			this.position = position;
			this.messageArguments = messageArguments;
		}
	}
}
