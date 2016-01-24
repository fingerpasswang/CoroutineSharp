using System;
using System.Collections;
using System.Collections.Generic;

namespace CoroutineSharp
{
    // todo thread-unsafe
    public class CoroutineManager : IDisposable
    {
        private readonly Dictionary<ulong, Coroutine> coroutinesDict = new Dictionary<ulong, Coroutine>();
        private readonly List<Coroutine> toAddCoroutines = new List<Coroutine>();
        private ulong currentID;
        private bool disposed;
        private readonly TimeManager timeManager;

        public TimeManager TimeManager
        {
            get { return timeManager; }
        }

        public CoroutineManager(TimeManager timeManager)
        {
            this.timeManager = timeManager;
        }

        public Coroutine StartCoroutine(IEnumerator routine)
        {
            //LogUtils.Debug("CoroutineManager.StartCoroutine");

            if (disposed)
            {
                return null;
            }

            var coroutine = new Coroutine(routine, this);
            var yielding = coroutine.Start();

            // todo need to check whether this coroutine invoke a timer 
            // record in coroutinesDict
            if (yielding)
            {
                // alloc a id, and record
            }

            return coroutine;
        }

        public void Remove(Coroutine coroutine)
        {
            if (coroutine.ID == 0)
            {
                return;
            }

            coroutinesDict.Remove(coroutine.ID);
        }
        public void Add(Coroutine coroutine)
        {
            coroutinesDict[coroutine.ID] = coroutine;
        }

        #region Dispose
        ~CoroutineManager()
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
