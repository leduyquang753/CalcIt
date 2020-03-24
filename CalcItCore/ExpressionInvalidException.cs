using System;

namespace CalcItCore {
	public class ExpressionInvalidException: Exception {
		public int position { get; }

		public ExpressionInvalidException(String key, int position = -1, string[] messageArguments = null):
			base(key) {
			this.position = position;
		}

		public ExpressionInvalidException(ExpressionInvalidException e, int position = -1) :
			base(e.Message) {
			this.position = position;
		}
	}
}
