using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace CalcItUWP {
	public sealed partial class HelpAndAbout: ContentDialog {
		public HelpAndAbout() {
			this.InitializeComponent();
			PackageVersion version = Package.Current.Id.Version;
			textVersion.Text = String.Format(Utils.getString("textVersion"), new[] { version.Major + "." + version.Minor, version.Build.ToString() });
		}

		private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
		}

		private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
		}
	}
}
