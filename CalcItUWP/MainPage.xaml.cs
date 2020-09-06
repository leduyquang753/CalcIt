using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
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
using CalcItCore;

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
		private bool hasOutputSinceLastSettingsChange = false;
		private bool showIntermediateCalculations = true;

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
			focusInputBox();
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
						string errorText = String.Format(Utils.getString("error/headerStartup/" + (position == -1 ? "y" : "xy")), new[] { lineNumber.ToString(), position.ToString() }) + Utils.formatError(e.Message, e.messageArguments);
						outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + inputBox.Text + "\n" + errorText;
						outputStack.Children.Add(new CalculationResult(expression, null, errorText, this));
						hasOutputSinceLastSettingsChange = true;
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
			textEmptyOutputPanel.Visibility = !useOldOutputBox && outputStack.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
		}

		private void loadConfig() {
			if (config.ContainsKey("angleUnit")) {
				switch (config["angleUnit"]) {
					case 0: engine.angleUnit = AngleUnits.DEGREE; radioAngleUnitDegree.IsChecked = true; break;
					case 1: engine.angleUnit = AngleUnits.RADIAN; radioAngleUnitRadian.IsChecked = true; break;
					case 2: engine.angleUnit = AngleUnits.GRADIAN; radioAngleUnitGradian.IsChecked = true; break;
				}
			} else config["angleUnit"] = 0;

			if (config.ContainsKey("decimalDot")) engine.decimalDot = (bool)config["decimalDot"]; else config["decimalDot"] = engine.decimalDot;
			(engine.decimalDot ? radioDecimalDot : radioDecimalComma).IsChecked = true;

			if (config.ContainsKey("enforceDecimalSeparator")) engine.enforceDecimalSeparator = (bool)config["enforceDecimalSeparator"]; else config["enforceDecimalSeparator"] = engine.enforceDecimalSeparator;
			checkEnforceDecimalSeparator.IsChecked = engine.enforceDecimalSeparator;

			if (config.ContainsKey("thousandDot")) engine.thousandDot = (bool)config["thousandDot"]; else config["thousandDot"] = engine.thousandDot;
			(engine.thousandDot ? radioThousandSeparatorDot : radioThousandSeparatorSpace).IsChecked = true;

			if (config.ContainsKey("mulAsterisk")) engine.mulAsterisk = (bool)config["mulAsterisk"]; else config["mulAsterisk"] = engine.mulAsterisk;
			(engine.mulAsterisk ? radioMultiplicationAsterisk : radioMultiplicationDot).IsChecked = true;

			if (config.ContainsKey("zeroUndefinedVars")) engine.zeroUndefinedVars = (bool)config["zeroUndefinedVars"]; else config["zeroUndefinedVars"] = engine.zeroUndefinedVars;
			(engine.zeroUndefinedVars ? radioDefaultUndefinedAs0 : radioRaiseErrorForUndefinedVariables).IsChecked = true;

			if (config.ContainsKey("calculateLastIfEmpty")) calculateLastIfEmpty = (bool)config["calculateLastIfEmpty"]; else config["calculateLastIfEmpty"] = calculateLastIfEmpty;
			checkCalculateLastIfEmpty.IsOn = calculateLastIfEmpty;

			if (config.ContainsKey("maximumHistorySize")) inputBox.maximumHistorySize = (int)config["maximumHistorySize"]; else config["maximumHistorySize"] = inputBox.maximumHistorySize;
			sliderMaximumHistorySize.Value = inputBox.maximumHistorySize;

			if (config.ContainsKey("useOldOutputBox")) useOldOutputBox = (bool)config["useOldOutputBox"]; else config["useOldOutputBox"] = useOldOutputBox;
			outputBox.Visibility = useOldOutputBox ? Visibility.Visible : Visibility.Collapsed;
			outputPanel.Visibility = useOldOutputBox ? Visibility.Collapsed : Visibility.Visible;
			checkUseOldOutputBox.IsChecked = useOldOutputBox;

			if (config.ContainsKey("appTheme")) {
				switch (config["appTheme"]) {
					case 0: RequestedTheme = ElementTheme.Default; radioThemeDefault.IsChecked = true; break;
					case 1: RequestedTheme = ElementTheme.Light; radioThemeLight.IsChecked = true; break;
					case 2: RequestedTheme = ElementTheme.Dark; radioThemeDark.IsChecked = true; break;
				}
			} else config["appTheme"] = 0;

			if (config.ContainsKey("showIntermediateCalculations")) showIntermediateCalculations = (bool)config["showIntermediateCalculations"]; else config["showIntermediateCalculations"] = showIntermediateCalculations;
			checkShowIntermediateCalculations.IsOn = showIntermediateCalculations;
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
				string[] expressions = input.Split('|');
				int i = 0;
				foreach (string str in expressions) {
					expression = str;
					if (expression.Trim() == "") {
						currentPosition += expression.Length + 1;
						continue;
					}
					string resultString = CoreUtils.formatNumber(engine.calculate(expression), engine);
					if (showIntermediateCalculations || ++i == expressions.Length) {
						expression = expression.Trim();
						outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + expression + "\n= " + (resultString ?? "? (" + Utils.getString("text/oldOutputNumberOutOfRange") + ")");
						CalculationResult resultElement = new CalculationResult(expression, resultString, null, this);
						if (resultString == null) resultElement.setResultOutOfRange();
						outputStack.Children.Add(resultElement);
					}
					currentPosition += expression.Length + 1;
				}
				if (inputBox.history.Count >= inputBox.maximumHistorySize) inputBox.history.RemoveRange(inputBox.maximumHistorySize - 1, inputBox.history.Count - inputBox.maximumHistorySize + 1);
				if (inputBox.Text != "") inputBox.history.Insert(0, inputBox.Text);
				lastExpression = input;
				inputBox.Text = "";
				inputBox.historyPointer = -1;
				hasOutputSinceLastSettingsChange = true;
			} catch (ExpressionInvalidException e) {
				expression = expression.Trim();
				string errorText = Utils.getString("error/header") + Utils.formatError(e.Message, e.messageArguments);
				outputBox.Text += (outputBox.Text.Length == 0 ? "" : "\n\n") + expression + "\n" + errorText;
				outputStack.Children.Add(new CalculationResult(expression, null, errorText, this));
				if (e.position != -1) inputBox.Select(currentPosition + e.position, 0);
				hasOutputSinceLastSettingsChange = true;
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
			if (grid == null) return; // Prevent nasty crash.
			for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++) {
				object obj = VisualTreeHelper.GetChild(grid, i);
				if (!(obj is ScrollViewer)) continue;
				((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f, true);
				break;
			}
		}		

		private void updateVariableBoxes() {
			ansValueBox.Text = Utils.getVariableString(engine, "Ans");
			preAnsValueBox.Text = Utils.getVariableString(engine, "PreAns");
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
			focusInputBox();
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
			onFormatsUpdated();
		}

		private void onSettingsAngleUnitChangedRadian(object sender, RoutedEventArgs e) {
			if (loading) return;
			engine.angleUnit = AngleUnits.RADIAN;
			config["angleUnit"] = 1;
			onFormatsUpdated();
		}

		private void onSettingsAngleUnitChangedGradian(object sender, RoutedEventArgs e) {
			if (loading) return;
			engine.angleUnit = AngleUnits.GRADIAN;
			config["angleUnit"] = 2;
			onFormatsUpdated();
		}

		private void onSettingsDecimalSeparatorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["decimalDot"] = engine.decimalDot = (bool)radioDecimalDot.IsChecked;
			if (engine.decimalDot || engine.thousandDot) {
				radioMultiplicationAsterisk.IsChecked = true;
			}
			onFormatsUpdated();
		}

		private void onSettingsEnforceDecimalSeparatorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["enforceDecimalSeparator"] = engine.enforceDecimalSeparator = (bool)checkEnforceDecimalSeparator.IsChecked;
		}

		private void onSettingsThousandSeparatorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["thousandDot"] = engine.thousandDot = (bool)radioThousandSeparatorDot.IsChecked;
			if (engine.decimalDot || engine.thousandDot) {
				radioMultiplicationAsterisk.IsChecked = true;
			}
			onFormatsUpdated();
		}

		private void onSettingsMultiplicationSignChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["mulAsterisk"] = engine.mulAsterisk = (bool)radioMultiplicationAsterisk.IsChecked;
			if (!engine.mulAsterisk) {
				radioThousandSeparatorSpace.IsChecked = true;
				radioDecimalComma.IsChecked = true;
			}
			onFormatsUpdated();
		}

		private void onSettingsUndefinedVariablesBehaviorChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["zeroUndefinedVars"] = engine.zeroUndefinedVars = (bool)radioDefaultUndefinedAs0.IsChecked;
			updateVariableBoxes();
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
			mainGrid.ColumnDefinitions[2].Width = new GridLength(ActualWidth - mainGrid.ColumnDefinitions[0].ActualWidth - mainGrid.ColumnDefinitions[1].ActualWidth);
		}

		private void onClearRequested(object sender, RoutedEventArgs e) {
			outputBox.Text = "";
			outputStack.Children.Clear();
			textEmptyOutputPanel.Visibility = useOldOutputBox ? Visibility.Collapsed : Visibility.Visible;
			hasOutputSinceLastSettingsChange = false;
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

		public async void focusInputBox() {
			await FocusManager.TryFocusAsync(inputBox, FocusState.Keyboard);
		}

		public void pasteTextToInput(string text) {
			inputBox.Text = text;
			inputBox.historyPointer = -1;
			inputBox.Select(text.Length, 0);
			focusInputBox();
		}

		private void onAppThemeChangedDefault(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["appTheme"] = 0;
			RequestedTheme = ElementTheme.Default;
		}

		private void onAppThemeChangedLight(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["appTheme"] = 1;
			RequestedTheme = ElementTheme.Light;
		}

		private void onAppThemeChangedDark(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["appTheme"] = 2;
			RequestedTheme = ElementTheme.Dark;
		}

		/// <summary>
		/// Fired when settings change that alter the display formats (such as separators)
		/// to notify the user about those outdated outputs.
		/// </summary>
		private void onFormatsUpdated() {
			if (!hasOutputSinceLastSettingsChange) return;
			outputStack.Children.Add(new PaneSettingsChanged());
			outputBox.Text += "\n\n" + Utils.getString("textSettingsChanged/Text");
			hasOutputSinceLastSettingsChange = false;
		}

		private void onCheckShowIntermediateCalculationsChanged(object sender, RoutedEventArgs e) {
			if (loading) return;
			config["showIntermediateCalculations"] = showIntermediateCalculations = checkShowIntermediateCalculations.IsOn;
		}
	}
}
