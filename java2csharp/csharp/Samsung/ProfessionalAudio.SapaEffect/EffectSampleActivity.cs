namespace com.samsung.audiosuite.sapaeffectsample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using ImageButton = android.widget.ImageButton;
	using TextView = android.widget.TextView;

	/// <summary>
	/// Activity class with functionality same for stand alone activity and activity started from other
	/// applications.
	/// </summary>
	public abstract class EffectSampleActivity : Activity
	{

		protected internal ImageButton mVolDownButton;
		protected internal ImageButton mVolUpButton;
		private TextView mVolTextView;

		public EffectSampleActivity() : base()
		{
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			ContentView = R.layout.activity_main;
			base.onCreate(savedInstanceState);

			// Views are being set from the layout.

			this.mVolDownButton = (ImageButton) findViewById(R.id.vDownButton);
			this.mVolUpButton = (ImageButton) findViewById(R.id.vUpButton);
			this.mVolTextView = (TextView) findViewById(R.id.current_value_textview);
		}

		/// <summary>
		/// This method sets appropriate state on buttons when volume has neither maximum nor minimum
		/// value.
		/// </summary>
		protected internal virtual void setBetweenVolumeStateOnButtons()
		{
			mVolDownButton.Enabled = true;
			mVolUpButton.Enabled = true;
		}

		/// <summary>
		/// This method sets appropriate state on buttons when volume has minimum value.
		/// </summary>
		protected internal virtual void setMinVolumeStateOnButtons()
		{
			mVolDownButton.Enabled = false;
			mVolUpButton.Enabled = true;
		}

		/// <summary>
		/// This method sets appropriate state on buttons when volume has maximum value.
		/// </summary>
		protected internal virtual void setMaxVolumeStateOnButtons()
		{
			mVolDownButton.Enabled = true;
			mVolUpButton.Enabled = false;
		}

		/// <summary>
		/// This method sets text on view of value of volume.
		/// </summary>
		/// <param name="text">
		///            Text to be set. </param>
		protected internal virtual void updateVolumeTextView(string text)
		{
			mVolTextView.Text = text;
		}

	}
}