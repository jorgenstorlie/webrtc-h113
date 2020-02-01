using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;
using WebRTC.Abstraction;

namespace WebRTC.Droid.Demo
{
    public class CallFragment : Fragment
    {
        private const string ExtraVideoCallEnabled = "video_call";
        private const string ExtraCaptureSliderEnabled = "capture_slider";
        private const string ExtraName = "name";

        private TextView _contactView;
        private ImageButton _cameraSwitchButton;
        private ImageButton _videoScalingButton;
        private ImageButton _toggleMuteButton;
        private TextView _captureFormatText;
        private SeekBar _captureFormatSlider;
        private IOnCallEvents _callEvents;
        private ScalingType _scalingType;

        private CallFragment()
        {
        }

        public static CallFragment Create(string name, bool videoCallEnabled, bool captureSliderEnabled)
        {
            var args = new Bundle();
            args.PutString(ExtraName, name);
            args.PutBoolean(ExtraVideoCallEnabled, videoCallEnabled);
            args.PutBoolean(ExtraCaptureSliderEnabled, captureSliderEnabled);

            return new CallFragment {Arguments = args};
        }


        public interface IOnCallEvents
        {
            void OnCallHangUp();
            void OnCameraSwitch();
            void OnVideoScalingSwitch(ScalingType scalingType);
            void OnCaptureFormatChange(int width, int height, int framerate);
            bool OnToggleMic();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var controlView = inflater.Inflate(Resource.Layout.fragment_call, container, false);

            _contactView = controlView.FindViewById<TextView>(Resource.Id.contact_name_call);

            var disconnectButton = controlView.FindViewById<ImageButton>(Resource.Id.button_call_disconnect);
            _cameraSwitchButton = controlView.FindViewById<ImageButton>(Resource.Id.button_call_switch_camera);
            _videoScalingButton = controlView.FindViewById<ImageButton>(Resource.Id.button_call_scaling_mode);
            _toggleMuteButton = controlView.FindViewById<ImageButton>(Resource.Id.button_call_toggle_mic);
            _captureFormatText = controlView.FindViewById<TextView>(Resource.Id.capture_format_text_call);
            _captureFormatSlider = controlView.FindViewById<SeekBar>(Resource.Id.capture_format_slider_call);

            disconnectButton.Click += (sender, args) => _callEvents.OnCallHangUp();
            _cameraSwitchButton.Click += (sender, args) => _callEvents.OnCameraSwitch();
            _videoScalingButton.Click += (sender, args) =>
            {
                if (_scalingType == ScalingType.AspectFill)
                {
                    _videoScalingButton.SetBackgroundResource(Resource.Drawable.ic_action_full_screen);
                    _scalingType = ScalingType.AspectFill;
                }
                else
                {
                    _videoScalingButton.SetBackgroundResource(Resource.Drawable.ic_action_return_from_full_screen);
                    _scalingType = ScalingType.AspectFit;
                }

                _callEvents.OnVideoScalingSwitch(_scalingType);
            };

            _scalingType = ScalingType.AspectFill;

            _toggleMuteButton.Click += (sender, args) =>
            {
                var enabled = _callEvents.OnToggleMic();
                _toggleMuteButton.Alpha = enabled ? 1f : 0.3f;
            };

            return controlView;
        }

        public override void OnStart()
        {
            base.OnStart();

            var videoCallEnabled = Arguments.GetBoolean(ExtraVideoCallEnabled);
            var captureSliderEnabled = Arguments.GetBoolean(ExtraCaptureSliderEnabled);
            var name = Arguments.GetString(ExtraName);

            _contactView.Text = name;
            if (!videoCallEnabled)
            {
                _cameraSwitchButton.Visibility = ViewStates.Invisible;
            }

            if (captureSliderEnabled)
            {

                _captureFormatSlider.SetOnSeekBarChangeListener(new CaptureQualityController(_captureFormatText,
                    _callEvents));
            }
            else
            {
                _captureFormatText.Visibility = ViewStates.Gone;
                _captureFormatSlider.Visibility = ViewStates.Gone;
            }
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            _callEvents = (IOnCallEvents) context;
        }
    }
}