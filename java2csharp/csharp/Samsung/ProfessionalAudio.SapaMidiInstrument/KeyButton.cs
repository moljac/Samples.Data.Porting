namespace com.samsung.audiosuite.sapamidisample
{

	using Button = android.widget.Button;
	using Context = android.content.Context;
	using TypedArray = android.content.res.TypedArray;
	using AttributeSet = android.util.AttributeSet;

	public class KeyButton : Button
	{

		private int mNote = 69;
		private int mVelocity = 80;

		public KeyButton(Context context, AttributeSet attrs) : base(context, attrs)
		{
			TypedArray a = context.obtainStyledAttributes(attrs, R.styleable.KeyButton);

			mNote = a.getInteger(R.styleable.KeyButton_note, 69);
			mVelocity = a.getInteger(R.styleable.KeyButton_velocity, 80);

			a.recycle();
		}

		public virtual int Note
		{
			set
			{
				mNote = value;
			}
			get
			{
				return mNote;
			}
		}

		public virtual int Velocity
		{
			set
			{
				mVelocity = value;
			}
			get
			{
				return mVelocity;
			}
		}


	}

}