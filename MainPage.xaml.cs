using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
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
		private bool useOldOutputBox = false;
		private bool loading = true;

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

			loading = false;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
			FocusManager.TryFocusAsync(inputBox, FocusState.Keyboard);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
		}

		private async void loadStartupExpressions() {
			StorageFolder dataFolder = ApplicationData.Current.LocalFolder;
			if (!await dataFolder.FileExistsAsync(startupExpressionsFileName)) 
				startupExpressionsFile = await (await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/defaultStartupExpressions.txt"))).CopyAsync(dataFolder, startupExpressionsFileName);
			startupExpressionsFile = await dataFolder.GetFileAsync(startupExpressionsFileName);
			int lineNumber = 0;
			foreach (string line in await FileIO.ReadLinesAsync(startupExpressionsFile)) {
				lineNumber++;
				string expression = line;
				int doubleSlashPosition = expression.IndexOf("//");
				if (doubleSlashPosition != -1) expression = expression.Substring(0, doubleSlashPosition).Trim();
				if (expression.Trim() != "") {
					try {
						engine.calculate(expression);
					} catch (ExpressionInvalidException e) {
						expression = line.Trim();
						textEmptyOutputPanel.Visibility = Visibility.Collapsed;
						int position = ((ExpressionInvalidException)e).position;
						string errorText = String.Format(Utils.getString("error/headerStartup/" + (position == -1 ? "y" : "xy")), new[] { lineNumber.ToString(), position.ToString() }) + e.Message;
						outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + inputBox.Text + "\n" + errorText;
						outputStack.Children.Add(new CalculationResult(expression, null, errorText));
					}
				}
			}

			// Scroll output controls to the bottom.
			if (useOldOutputBox) {
				scrollOutputBox();
			} else {
				await Task.Delay(200);
				outputPanel.ChangeView(outputPanel.HorizontalOffset, outputPanel.ScrollableHeight, outputPanel.ZoomFactor);
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

			if (config.ContainsKey("useOldOutputBox")) useOldOutputBox = (bool)config["useOldOutputBox"]; else config["useOldOutputBox"] = false;
			outputBox.Visibility = useOldOutputBox ? Visibility.Visible : Visibility.Collapsed;
			outputPanel.Visibility = useOldOutputBox ? Visibility.Collapsed : Visibility.Visible;
			checkUseOldOutputBox.IsChecked = useOldOutputBox;
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

		private async void calculateIt() {
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
					string resultString = Utils.formatNumber(engine.calculate(expression), engine);
					outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + expression + "\n= " + resultString;
					outputStack.Children.Add(new CalculationResult(expression, "= " + resultString, null));
					currentPosition += expression.Length + 1;
				}
				if (inputBox.history.Count >= inputBox.maximumHistorySize) inputBox.history.RemoveRange(inputBox.maximumHistorySize - 1, inputBox.history.Count - inputBox.maximumHistorySize + 1);
				if (inputBox.Text != "") inputBox.history.Insert(0, inputBox.Text);
				lastExpression = input;
				inputBox.Text = "";
				inputBox.historyPointer = -1;
			} catch (ExpressionInvalidException e) {
				expression = expression.Trim();
				string errorText = Utils.getString("error/header") + e.Message;
				outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + expression + "\n" + errorText;
				outputStack.Children.Add(new CalculationResult(expression, null, errorText));
				if (e.position != -1) inputBox.Select(currentPosition + e.position, 0);
			}
			updateVariableBoxes();
			textEmptyOutputPanel.Visibility = Visibility.Collapsed;
			// Scroll output controls to the bottom.
			if (useOldOutputBox) {
				scrollOutputBox();
			} else {
				await Task.Delay(200);
				outputPanel.ChangeView(outputPanel.HorizontalOffset, outputPanel.ScrollableHeight, outputPanel.ZoomFactor);
			}
		}

		private void scrollOutputBox() {
			var grid = (Grid)VisualTreeHelper.GetChild(outputBox, 0);
			for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++) {
				object obj = VisualTreeHelper.GetChild(grid, i);
				if (!(obj is ScrollViewer)) continue;
				((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f, true);
				break;
			}
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
			if (loading) return;
			engine.angleUnit = AngleUnits.DEGREE;
			config["angleUnit"] = 0;
		}

		private void onSettingsAngleUnitChangedRadian(object sender, RoutedEventArgs e) {
			if (loading) return;
			engine.angleUnit = AngleUnits.RADIAN;
			config["angleUnit"] = 1;
		}

		private void onSettingsAngleUnitChangedGradian(object sender, RoutedEventArgs e) {
			if (loading) return;
			engine.angleUnit = AngleUnits.GRADIAN;
			config["angleUnit"] = 2;
		}

		private void onSettingsDecimalSeparatorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["decimalDot"] = engine.decimalDot = (bool)radioDecimalDot.IsChecked;
			if (engine.decimalDot) {
				radioMultiplicationAsterisk.IsChecked = true;
			}
		}

		private void onSettingsEnforceDecimalSeparatorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["enforceDecimalSeparator"] = engine.enforceDecimalSeparator = (bool)checkEnforceDecimalSeparator.IsChecked;
		}

		private void onSettingsThousandSeparatorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["thousandDot"] = engine.thousandDot = (bool)radioThousandSeparatorDot.IsChecked;
		}

		private void onSettingsMultiplicationSignChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["mulAsterisk"] = engine.mulAsterisk = (bool)radioMultiplicationAsterisk.IsChecked;
			if (!engine.mulAsterisk) {
				radioDecimalComma.IsChecked = true;
			}
		}

		private void onSettingsUndefinedVariablesBehaviorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["zeroUndefinedVars"] = engine.zeroUndefinedVars = (bool)radioDefaultUndefinedAs0.IsChecked;
		}

		private async void onVariableWatchAddition(object sender, RoutedEventArgs e) {
			variableViewStack.Children.Add(new VariableView(variableViewStack.Children.Count, this));
			await Task.Delay(200);
			variableViewer.ChangeView(variableViewer.HorizontalOffset, variableViewer.ScrollableHeight, variableViewer.ZoomFactor);
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
			outputStack.Children.Clear();
			textEmptyOutputPanel.Visibility = useOldOutputBox ? Visibility.Collapsed : Visibility.Visible;
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
			if (loading) return;
			if (inputBox == null) return;
			config["maximumHistorySize"] = inputBox.maximumHistorySize = (int)sliderMaximumHistorySize.Value;
		}

		private void onCheckCalculateLastOfEmptyChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["calculateLastIfEmpty"] = calculateLastIfEmpty = checkCalculateLastIfEmpty.IsOn;
		}

		private void onCheckUseOldOutputBoxChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["useOldOutputBox"] = useOldOutputBox = (bool)checkUseOldOutputBox.IsChecked;
			outputBox.Visibility = useOldOutputBox ? Visibility.Visible : Visibility.Collapsed;
			outputPanel.Visibility = useOldOutputBox ? Visibility.Collapsed : Visibility.Visible;
			textEmptyOutputPanel.Visibility = !useOldOutputBox && outputStack.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
			if (useOldOutputBox) scrollOutputBox();
			else
				outputPanel.ChangeView(outputPanel.HorizontalOffset, outputPanel.ScrollableHeight, outputPanel.ZoomFactor);
		}
	}
}
