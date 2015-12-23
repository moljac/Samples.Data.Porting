namespace com.bixolon.labelprintersample
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using OnTouchListener = android.view.View.OnTouchListener;
	using ViewGroup = android.view.ViewGroup;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using OnCheckedChangeListener = android.widget.CompoundButton.OnCheckedChangeListener;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;
	using ListView = android.widget.ListView;
	using ScrollView = android.widget.ScrollView;
	using Toast = android.widget.Toast;

	public class DirectIoActivity : Activity, View.OnClickListener
	{
		private static readonly string[] EXAMPLE_ITEMS = new string[] {"T_resident", "T_Rotate4", "V_resident", "V_Rotate4", "Code39", "BD1", "BD3", "BD4", "BD5", "Slope"};

		private static readonly string[] EXAMPLES = new string[] {"SS3\n" + "SD20\n" + "SW800\n" + "SOT\n" + "T26,20,0,1,1,0,0,N,N,'Font - 6 pt'\n" + "T26,49,1,1,1,0,0,N,N,'Font - 8 pt'\n" + "T26,81,2,1,1,0,0,N,N,'Font - 10 pt'\n" + "T26,117,3,1,1,0,0,N,N,'Font - 12 pt'\n" + "T26,156,4,1,1,0,0,R,N,'Font - 15 pt'\n" + "T26,200,5,1,1,0,0,N,N,'Font - 20 pt'\n" + "T26,252,6,1,1,0,0,N,N,'Font - 30 pt'\n" + "P1", "SS3\n" + "SW832\n" + "T300,500,4,1,1,0,0,N,N,'ABCDEFG'\n" + "T300,500,4,1,1,0,1,N,N,'ABCDEFG'\n" + "T300,500,4,1,1,0,2,N,N,'ABCDEFG'\n" + "T300,500,4,1,1,0,3,N,N,'ABCDEFG'\n" + "P1", "SS3\n" + "SD20\n" + "SW800\n" + "SOT\n" + "V50,100,U,25,25,+1,N,N,N,0,L,0,'Vector Font Test'\n" + "V50,200,U,35,35,-1,N,N,N,0,L,0,'Vector Font Test'\n" + "V50,300,U,35,35,+1,B,R,I,0,L,0,'Vector Font Test '\n" + "V50,400,U,45,25,+1,N,N,N,0,L,0,'Vector Font Test'\n" + "V50,500,U,25,45,+1,N,N,N,0,L,0,'Vector Font Test'\n" + "V50,700,U,65,65,+1,N,N,N,0,L,0,'ABCDEFGHIJKLMNO'\n" + "V50,900,U,65,65,+1,N,N,N,0,L,0,'abcdefghijklmno'\n" + "P1", "SS3\n" + "SW832\n" + "V400,500,U,45,40,+1,N,N,N,0,L,0,'VECTOR FONT'\n" + "V400,500,U,45,40,+1,N,N,N,1,L,0,'VECTOR FONT'\n" + "V400,500,U,45,40,+1,N,N,N,2,L,0,'VECTOR FONT'\n" + "V400,500,U,45,40,+1,N,N,N,3,L,0,'VECTOR FONT'\n" + "P1", "SM10,0\n" + "B178,196,0,2,6,100,0,0,'1234567890'\n" + "B150,468,0,4,10,200,0,0,'1234567890'\n" + "P1", "SS3\n" + "SD20\n" + "SW800\n" + "BD50,50,750,500,B,20\n" + "T100,150,5,1,1,0,0,N,N,'Normal Mode'\n" + "T100,300,5,1,1,0,0,R,N,'Reverse Mode'\n" + "SOT\n" + "P1", "SS3\n" + "SD20\n" + "SW800\n" + "BD50,100,400,150,O\n" + "BD50,200,400,250,O\n" + "BD50,300,400,350,O\n" + "BD100,50,150,400,E\n" + "BD200,50,250,400,E\n" + "BD300,50,350,400,E\n" + "BD500,200,700,400,O\n" + "BD510,210,670,370,D\n" + "BD100,600,350,1000,O\n" + "T50,700,5,1,1,0,0,N,N,'NORMAL'\n" + "T50,800,5,1,1,0,0,N,N,'NORMAL'\n" + "BD110,780,340,900,E\n" + "T500,700,5,1,1,0,0,n,N,'TEST'\n" + "BD480,680,700,800,E\n" + "SOT\n" + "P1", "SW800\n" + "SM10,0\n" + "BD100,300,550,330,O\n" + "BD200,200,250,430,O\n" + "BD400,200,450,430,E\n" + "P1", "CB\n" + "SW800\n" + "SM10,0\n" + "BD100,300,300,500,O\n" + "BD400,300,700,500,B,30\n" + "P1", "CB\n" + "SS3\n" + "SD20\n" + "SW8000\n" + "BD100,300,300,800,S,100\n" + "BD600,300,400,800,S,100\n" + "P1"};

		private EditText mCommandEdit;
		private CheckBox mCheckBox;
		private EditText mResponseLengthEdit;

		private AlertDialog mDialog;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_direct_io;

			mCommandEdit = (EditText) findViewById(R.id.editText1);
			mResponseLengthEdit = (EditText) findViewById(R.id.editText2);

			mCheckBox = (CheckBox) findViewById(R.id.checkBox1);
			mCheckBox.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : CompoundButton.OnCheckedChangeListener
		{
			private readonly DirectIoActivity outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(DirectIoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onCheckedChanged(CompoundButton buttonView, bool isChecked)
			{
				outerInstance.mResponseLengthEdit.Enabled = isChecked;
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_direct_io, menu);
			return true;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				showExampleDialog();
				break;

			case R.id.button2:
				executeDirectIo();
				break;
			}
		}

		private void showExampleDialog()
		{
			if (mDialog == null)
			{
				mDialog = (new AlertDialog.Builder(DirectIoActivity.this)).setTitle("Examples").setItems(EXAMPLE_ITEMS, new OnClickListenerAnonymousInnerClassHelper(this))
			   .create();
			}
			mDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly DirectIoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(DirectIoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				outerInstance.mCommandEdit.Text = EXAMPLES[which];

			}
		}

		private void executeDirectIo()
		{
			string command = mCommandEdit.Text.ToString();
			if (command.Length == 0)
			{
				Toast.makeText(ApplicationContext, "Please input command", Toast.LENGTH_SHORT).show();
				return;
			}

			bool hasResponse = mCheckBox.Checked;

			string @string = mResponseLengthEdit.Text.ToString();
			if (@string.Length == 0)
			{
				Toast.makeText(ApplicationContext, "Please input response length", Toast.LENGTH_SHORT).show();
				return;
			}
			int responseLength = int.Parse(@string);

			MainActivity.mBixolonLabelPrinter.executeDirectIo(command, hasResponse, responseLength);
		}
	}

}