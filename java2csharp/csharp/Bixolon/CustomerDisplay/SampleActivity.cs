using System;

namespace com.bixolon.customerdisplaysample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ViewGroup = android.view.ViewGroup;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;

	using BixolonCustomerDisplay = com.bixolon.customerdisplay.BixolonCustomerDisplay;

	public class SampleActivity : Activity, View.OnClickListener
	{
		private BixolonCustomerDisplay mBcd = MainActivity.mBcd;

		private Button mOpenButton;
		private Button mCloseButton;
		private LinearLayout mLinearLayout;
		private EditText mWriteString1;
		private EditText mWriteString2;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_sample;

			mOpenButton = (Button) findViewById(R.id.button1);
			mCloseButton = (Button) findViewById(R.id.button2);
			mLinearLayout = (LinearLayout) findViewById(R.id.linearLayout1);
	//		setEnableView(mLinearLayout, false);

			mWriteString1 = (EditText) findViewById(R.id.editText1);
			mWriteString2 = (EditText) findViewById(R.id.editText2);

			mOpenButton.OnClickListener = this;
			mCloseButton.OnClickListener = this;
			Button button = (Button) findViewById(R.id.button3);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button4);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button5);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button6);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button7);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button8);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button9);
			button.OnClickListener = this;
		}

	//	@Override
	//	public void onDestroy() {
	//		closeDisplay();
	//		super.onDestroy();
	//	}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_main, menu);
			return true;
		}

		private void setEnableView(View view, bool enabled)
		{
			if (view is ViewGroup)
			{
				ViewGroup viewGroup = (ViewGroup) view;
				int childCount = viewGroup.ChildCount;

				for (int i = 0; i < childCount; i++)
				{
					View childView = viewGroup.getChildAt(i);
					if (childView is ViewGroup)
					{
						setEnableView(childView, enabled);
					}
					else
					{
						viewGroup.getChildAt(i).Enabled = enabled;
					}
				}
			}
		}

		private void readyToDisplay()
		{
			mBcd.selectPeripheralDevices(true);
			mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_USA;
			mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_437_STANDARD_EUROPEAN;
			mBcd.ScrollMode = BixolonCustomerDisplay.MODE_HORIZONTAL;
			mBcd.Cursor = false;
			mBcd.DimmingControl = BixolonCustomerDisplay.BRIGHTNESS_100;
		}

		private void closeDisplay()
		{
			if (mBcd != null)
			{
				readyToDisplay();
				mBcd.clearScreen();
				mBcd.selectPeripheralDevices(false);
				mBcd.close();
			}
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				mBcd = new BixolonCustomerDisplay(SampleActivity.this);
				if (mBcd.open())
				{
					setEnableView(mLinearLayout, true);
					mOpenButton.Enabled = false;
					mCloseButton.Enabled = true;
				}
				else
				{
					mBcd = null;
				}
				break;

			case R.id.button2:
				closeDisplay();
				setEnableView(mLinearLayout, false);
				mOpenButton.Enabled = true;
				mCloseButton.Enabled = false;
				break;

			case R.id.button3: // 1st line display
				readyToDisplay();
				mBcd.BlinkInterval = 0;
				mBcd.ReversedCharacterMode = false;
				mBcd.moveCursor(1, 1);
				mBcd.writeString(mWriteString1.Text.ToString());
				mBcd.selectPeripheralDevices(false);
				break;

			case R.id.button4: // clear 1st line
				readyToDisplay();
				mBcd.moveCursor(1, 20);
				mBcd.clearLine();
				mBcd.selectPeripheralDevices(false);
				break;

			case R.id.button5: // 2nd line display
				readyToDisplay();
				mBcd.BlinkInterval = 0;
				mBcd.ReversedCharacterMode = false;
				mBcd.moveCursor(2, 1);
				mBcd.writeString(mWriteString2.Text.ToString());
				mBcd.selectPeripheralDevices(false);
				break;

			case R.id.button6: // clear 2nd line
				readyToDisplay();
				mBcd.moveCursor(2, 20);
				mBcd.clearLine();
				mBcd.selectPeripheralDevices(false);
				break;

			case R.id.button7: // time
				readyToDisplay();
				DateTime c = new DateTime();
				int h = c.Hour;
				int m = c.Minute;
				mBcd.setTime(h, m);
				mBcd.selectPeripheralDevices(false);
				break;

			case R.id.button8: // macro
				readyToDisplay();
				mBcd.startMacroDefinition();
				mBcd.clearScreen();
				mBcd.BlinkInterval = 0;
				mBcd.moveCursor(1, 1);
				mBcd.writeString("Save your money");
				mBcd.moveCursor(2, 1);
				mBcd.writeString(" with BIXOLON");
				mBcd.BlinkInterval = 10;
				mBcd.endMacroDefinition();
				mBcd.executeDefinedMacro(5, 60);
				break;

			case R.id.button9: // clear screen
				readyToDisplay();
				mBcd.clearScreen();
				mBcd.selectPeripheralDevices(false);
				break;
			}
		}
	}

}