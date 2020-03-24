using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CalcItUWP {
	/// <summary>
	/// A text box with ability to remember entered expressions.
	/// </summary>
	public partial class InputBox: TextBox {
		public List<string> history = new List<string>();
		private int maxHistSize = 64;
		public int maximumHistorySize {
			get { return maxHistSize; }
			set {
				maxHistSize = value;
				if (history.Count > maxHistSize) history.RemoveRange(maxHistSize, history.Count - maxHistSize);
			}
		}
		public string currentExpression = "";
		public int historyPointer = -1;

		public InputBox(): base() {
			maximumHistorySize = 64;
		}

		protected override void OnKeyDown(KeyRoutedEventArgs args) {
			switch (args.Key) {
				case VirtualKey.Up:
					args.Handled = true;
					if (historyPointer >= history.Count - 1) {
						SelectionStart = 0;
						historyPointer = history.Count - 1; // Just make sure the pointer is inside the bounds.
						break;
					}
					if (historyPointer == -1) currentExpression = Text;
					Text = history[++historyPointer];
					SelectionStart = Text.Length;
					break;
				case VirtualKey.Down:
					args.Handled = true;
					if (historyPointer <= -1) {
						SelectionStart = Text.Length;
						historyPointer = -1; // Just make sure the pointer is inside the bounds.
						break;
					}
					Text = --historyPointer == -1 ? currentExpression : history[historyPointer];
					SelectionStart = Text.Length;
					break;
			}
			base.OnKeyDown(args);
		}
	}
}
