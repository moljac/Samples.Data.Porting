using System;
using System.Collections.Generic;

namespace com.twilio.video.quickstart.activity
{

	using Manifest = android.Manifest;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using PackageManager = android.content.pm.PackageManager;
	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using NonNull = android.support.annotation.NonNull;
	using FloatingActionButton = android.support.design.widget.FloatingActionButton;
	using Snackbar = android.support.design.widget.Snackbar;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using View = android.view.View;
	using EditText = android.widget.EditText;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using JsonObject = com.google.gson.JsonObject;
	using FutureCallback = com.koushikdutta.@async.future.FutureCallback;
	using Ion = com.koushikdutta.ion.Ion;
	using Dialog = com.twilio.video.quickstart.dialog.Dialog;
	using CameraSource = com.twilio.video.CameraCapturer.CameraSource;

	public class VideoActivity : AppCompatActivity
	{
		private const int CAMERA_MIC_PERMISSION_REQUEST_CODE = 1;

		/*
		 * You must provide a Twilio Access Token to connect to the Video service
		 */
		private const string TWILIO_ACCESS_TOKEN = "TWILIO_ACCESS_TOKEN";

		/*
		 * The Video Client allows a client to connect to a room
		 */
		private VideoClient videoClient;

		/*
		 * A Room represents communication between the client and one or more participants.
		 */
		private Room room;

		/*
		 * A VideoView receives frames from a local or remote video track and renders them
		 * to an associated view.
		 */
		private VideoView primaryVideoView;
		private VideoView thumbnailVideoView;

		/*
		 * Android application UI elements
		 */
		private TextView videoStatusTextView;
		private CameraCapturer cameraCapturer;
		private LocalMedia localMedia;
		private LocalAudioTrack localAudioTrack;
		private LocalVideoTrack localVideoTrack;
		private VideoView localVideoView;
		private FloatingActionButton connectActionFab;
		private FloatingActionButton switchCameraActionFab;
		private FloatingActionButton localVideoActionFab;
		private FloatingActionButton muteActionFab;
		private android.support.v7.app.AlertDialog alertDialog;
		private AudioManager audioManager;
		private string participantIdentity;

		private int previousAudioMode;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_video;

			primaryVideoView = (VideoView) findViewById(R.id.primary_video_view);
			thumbnailVideoView = (VideoView) findViewById(R.id.thumbnail_video_view);
			videoStatusTextView = (TextView) findViewById(R.id.video_status_textview);

			connectActionFab = (FloatingActionButton) findViewById(R.id.connect_action_fab);
			switchCameraActionFab = (FloatingActionButton) findViewById(R.id.switch_camera_action_fab);
			localVideoActionFab = (FloatingActionButton) findViewById(R.id.local_video_action_fab);
			muteActionFab = (FloatingActionButton) findViewById(R.id.mute_action_fab);

			/*
			 * Enable changing the volume using the up/down keys during a conversation
			 */
			VolumeControlStream = AudioManager.STREAM_VOICE_CALL;

			/*
			 * Needed for setting/abandoning audio focus during call
			 */
			audioManager = (AudioManager)getSystemService(Context.AUDIO_SERVICE);

			/*
			 * Check camera and microphone permissions. Needed in Android M.
			 */
			if (!checkPermissionForCameraAndMicrophone())
			{
				requestPermissionForCameraAndMicrophone();
			}
			else
			{
				createLocalMedia();
				createVideoClient();
			}

			/*
			 * Set the initial state of the UI
			 */
			intializeUI();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults)
		public override void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			if (requestCode == CAMERA_MIC_PERMISSION_REQUEST_CODE)
			{
				bool cameraAndMicPermissionGranted = true;

				foreach (int grantResult in grantResults)
				{
					cameraAndMicPermissionGranted &= grantResult == PackageManager.PERMISSION_GRANTED;
				}

				if (cameraAndMicPermissionGranted)
				{
					createLocalMedia();
					createVideoClient();
				}
				else
				{
					Toast.makeText(this, R.@string.permissions_needed, Toast.LENGTH_LONG).show();
				}
			}
		}

		protected internal override void onDestroy()
		{
			/*
			 * Release local media when no longer needed
			 */
			if (localMedia != null)
			{
				localMedia.release();
				localMedia = null;
			}

			base.onDestroy();
		}

		private bool checkPermissionForCameraAndMicrophone()
		{
			int resultCamera = ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA);
			int resultMic = ContextCompat.checkSelfPermission(this, Manifest.permission.RECORD_AUDIO);
			return resultCamera == PackageManager.PERMISSION_GRANTED && resultMic == PackageManager.PERMISSION_GRANTED;
		}

		private void requestPermissionForCameraAndMicrophone()
		{
			if (ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.CAMERA) || ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.RECORD_AUDIO))
			{
				Toast.makeText(this, R.@string.permissions_needed, Toast.LENGTH_LONG).show();
			}
			else
			{
				ActivityCompat.requestPermissions(this, new string[]{Manifest.permission.CAMERA, Manifest.permission.RECORD_AUDIO}, CAMERA_MIC_PERMISSION_REQUEST_CODE);
			}
		}

		private void createLocalMedia()
		{
			localMedia = LocalMedia.create(this);

			// Share your microphone
			localAudioTrack = localMedia.addAudioTrack(true);

			// Share your camera
			cameraCapturer = new CameraCapturer(this, CameraCapturer.CameraSource.FRONT_CAMERA);
			localVideoTrack = localMedia.addVideoTrack(true, cameraCapturer);
			primaryVideoView.Mirror = true;
			localVideoTrack.addRenderer(primaryVideoView);
			localVideoView = primaryVideoView;
		}

		private void createVideoClient()
		{
			/*
			 * Create a VideoClient allowing you to connect to a Room
			 */

			// OPTION 1- Generate an access token from the getting started portal
			// https://www.twilio.com/user/account/video/getting-started
			videoClient = new VideoClient(VideoActivity.this, TWILIO_ACCESS_TOKEN);

			// OPTION 2- Retrieve an access token from your own web app
			// retrieveAccessTokenfromServer();

		}

		private void connectToRoom(string roomName)
		{
			AudioFocus = true;
			ConnectOptions connectOptions = (new ConnectOptions.Builder()).roomName(roomName).localMedia(localMedia).build();
			room = videoClient.connect(connectOptions, roomListener());
			setDisconnectAction();
		}

		/*
		 * The initial state when there is no active conversation.
		 */
		private void intializeUI()
		{
			connectActionFab.ImageDrawable = ContextCompat.getDrawable(this, R.drawable.ic_call_white_24px);
			connectActionFab.show();
			connectActionFab.OnClickListener = connectActionClickListener();
			switchCameraActionFab.show();
			switchCameraActionFab.OnClickListener = switchCameraClickListener();
			localVideoActionFab.show();
			localVideoActionFab.OnClickListener = localVideoClickListener();
			muteActionFab.show();
			muteActionFab.OnClickListener = muteClickListener();
		}

		/*
		 * The actions performed during disconnect.
		 */
		private void setDisconnectAction()
		{
			connectActionFab.ImageDrawable = ContextCompat.getDrawable(this, R.drawable.ic_call_end_white_24px);
			connectActionFab.show();
			connectActionFab.OnClickListener = disconnectClickListener();
		}

		/*
		 * Creates an connect UI dialog
		 */
		private void showConnectDialog()
		{
			EditText roomEditText = new EditText(this);
			alertDialog = Dialog.createConnectDialog(roomEditText, connectClickListener(roomEditText), cancelConnectDialogClickListener(), this);
			alertDialog.show();
		}

		/*
		 * Called when participant joins the room
		 */
		private void addParticipant(Participant participant)
		{
			/*
			 * This app only displays video for one additional participant per Room
			 */
			if (thumbnailVideoView.Visibility == View.VISIBLE)
			{
				Snackbar.make(connectActionFab, "Multiple participants are not currently support in this UI", Snackbar.LENGTH_LONG).setAction("Action", null).show();
				return;
			}
			participantIdentity = participant.Identity;
			videoStatusTextView.Text = "Participant " + participantIdentity + " joined";
			/*
			 * Stop rendering local video track in primary view and move it to thumbnail view
			 */
			localVideoTrack.removeRenderer(primaryVideoView);
			thumbnailVideoView.Visibility = View.VISIBLE;
			localVideoTrack.addRenderer(thumbnailVideoView);
			localVideoView = thumbnailVideoView;
			/*
			 * Start listening for participant media events
			 */
			participant.Media.Listener = mediaListener();
		}

		/*
		 * Called when participant leaves the room
		 */
		private void removeParticipant(Participant participant)
		{
			videoStatusTextView.Text = "Participant " + participant.Identity + " left.";
			if (!participant.Identity.Equals(participantIdentity))
			{
				return;
			}
			/*
			 * Show local video in primary view
			 */
			thumbnailVideoView.Visibility = View.GONE;
			localVideoTrack.removeRenderer(thumbnailVideoView);
			primaryVideoView.Mirror = true;
			localVideoTrack.addRenderer(primaryVideoView);
			localVideoView = primaryVideoView;
		}

		/*
		 * Room events listener
		 */
		private Room.Listener roomListener()
		{
			return new ListenerAnonymousInnerClassHelper(this);
		}

		private class ListenerAnonymousInnerClassHelper : Room.Listener
		{
			private readonly VideoActivity outerInstance;

			public ListenerAnonymousInnerClassHelper(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConnected(Room room)
			{
				outerInstance.videoStatusTextView.Text = "Connected to " + room.Name;
				Title = room.Name;

				foreach (KeyValuePair<string, Participant> entry in room.Participants.entrySet())
				{
					outerInstance.addParticipant(entry.Value);
					break;
				}
			}

			public override void onConnectFailure(Room room, VideoException e)
			{
				outerInstance.videoStatusTextView.Text = "Failed to connect";
			}

			public override void onDisconnected(Room room, VideoException e)
			{
				outerInstance.videoStatusTextView.Text = "Disconnected from " + room.Name;
				outerInstance.room = null;
				outerInstance.AudioFocus = false;
				outerInstance.intializeUI();

				/*
				 * Show local video in primary view
				 */
				outerInstance.thumbnailVideoView.Visibility = View.GONE;
				outerInstance.localVideoTrack.removeRenderer(outerInstance.thumbnailVideoView);
				outerInstance.primaryVideoView.Mirror = true;
				outerInstance.localVideoTrack.addRenderer(outerInstance.primaryVideoView);
				outerInstance.localVideoView = outerInstance.primaryVideoView;
			}

			public override void onParticipantConnected(Room room, Participant participant)
			{
				outerInstance.addParticipant(participant);

			}

			public override void onParticipantDisconnected(Room room, Participant participant)
			{
				outerInstance.removeParticipant(participant);
			}
		}

		private Media.Listener mediaListener()
		{
			return new ListenerAnonymousInnerClassHelper(this);
		}

		private class ListenerAnonymousInnerClassHelper : Media.Listener
		{
			private readonly VideoActivity outerInstance;

			public ListenerAnonymousInnerClassHelper(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onAudioTrackAdded(Media media, AudioTrack audioTrack)
			{
				outerInstance.videoStatusTextView.Text = "onAudioTrackAdded";
			}

			public override void onAudioTrackRemoved(Media media, AudioTrack audioTrack)
			{
				outerInstance.videoStatusTextView.Text = "onAudioTrackRemoved";
			}

			public override void onVideoTrackAdded(Media media, VideoTrack videoTrack)
			{
				outerInstance.videoStatusTextView.Text = "onVideoTrackAdded";
				/*
				 * Set primary view as renderer for participant video track
				 */
				outerInstance.primaryVideoView.Mirror = false;
				videoTrack.addRenderer(outerInstance.primaryVideoView);
			}

			public override void onVideoTrackRemoved(Media media, VideoTrack videoTrack)
			{
				outerInstance.videoStatusTextView.Text = "onVideoTrackRemoved";
				videoTrack.removeRenderer(outerInstance.primaryVideoView);
			}

			public override void onAudioTrackEnabled(Media media, AudioTrack audioTrack)
			{

			}

			public override void onAudioTrackDisabled(Media media, AudioTrack audioTrack)
			{

			}

			public override void onVideoTrackEnabled(Media media, VideoTrack videoTrack)
			{

			}

			public override void onVideoTrackDisabled(Media media, VideoTrack videoTrack)
			{

			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private android.content.DialogInterface.OnClickListener connectClickListener(final android.widget.EditText roomEditText)
		private DialogInterface.OnClickListener connectClickListener(EditText roomEditText)
		{
			return new OnClickListenerAnonymousInnerClassHelper(this, roomEditText);
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			private EditText roomEditText;

			public OnClickListenerAnonymousInnerClassHelper(VideoActivity outerInstance, EditText roomEditText)
			{
				this.outerInstance = outerInstance;
				this.roomEditText = roomEditText;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				/*
				 * Connect to room
				 */
				outerInstance.connectToRoom(roomEditText.Text.ToString());
			}
		}

		private View.OnClickListener disconnectClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				/*
				 * Disconnect from room
				 */
				if (outerInstance.room != null)
				{
					outerInstance.room.disconnect();
				}
				outerInstance.intializeUI();
			}
		}

		private View.OnClickListener connectActionClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.showConnectDialog();
			}
		}

		private DialogInterface.OnClickListener cancelConnectDialogClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				outerInstance.intializeUI();
				outerInstance.alertDialog.dismiss();
			}
		}

		private View.OnClickListener switchCameraClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper3(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (outerInstance.cameraCapturer != null)
				{
					outerInstance.cameraCapturer.switchCamera();
					outerInstance.localVideoView.Mirror = outerInstance.cameraCapturer.CameraSource == CameraCapturer.CameraSource.FRONT_CAMERA;
				}
			}
		}

		private View.OnClickListener localVideoClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper4(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : View.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper4(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				/*
				 * Enable/disable the local video track
				 */
				if (outerInstance.localVideoTrack != null)
				{
					bool enable = !outerInstance.localVideoTrack.Enabled;
					outerInstance.localVideoTrack.enable(enable);
					int icon;
					if (enable)
					{
						icon = R.drawable.ic_videocam_green_24px;
						outerInstance.switchCameraActionFab.show();
					}
					else
					{
						icon = R.drawable.ic_videocam_off_red_24px;
						outerInstance.switchCameraActionFab.hide();
					}
					outerInstance.localVideoActionFab.ImageDrawable = ContextCompat.getDrawable(outerInstance, icon);
				}
			}
		}

		private View.OnClickListener muteClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper5(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : View.OnClickListener
		{
			private readonly VideoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper5(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				/*
				 * Enable/disable the local audio track
				 */
				if (outerInstance.localAudioTrack != null)
				{
					bool enable = !outerInstance.localAudioTrack.Enabled;
					outerInstance.localAudioTrack.enable(enable);
					int icon = enable ? R.drawable.ic_mic_green_24px : R.drawable.ic_mic_off_red_24px;
					outerInstance.muteActionFab.ImageDrawable = ContextCompat.getDrawable(outerInstance, icon);
				}
			}
		}

		private void retrieveAccessTokenfromServer()
		{
			Ion.with(this).load("http://localhost:8000/token.php").asJsonObject().Callback = new FutureCallbackAnonymousInnerClassHelper(this);
		}

		private class FutureCallbackAnonymousInnerClassHelper : FutureCallback<JsonObject>
		{
			private readonly VideoActivity outerInstance;

			public FutureCallbackAnonymousInnerClassHelper(VideoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCompleted(Exception e, JsonObject result)
			{
				if (e == null)
				{
					string accessToken = result.get("token").AsString;

					outerInstance.videoClient = new VideoClient(outerInstance, accessToken);
				}
				else
				{
					Toast.makeText(outerInstance, R.@string.error_retrieving_access_token, Toast.LENGTH_LONG).show();
				}
			}
		}

		private bool AudioFocus
		{
			set
			{
				if (value)
				{
					previousAudioMode = audioManager.Mode;
					// Request audio value before making any device switch.
					audioManager.requestAudioFocus(null, AudioManager.STREAM_VOICE_CALL, AudioManager.AUDIOFOCUS_GAIN_TRANSIENT);
					/*
					 * Use MODE_IN_COMMUNICATION as the default audio mode. It is required
					 * to be in this mode when playout and/or recording starts for the best
					 * possible VoIP performance. Some devices have difficulties with
					 * speaker mode if this is not set.
					 */
					audioManager.Mode = AudioManager.MODE_IN_COMMUNICATION;
				}
				else
				{
					audioManager.Mode = previousAudioMode;
					audioManager.abandonAudioFocus(null);
				}
			}
		}
	}

}