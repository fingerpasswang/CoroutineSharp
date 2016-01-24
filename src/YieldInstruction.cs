using System;

namespace CoroutineSharp
{
    public interface IYieldInstruction
    {
        void Yield(Coroutine coroutine, Action<Coroutine> callback);
    }

    public class WaitForSeconds : IYieldInstruction
    {
        public float WaitTime { get; private set; }
        public WaitForSeconds(float time)
        {
            WaitTime = time;
        }

        public void Yield(Coroutine coroutine, Action<Coroutine> callback)
        {
            coroutine.CoroutineManager.TimeManager.AddTimer(WaitTime, (timer, data) => callback(data[0] as Coroutine),
                coroutine);
        }
    }

    public class WaitForTicks : IYieldInstruction
    {
        public uint WaitTick { get; private set; }
        public WaitForTicks(uint tick)
        {
            WaitTick = tick;
        }

        public void Yield(Coroutine coroutine, Action<Coroutine> callback)
        {
            coroutine.CoroutineManager.TimeManager.AddTimer(WaitTick, (timer, data) => callback(data[0] as Coroutine),
                coroutine);
        }
    }

    public class WaitForRPC : IYieldInstruction
    {
        public void Yield(Coroutine coroutine, Action<Coroutine> callback)
        {
        }
    }
}
