using System;
using Android.App;
using Android.OS;
using IO.Crossbar.Autobahn.Websocket;
using IO.Crossbar.Autobahn.Websocket.Interfaces;
using IO.Crossbar.Autobahn.Websocket.Types;
using WebRTC.AppRTC.Abstraction;
using Exception = System.Exception;
using WebSocketConnectionHandler = IO.Crossbar.Autobahn.Websocket.WebSocketConnectionHandler;


namespace WebRTC.Droid.Demo
{
    public static class H113Platform
    {
        public static void Init(Activity activity)
        {
            Platform.Init(activity);
            WebSocketConnectionFactory.Factory = () => new OkHttpWebSocket();
            ExecutorServiceFactory.Factory = tag => new ExecutorServiceImpl(tag);
            ExecutorServiceFactory.MainExecutor = new MainExecutor();
        }

        private class MainExecutor : AppRTC.Abstraction.IExecutor
        {
            private readonly Handler _handler = new Handler(Looper.MainLooper);
            public bool IsCurrentExecutor => Looper.MainLooper == Looper.MyLooper();
            public void Execute(Action action) => _handler.Post(action);
        }

        private class ExecutorServiceImpl : IExecutorService
        {

            private readonly HandlerThread _handlerThread;
            private readonly Handler _handler;

            public ExecutorServiceImpl(string tag)
            {
                _handlerThread = new HandlerThread(tag);
                _handlerThread.Start();
                _handler = new Handler(_handlerThread.Looper);
            }


            public bool IsCurrentExecutor => _handlerThread.Looper == Looper.MyLooper();
            public void Execute(Action action) => _handler.Post(action);
        
            public void Release()
            {
                _handlerThread.QuitSafely();
            }
        }
    }
}