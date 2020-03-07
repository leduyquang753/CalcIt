using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CalcItUWP {
	public sealed partial class VariableView: UserControl {
		public int index;
		MainPage parent;

		// Throwaway constructor. Only here to prevent errors.
		public VariableView() {
			InitializeComponent();
			index = -1;
			parent = null;
		}

		public VariableView(int index = -1, MainPage parent = null) {
			InitializeComponent();
			this.index = index;
			this.parent = parent;
			Height = 42;
			update();
		}

		private void onNameChanged(object sender, TextChangedEventArgs e) {
			update();
		}

		private void onRemovalRequested(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
			if (parent == null) return;
			parent.onVariableRemovalRequested(index);
		}

		public void update() {
			if (parent == null) return;
			value.Text = parent.engine.getVariableString(name.Text) ?? Utils.getString("getVariableString/numberOutOfRange");
		}
	}
}
