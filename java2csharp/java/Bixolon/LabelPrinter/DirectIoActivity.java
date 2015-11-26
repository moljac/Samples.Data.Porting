package com.bixolon.labelprintersample;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.view.Menu;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnTouchListener;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.ListView;
import android.widget.ScrollView;
import android.widget.Toast;

public class DirectIoActivity extends Activity implements View.OnClickListener {
	private static final String[] EXAMPLE_ITEMS = {
		"T_resident",
		"T_Rotate4",
		"V_resident",
		"V_Rotate4",
		"Code39",
		"BD1",
		"BD3",
		"BD4",
		"BD5",
		"Slope"
	};
	
	private static final String[] EXAMPLES = {
		"SS3\n" +	// Set Speed to 5 ips
		"SD20\n" +	// Set Density level to 20
		"SW800\n" +	// Set Label Width 800
		"SOT\n" +	// Set Printing Orientation from Top to Bottom
		"T26,20,0,1,1,0,0,N,N,'Font - 6 pt'\n" +
		"T26,49,1,1,1,0,0,N,N,'Font - 8 pt'\n" +
		"T26,81,2,1,1,0,0,N,N,'Font - 10 pt'\n" +
		"T26,117,3,1,1,0,0,N,N,'Font - 12 pt'\n" +
		"T26,156,4,1,1,0,0,R,N,'Font - 15 pt'\n" +
		"T26,200,5,1,1,0,0,N,N,'Font - 20 pt'\n" +
		"T26,252,6,1,1,0,0,N,N,'Font - 30 pt'\n" +
		"P1",
		
		"SS3\n" +
		"SW832\n" +
		"T300,500,4,1,1,0,0,N,N,'ABCDEFG'\n" +
		"T300,500,4,1,1,0,1,N,N,'ABCDEFG'\n" +
		"T300,500,4,1,1,0,2,N,N,'ABCDEFG'\n" +
		"T300,500,4,1,1,0,3,N,N,'ABCDEFG'\n" +
		"P1",
		
		"SS3\n" +	// Set speed to 5 ips
		"SD20\n" +	// Set density to 20
		"SW800\n" +	// Set label width to 800
		"SOT\n" +	// Set printing direction to from top to bottom
		"V50,100,U,25,25,+1,N,N,N,0,L,0,'Vector Font Test'\n" +
		"V50,200,U,35,35,-1,N,N,N,0,L,0,'Vector Font Test'\n" +
		"V50,300,U,35,35,+1,B,R,I,0,L,0,'Vector Font Test '\n" +
		"V50,400,U,45,25,+1,N,N,N,0,L,0,'Vector Font Test'\n" +
		"V50,500,U,25,45,+1,N,N,N,0,L,0,'Vector Font Test'\n" +
		"V50,700,U,65,65,+1,N,N,N,0,L,0,'ABCDEFGHIJKLMNO'\n" +
		"V50,900,U,65,65,+1,N,N,N,0,L,0,'abcdefghijklmno'\n" +
		"P1",
		
		"SS3\n" +
		"SW832\n" +
		"V400,500,U,45,40,+1,N,N,N,0,L,0,'VECTOR FONT'\n" +
		"V400,500,U,45,40,+1,N,N,N,1,L,0,'VECTOR FONT'\n" +
		"V400,500,U,45,40,+1,N,N,N,2,L,0,'VECTOR FONT'\n" +
		"V400,500,U,45,40,+1,N,N,N,3,L,0,'VECTOR FONT'\n" +
		"P1",
		
		"SM10,0\n" +
		"B178,196,0,2,6,100,0,0,'1234567890'\n" +	// Caution : The position is not (178,196)
		"B150,468,0,4,10,200,0,0,'1234567890'\n" +
		"P1",
		
		"SS3\n" +	// Set Speed to 5 ips
		"SD20\n" +	// Set Density level to 20
		"SW800\n" +	// Set Label Width to 800
		"BD50,50,750,500,B,20\n" +
		"T100,150,5,1,1,0,0,N,N,'Normal Mode'\n" +
		"T100,300,5,1,1,0,0,R,N,'Reverse Mode'\n" +
		"SOT\n" +
		"P1",
		
		"SS3\n" +	// Set Printing Speed to 5 ips
		"SD20\n" +	// Set Printing Density level to 20
		"SW800\n" +	// Set Label Width to 800
		"BD50,100,400,150,O\n" +	// Draw a block in Overwriting Mode
		"BD50,200,400,250,O\n" +
		"BD50,300,400,350,O\n" +
		"BD100,50,150,400,E\n" +	// Draw a block in Exclusive OR mode
		"BD200,50,250,400,E\n" +
		"BD300,50,350,400,E\n" +
		"BD500,200,700,400,O\n" +
		"BD510,210,670,370,D\n" +	// Draw a block in Delete mode, namely Erase block area
		"BD100,600,350,1000,O\n" +
		"T50,700,5,1,1,0,0,N,N,'NORMAL'\n" +	// Write Text data on image buffer
		"T50,800,5,1,1,0,0,N,N,'NORMAL'\n" +
		"BD110,780,340,900,E\n" +
		"T500,700,5,1,1,0,0,n,N,'TEST'\n" +
		"BD480,680,700,800,E\n" +
		"SOT\n" +	// Set Printing Orientation from Top to Bottom
		"P1",	// Start Printing
		
		"SW800\n" +
		"SM10,0\n" +
		"BD100,300,550,330,O\n" +	// Overwrite mode
		"BD200,200,250,430,O\n" +	// Overwrite mode
		"BD400,200,450,430,E\n" +	// Exclusive OR mode
		"P1",
		
		"CB\n" +
		"SW800\n" +
		"SM10,0\n" +
		"BD100,300,300,500,O\n" +
		"BD400,300,700,500,B,30\n" +	// Box mode, additional parameter follows
		"P1",
		
		"CB\n" +
		"SS3\n" +
		"SD20\n" +
		"SW8000\n" +
		"BD100,300,300,800,S,100\n" +	// Slope mode, additional parameter follows
		"BD600,300,400,800,S,100\n" +
		"P1",
	};
	
	private EditText mCommandEdit;
	private CheckBox mCheckBox;
	private EditText mResponseLengthEdit;
	
	private AlertDialog mDialog;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_direct_io);
		
		mCommandEdit = (EditText) findViewById(R.id.editText1);
		mResponseLengthEdit = (EditText) findViewById(R.id.editText2);
		
		mCheckBox = (CheckBox) findViewById(R.id.checkBox1);
		mCheckBox.setOnCheckedChangeListener(new OnCheckedChangeListener() {
			
			public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
				mResponseLengthEdit.setEnabled(isChecked);
			}
		});
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_direct_io, menu);
		return true;
	}

	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			showExampleDialog();
			break;
			
		case R.id.button2:
			executeDirectIo();
			break;
		}
	}
	
	private void showExampleDialog() {
		if (mDialog == null) {
			mDialog = new AlertDialog.Builder(DirectIoActivity.this).setTitle("Examples").setItems(EXAMPLE_ITEMS, new DialogInterface.OnClickListener() {
				
				public void onClick(DialogInterface dialog, int which) {
					mCommandEdit.setText(EXAMPLES[which]);
					
				}
			}).create();
		}
		mDialog.show();
	}
	
	private void executeDirectIo() {
		String command = mCommandEdit.getText().toString();
		if (command.length() == 0) {
			Toast.makeText(getApplicationContext(), "Please input command", Toast.LENGTH_SHORT).show();
			return;
		}
		
		boolean hasResponse = mCheckBox.isChecked();
		
		String string = mResponseLengthEdit.getText().toString();
		if (string.length() == 0) {
			Toast.makeText(getApplicationContext(), "Please input response length", Toast.LENGTH_SHORT).show();
			return;
		}
		int responseLength = Integer.parseInt(string);
		
		MainActivity.mBixolonLabelPrinter.executeDirectIo(command, hasResponse, responseLength);
	}
}
