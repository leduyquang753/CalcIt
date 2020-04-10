using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System;

namespace CalcItAndroid {
	class FragmentCalculate: Fragment {
		private EditText inputBox, outputBox;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
			return inflater.Inflate(Resource.Layout.layout_calculate, container, false);
		}

		private void insertText(string text) {
			inputBox.Text += text;
		}

		public void onButtonClick(object sender, EventArgs eventArgs) {
			insertText(((Button)sender).Text);
			inputBox.RequestFocus();
		}

		public void onCalculateClick(object sender, EventArgs eventArgs) {

		}

		private int[] numpadIds = new[] { Resource.Id.calculatorButton111, Resource.Id.calculatorButton112, Resource.Id.calculatorButton113, Resource.Id.calculatorButton114,
										  Resource.Id.calculatorButton121, Resource.Id.calculatorButton122, Resource.Id.calculatorButton123, Resource.Id.calculatorButton124, Resource.Id.calculatorButton125, Resource.Id.calculatorButton126,
										  Resource.Id.calculatorButton131, Resource.Id.calculatorButton132, Resource.Id.calculatorButton133, Resource.Id.calculatorButton134, Resource.Id.calculatorButton135,
										  Resource.Id.calculatorButton141, Resource.Id.calculatorButton142, Resource.Id.calculatorButton143, Resource.Id.calculatorButton144, Resource.Id.calculatorButton145,
										  Resource.Id.calculatorButton151, Resource.Id.calculatorButton152, Resource.Id.calculatorButton153, Resource.Id.calculatorButton154, Resource.Id.calculatorButton155,
										  Resource.Id.calculatorButton161, Resource.Id.calculatorButton162, Resource.Id.calculatorButton163
										};

		public override void OnViewCreated(View view, Bundle savedInstanceState) {
			inputBox = view.FindViewById<EditText>(Resource.Id.inputBox);
			inputBox.ShowSoftInputOnFocus = false;
			outputBox = view.FindViewById<EditText>(Resource.Id.outputBox);
			foreach (int i in numpadIds) view.FindViewById<Button>(i).Click += onButtonClick;
			view.FindViewById<Button>(Resource.Id.calculatorButtonCalculate).Click += onCalculateClick;
		}
	}
}