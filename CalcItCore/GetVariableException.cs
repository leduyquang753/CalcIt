using System;
using System.Collections.Generic;
using System.Text;

namespace CalcItCore {
	public class GetVariableException: Exception {
		public enum Type {
			EMPTY_NAME,
			INVALID_NAME,
			NOT_SET
		}

		public Type type { get; }

		public GetVariableException(Type type) {
			this.type = type;
		}
	}
}
