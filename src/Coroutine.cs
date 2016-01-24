using System;
using System.Collections;

namespace CoroutineSharp
{
    internal enum CoroutineState
    {
        None,
        Stop,
        Continue,
        End,
    }

    public class Coroutine : IYieldInstruction, IDisposable
    {
        private bool disposed;

        public ulong ID { get; private set; }
        public IEnumerator RoutineInner { get; private set; }
        internal CoroutineState State { get; set; }

        private readonly CoroutineManager coroutineManager;

        public CoroutineManager CoroutineManager
        {
            get { return coroutineManager; }
        }
        public Coroutine(IEnumerator routine, CoroutineManager coroutineManager)
        {
            RoutineInner = routine;
            this.coroutineManager = coroutineManager;
        }

        private bool MoveNext()
        {
            //LogConsole.Debug("Coroutine.MoveNext");

            var ret = false;
            try
            {
                ret = RoutineInner.MoveNext();
                if (disposed)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Stop();
                ret = false;
            }
            return ret;
        }
        private bool CheckYield()
        {
            var obj = RoutineInner.Current;
            if (obj == null)
            {
                // TODO handled specially when obj is null
                // nullYieldInstruction
                // dispatch next frame
                //LogUtils.Debug("Coroutine.Yield WaitForTicks(1)");
                Yield(new WaitForTicks(1));

                // CoroutineManager.Remove(this)
                return true;
            }

            var yieldInstruction = obj as IYieldInstruction;
            if (yieldInstruction == null)
            {
                throw new Exception();
            }

            //LogUtils.Debug("Coroutine.Yield");

            Yield(yieldInstruction);

            return true;
        }
        private bool NextStep()
        {
            //LogConsole.Debug("Coroutine.NextStep");

            if (disposed)
            {
                return false;
            }

            if (MoveNext())
            {
                CheckYield();
                return true;
            }
            else
            {
                coroutineManager.Remove(this);
                Dispose();
                return false;
            }
        }

        public static void YieldReturn(Coroutine coroutine)
        {
            //LogConsole.Debug("Coroutine.YieldReturn");
            coroutine.Resume();
        }

        #region state transition
        public bool Start()
        {
            return MoveNext() && CheckYield();
        }

        public void Stop()
        {
            State = CoroutineState.End;
        }

        private void Yield(IYieldInstruction instruction)
        {
            State = CoroutineState.Continue;
            instruction.Yield(this, YieldReturn);
        }
        private void Resume()
        {
            if (State != CoroutineState.Continue)
            {
                coroutineManager.Remove(this);
                Dispose();
                return;
            }

            try
            {
                NextStep();
            }
            catch (Exception e)
            {
                //LogConsole.Debug(e.ToString());
            }
        }
        #endregion

        public void Yield(Coroutine coroutine, Action<Coroutine> callback)
        {
        }

        #region Dispose
        ~Coroutine()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool b)
        {
        }
        #endregion
    }
}
