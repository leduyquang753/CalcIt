using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CalcItUWP {
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private CalculatorEngine engine = new CalculatorEngine();

		public static TextBox outputBoxInstance;

		public MainPage()
		{
			this.InitializeComponent();

			// Use custom title bar.
			var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			updateTitleBarLayout(coreTitleBar);
			Window.Current.SetTitleBar(this.AppTitleBar);
			coreTitleBar.LayoutMetricsChanged += (s, a) => updateTitleBarLayout(s);

			Utils.resourceLoader = ResourceLoader.GetForCurrentView();

			outputBoxInstance = outputBox;
		}

		private void updateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar) {
			// Get the size of the caption controls area and back button
			// (returned in logical pixels), and move your content around as necessary.
			var isVisible = SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility == AppViewBackButtonVisibility.Visible;
			var width = isVisible ? coreTitleBar.SystemOverlayLeftInset : 0;
			LeftPaddingColumn.Width = new GridLength(width);
			// Update title bar control size as needed to account for system size changes.
			this.AppTitleBar.Height = coreTitleBar.Height;
		}

		private void calculateIt() {
			try {
				double result = engine.calculate(inputBox.Text);
				outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + inputBox.Text + "\n= " + Utils.formatNumber(result, engine);
				inputBox.Text = "";
			} catch (ExpressionInvalidException e) {
				outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + inputBox.Text + "\n" + Utils.getString("error/header") + e.Message;
				if (e.position != -1) inputBox.Select(e.position, 0);
			}
		}

		private void onInputBoxKeyDown(object sender, KeyRoutedEventArgs args) {
			if (args.Key == VirtualKey.Enter) calculateIt();
		}

		private void onCalculateButtonClick(object sender, RoutedEventArgs e) {
			calculateIt();
		}
	}
}
