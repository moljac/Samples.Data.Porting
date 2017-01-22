namespace com.twilio.video.examples.customcapturer
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Chronometer = android.widget.Chronometer;
	using LinearLayout = android.widget.LinearLayout;


	/// <summary>
	/// This example demonstrates how to implement a custom capturer. Here we capture the contents
	/// of a LinearLayout using <seealso cref="ViewCapturer"/>. To validate we render the video frames in a
	/// <seealso cref="VideoView"/> below.
	/// </summary>
	public class CustomCapturerVideoActivity : Activity
	{
		private LocalMedia localMedia;
		private LinearLayout capturedView;
		private VideoView videoView;
		private Chronometer timerView;
		private LocalVideoTrack localVideoTrack;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_custom_capturer;

			localMedia = LocalMedia.create(this);
			capturedView = (LinearLayout) findViewById(R.id.captured_view);
			videoView = (VideoView) findViewById(R.id.video_view);
			timerView = (Chronometer) findViewById(R.id.timer_view);
			timerView.start();

			// Once added we should see our linear layout rendered live below
			localVideoTrack = localMedia.addVideoTrack(true, new ViewCapturer(capturedView));
			localVideoTrack.addRenderer(videoView);
		}

		protected internal override void onDestroy()
		{
			localVideoTrack.removeRenderer(videoView);
			localMedia.removeVideoTrack(localVideoTrack);
			timerView.stop();
			localMedia.release();
			base.onDestroy();
		}
	}

}