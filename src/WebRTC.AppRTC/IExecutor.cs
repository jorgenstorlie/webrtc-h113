using System;

namespace WebRTC.AppRTC
{
    public interface IExecutor 
    {
        bool IsCurrentExecutor { get; }
        
        void Execute(Action action);
    }

    public interface IExecutorService : IExecutor
    {
        void Release();
    }
}