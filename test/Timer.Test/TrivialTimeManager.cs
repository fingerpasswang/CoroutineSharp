using System.Collections.Generic;
using CoroutineSharp;

namespace Timer.Test
{
    public class TrivialTimeManager
    {
        private class TimerNodeComparer : IComparer<TimerNode>
        {
            public int Compare(TimerNode x, TimerNode y)
            {
                return (int) y.ExpireTick - (int)x.ExpireTick;
            }
        }

        private readonly PriorityQueue<TimerNode> timerNodesSet = new PriorityQueue<TimerNode>(new TimerNodeComparer());
        private uint index;
        public uint FixedTicks
        {
            get { return index; }
        }

        public ITimer AddTimer(uint afterTick, OnTimerTimeout callback, params object[] userData)
        {
            var expireTick = afterTick + index;
            var node = new TimerNode()
            {
                ExpireTick = expireTick,
                Callback = callback,
                UserData = userData,
            };

            timerNodesSet.Push(node);

            return node;
        }

        public void FixedTick()
        {
            DoTimer();
            Shift();
            DoTimer();
        }

        private void Shift()
        {
            index++;
        }

        private void DoTimer()
        {
            while (timerNodesSet.Count > 0 && timerNodesSet.Top().ExpireTick <= index)
            {
                var top = timerNodesSet.Pop();

                top.Callback(top, top.UserData);
            }
        }
    }
}
