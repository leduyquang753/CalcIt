using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.Resources;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace CalcItUWP {
	/// <summary>
	/// The main page of the application, where everything happens.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		public readonly CalculatorEngine engine = new CalculatorEngine();
		public static TextBox outputBoxInstance;
		private readonly IPropertySet config = new PropertySet(); // The init value is to prevent nasty errors. The actual config object will be loaded in the constructor.
		private StorageFile startupExpressionsFile = null;
		const string startupExpressionsFileName = "Startup expressions.txt";
		private string lastExpression = null;
		private bool calculateLastIfEmpty = true;

		public MainPage()
		{
			InitializeComponent();

			// Use custom title bar.
			var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			updateTitleBarLayout(coreTitleBar);
			Window.Current.SetTitleBar(this.AppTitleBar);
			coreTitleBar.LayoutMetricsChanged += (s, a) => updateTitleBarLayout(s);

			Utils.resourceLoader = ResourceLoader.GetForCurrentView();

			outputBoxInstance = outputBox;
			variablesToggleButton.IsChecked = false;

			// Load config.
			ApplicationData appData = ApplicationData.Current;
			config = appData.RoamingSettings.Values;
			appData.DataChanged += (a, o) => { loadConfig(); };
			loadConfig();
			loadStartupExpressions();

			updateVariableBoxes();
		}

		private async void loadStartupExpressions() {
			StorageFolder dataFolder = ApplicationData.Current.LocalFolder;
			if (await dataFolder.FileExistsAsync(startupExpressionsFileName)) {
				startupExpressionsFile = await dataFolder.GetFileAsync(startupExpressionsFileName);
				int lineNumber = 0;
				foreach (string line in await FileIO.ReadLinesAsync(startupExpressionsFile)) {
					lineNumber++;
					string expression = line;
					int doubleSlashPosition = expression.IndexOf("//");
					if (doubleSlashPosition != -1) expression = expression.Substring(0, doubleSlashPosition).Trim();
					if (expression != "") {
						try {
							engine.calculate(expression);
						} catch (ExpressionInvalidException e) {
							int position = ((ExpressionInvalidException)e).position;
							outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + inputBox.Text + "\n"
								+ String.Format(Utils.getString("error/headerStartup/" + (position == -1 ? "y" : "xy")), new[] { lineNumber.ToString(), position.ToString() })
								+ e.Message;
						}
					}
				}
			} else {
				startupExpressionsFile = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/defaultStartupExpressions.txt"))).CopyAsync(dataFolder, startupExpressionsFileName);
			}
		}

		private void loadConfig() {
			if (config.ContainsKey("angleUnit")) {
				switch (config["angleUnit"]) {
					case 0: engine.angleUnit = AngleUnits.DEGREE; radioAngleUnitDegree.IsChecked = true; break;
					case 1: engine.angleUnit = AngleUnits.RADIAN; radioAngleUnitRadian.IsChecked = true; break;
					case 2: engine.angleUnit = AngleUnits.GRADIAN; radioAngleUnitGradian.IsChecked = true; break;
				}
			} else config["angleUnit"] = 0;

			if (config.ContainsKey("decimalDot")) engine.decimalDot = (bool)config["decimalDot"]; else config["decimalDot"] = false;
			(engine.decimalDot ? radioDecimalDot : radioDecimalComma).IsChecked = true;

			if (config.ContainsKey("enforceDecimalSeparator")) engine.enforceDecimalSeparator = (bool)config["enforceDecimalSeparator"]; else config["enforceDecimalSeparator"] = false;
			checkEnforceDecimalSeparator.IsChecked = engine.enforceDecimalSeparator;

			if (config.ContainsKey("thousandDot")) engine.thousandDot = (bool)config["thousandDot"]; else config["thousandDot"] = false;
			(engine.thousandDot ? radioThousandSeparatorDot : radioThousandSeparatorSpace).IsChecked = true;

			if (config.ContainsKey("mulAsterisk")) engine.mulAsterisk = (bool)config["mulAsterisk"]; else config["mulAsterisk"] = false;
			(engine.mulAsterisk ? radioMultiplicationAsterisk : radioMultiplicationDot).IsChecked = true;

			if (config.ContainsKey("zeroUndefinedVars")) engine.zeroUndefinedVars = (bool)config["zeroUndefinedVars"]; else config["zeroUndefinedVars"] = false;
			(engine.zeroUndefinedVars ? radioDefaultUndefinedAs0 : radioRaiseErrorForUndefinedVariables).IsChecked = true;

			if (config.ContainsKey("calculateLastIfEmpty")) calculateLastIfEmpty = (bool)config["calculateLastIfEmpty"]; else config["calculateLastIfEmpty"] = false;
			checkCalculateLastIfEmpty.IsOn = calculateLastIfEmpty;

			if (config.ContainsKey("maximumHistorySize")) inputBox.maximumHistorySize = (int)config["maximumHistorySize"]; else config["maximumHistorySize"] = 64;
			sliderMaximumHistorySize.Value = inputBox.maximumHistorySize;
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
			string input;
			if (inputBox.Text == "") {
				if (calculateLastIfEmpty && lastExpression != null) {
					input = lastExpression;
				} else return;
			} else input = inputBox.Text;
			int currentPosition = 0;
			string expression = "";
			try {
				foreach (string str in input.Split('|')) {
					expression = str;
					if (expression.Trim() == "") {
						currentPosition += expression.Length + 1;
						continue;
					}
					double result = engine.calculate(expression);
					outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + expression.Trim() + "\n= " + Utils.formatNumber(result, engine);
					currentPosition += expression.Length + 1;
				}
				if (inputBox.history.Count >= inputBox.maximumHistorySize) inputBox.history.RemoveRange(inputBox.maximumHistorySize - 1, inputBox.history.Count - inputBox.maximumHistorySize + 1);
				if (inputBox.Text != "") inputBox.history.Insert(0, inputBox.Text);
				lastExpression = input;
				inputBox.Text = "";
				inputBox.historyPointer = -1;
			} catch (ExpressionInvalidException e) {
				outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + expression.Trim() + "\n" + Utils.getString("error/header") + e.Message;
				if (e.position != -1) inputBox.Select(currentPosition + e.position, 0);
			}
			// Scroll output box to the bottom.
			var grid = (Grid)VisualTreeHelper.GetChild(outputBox, 0);
			for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++) {
				object obj = VisualTreeHelper.GetChild(grid, i);
				if (!(obj is ScrollViewer)) continue;
				((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f, true);
				break;
			}
			updateVariableBoxes();
		}

		private void updateVariableBoxes() {
			ansValueBox.Text = engine.getVariableString("Ans");
			preAnsValueBox.Text = engine.getVariableString("PreAns");
			foreach (FrameworkElement view in variableViewStack.Children) ((VariableView)view).update();
		}

		private void onInputBoxKeyDown(object sender, KeyRoutedEventArgs args) {
			if (args.Key == VirtualKey.Enter) {
				calculateIt();
				args.Handled = true;
			}
		}

		private void onCalculateButtonClick(object sender, RoutedEventArgs e) {
			calculateIt();
		}

		private void onSettingsButtonClick(object sender, RoutedEventArgs e) {
			mainSplitView.IsPaneOpen = true;
		}

		private void onSettingsReturnClick(object sender, RoutedEventArgs e) {
			mainSplitView.IsPaneOpen = false;
		}

		private void onVariablesPaneToggle(object sender, RoutedEventArgs e) {
			__Splitter__.Visibility = variablesGrid.Visibility = (bool)variablesToggleButton.IsChecked ? Visibility.Visible : Visibility.Collapsed;
			mainGrid.ColumnDefinitions[2].MinWidth = (bool)variablesToggleButton.IsChecked ? 260 : 0;
			if (!(bool)variablesToggleButton.IsChecked) mainGrid.ColumnDefinitions[2].Width = GridLength.Auto;
		}

		private void onSettingsAngleUnitChangedDegree(object sender, RoutedEventArgs e) {
			engine.angleUnit = AngleUnits.DEGREE;
			config["angleUnit"] = 0;
		}

		private void onSettingsAngleUnitChangedRadian(object sender, RoutedEventArgs e) {
			engine.angleUnit = AngleUnits.RADIAN;
			config["angleUnit"] = 1;
		}

		private void onSettingsAngleUnitChangedGradian(object sender, RoutedEventArgs e) {
			engine.angleUnit = AngleUnits.GRADIAN;
			config["angleUnit"] = 2;
		}

		private void onSettingsDecimalSeparatorChanged(object sender, RoutedEventArgs e) {
			config["decimalDot"] = engine.decimalDot = (bool)radioDecimalDot.IsChecked;
			if (engine.decimalDot) {
				radioMultiplicationAsterisk.IsChecked = true;
			}
		}

		private void onSettingsEnforceDecimalSeparatorChanged(object sender, RoutedEventArgs e) {
			config["enforceDecimalSeparator"] = engine.enforceDecimalSeparator = (bool)checkEnforceDecimalSeparator.IsChecked;
		}

		private void onSettingsThousandSeparatorChanged(object sender, RoutedEventArgs e) {
			config["thousandDot"] = engine.thousandDot = (bool)radioThousandSeparatorDot.IsChecked;
		}

		private void onSettingsMultiplicationSignChanged(object sender, RoutedEventArgs e) {
			config["mulAsterisk"] = engine.mulAsterisk = (bool)radioMultiplicationAsterisk.IsChecked;
			if (!engine.mulAsterisk) {
				radioDecimalComma.IsChecked = true;
			}
		}

		private void onSettingsUndefinedVariablesBehaviorChanged(object sender, RoutedEventArgs e) {
			config["zeroUndefinedVars"] = engine.zeroUndefinedVars = (bool)radioDefaultUndefinedAs0.IsChecked;
		}

		private void onVariableWatchAddition(object sender, RoutedEventArgs e) {
			variableViewStack.Children.Add(new VariableView(variableViewStack.Children.Count, this));
		}

		public void onVariableRemovalRequested(int index) {
			UIElementCollection views = variableViewStack.Children;
			views.RemoveAt(index);
			for (int i = index; i < views.Count; i++) ((VariableView)views[i]).index = i;
		}

		private void onAnsInsertion(object sender, RoutedEventArgs e) {
			int caretPos = inputBox.SelectionStart;
			inputBox.Text = inputBox.Text.Insert(Math.Min(caretPos, inputBox.Text.Length), "Ans");
			inputBox.SelectionStart = caretPos + 3;
		}

		private void onPreAnsInsertion(object sender, RoutedEventArgs e) {
			int caretPos = inputBox.SelectionStart;
			inputBox.Text = inputBox.Text.Insert(Math.Min(caretPos, inputBox.Text.Length), "PreAns");
			inputBox.SelectionStart = caretPos + 6;
		}

		private void onSplitterMove(object sender, object e) {
			bool small = mainGrid.ColumnDefinitions[0].ActualWidth < 350;
			Thickness newThickness = inputBox.Margin;
			newThickness.Right = small ? 62 : 20;
			inputBox.Margin = newThickness;
			buttonCalculateSmall.Visibility = small ? Visibility.Visible : Visibility.Collapsed;
			buttonCalculate.Visibility = small ? Visibility.Collapsed : Visibility.Visible;

		}

		private void onClearRequested(object sender, RoutedEventArgs e) {
			outputBox.Text = "";
		}

		private async void onHelpButtonClick(object sender, RoutedEventArgs e) {
			await new HelpAndAbout().ShowAsync();
		}

		private async void onStartupExpressionsEditingRequested(object sender, RoutedEventArgs e) {
			if (startupExpressionsFile != null) await Launcher.LaunchFileAsync(startupExpressionsFile);
		}

		private async void onAppRestartRequested(object sender, RoutedEventArgs e) {
			await CoreApplication.RequestRestartAsync("");
		}

		private void onMaximumStoredExpressionsChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e) {
			if (inputBox == null) return;
			config["maximumHistorySize"] = inputBox.maximumHistorySize = (int)sliderMaximumHistorySize.Value;
		}

		private void onCheckCalculateLastOfEmptyChanged(object sender, RoutedEventArgs e) {
			config["calculateLastIfEmpty"] = calculateLastIfEmpty = checkCalculateLastIfEmpty.IsOn;
		}
	}
}
