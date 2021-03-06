﻿using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using WebRTC.Abstraction;
using WebRTC.AppRTC.Abstraction;
using WebRTC.iOS.Binding;
using Xamarin.Essentials;

namespace WebRTC.iOS.Demo
{

    public interface IARDVideoCallViewControllerDelegate
    {
        void DidFinish(CallViewControllerBase viewController);
    }


    public abstract class CallViewControllerBase : UIViewController, IARDVideoCallViewDelegate, IAppRTCEngineEvents, IRTCAudioSessionDelegate
    {
        private AVAudioSessionPortOverride _portOvveride;


        private ARDVideoCallView _videoCallView;

        private VideoRendererProxy _localRenderer;
        private VideoRendererProxy _remoteRenderer;

        private ARDFileCaptureController _fileCaptureController;

        public IARDVideoCallViewControllerDelegate Delegate { get; set; }

        public bool IsSimulator => ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR;


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _localRenderer = new VideoRendererProxy();
            _remoteRenderer = new VideoRendererProxy();

            _localRenderer.Renderer = _videoCallView.LocalVideoRender;
            _remoteRenderer.Renderer = _videoCallView.RemoteVideoRender;


        }

        public override void LoadView()
        {
            base.LoadView();



            _videoCallView = new ARDVideoCallView(CGRect.Empty, !IsSimulator);
            _videoCallView.Delegate = this;

            View = _videoCallView;
        }



        public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
        {
            return UIInterfaceOrientationMask.All;
        }


        public void OnPeerFactoryCreated(IPeerConnectionFactory factory)
        {
        }

        public void OnDisconnect(DisconnectType disconnectType)
        {
            Disconnect();
        }

        public IVideoCapturer CreateVideoCapturer(IPeerConnectionFactory factory, IVideoSource videoSource)
        {
            if (!IsSimulator)
            {
                return factory.CreateCameraCapturer(videoSource, true);
            }

            _fileCaptureController = new ARDFileCaptureController(videoSource);
            return _fileCaptureController;
        }


        public async void ReadyToStart()
        {
            var permission = await Xamarin.Essentials.Permissions.RequestAsync<Permissions.Camera>();
            if (permission == PermissionStatus.Granted)
            {
                StartVideoCallInternal(_localRenderer, _remoteRenderer);
            }
            else
            {

            }
        }

        public void OnError(string description)
        {
            var alertDialog = UIAlertController.Create("Error", description, UIAlertControllerStyle.Alert);
            alertDialog.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Destructive, (s) => Disconnect()));
            PresentViewController(alertDialog, true, null);
        }


        public virtual void DidSwitchCamera(ARDVideoCallView view)
        {
            if (_fileCaptureController != null)
            {
                _fileCaptureController.Toggle();
                return;
            }
            SwitchCamera();
        }

        public void DidChangeRoute(ARDVideoCallView view)
        {
            var @override = AVAudioSessionPortOverride.None;
            if (_portOvveride == AVAudioSessionPortOverride.None)
            {
                @override = AVAudioSessionPortOverride.Speaker;
            }

            RTCDispatcher.DispatchAsyncOnType(RTCDispatcherQueueType.AudioSession, () =>
            {
                var session = RTCAudioSession.SharedInstance;
                session.LockForConfiguration();
                session.OverrideOutputAudioPort(@override, out NSError error);

                if (error == null)
                    _portOvveride = @override;
                else
                {
                    Console.WriteLine("Error overriding output port:{0}", error.LocalizedDescription);
                }
                session.UnlockForConfiguration();
            });
        }

        [Export("audioSession:didDetectPlayoutGlitch:")]
        public void AudioSession(RTCAudioSession audioSession, long totalNumberOfGlitches)
        {
            Console.WriteLine("Audio session detected glitch, total:{0}", totalNumberOfGlitches);
        }

        public void DidHangup(ARDVideoCallView view)
        {
            Disconnect();
        }

        public void DidEnableStats(ARDVideoCallView view)
        {

        }

        protected abstract void Disconnect();

        protected abstract void SwitchCamera();

        protected abstract void StartVideoCall(IVideoRenderer localRenderer, IVideoRenderer remoteRenderer);

        private void StartVideoCallInternal(IVideoRenderer localRenderer, IVideoRenderer remoteRenderer)
        {

            var session = RTCAudioSession.SharedInstance;

            session.UseManualAudio = true;
            session.IsAudioEnabled = false;


            StartVideoCall(localRenderer, remoteRenderer);
        }

    }

    public abstract class CallViewControllerBase<TConnectionParam, TSignalParam, TController> : CallViewControllerBase
        where TSignalParam : ISignalingParameters
        where TConnectionParam : IConnectionParameters
        where TController : AppRTCControllerBase<TConnectionParam, TSignalParam>
    {

        private TController _rtcController;


        protected abstract TController CreateController();

        protected abstract void Connect(TController rtcController);


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _rtcController = CreateController();

            Connect(_rtcController);
        }

        protected override void Disconnect()
        {
            _rtcController.Disconnect();
            Delegate?.DidFinish(this);
        }

        protected override void SwitchCamera()
        {
            _rtcController.SwitchCamera();
        }

        protected override void StartVideoCall(IVideoRenderer localRenderer, IVideoRenderer remoteRenderer)
        {
            _rtcController.StartVideoCall(localRenderer, remoteRenderer);
        }
    }
}
