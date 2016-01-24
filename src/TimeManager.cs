using System;
using System.Collections.Generic;
using TimerNodes = System.Collections.Generic.LinkedList<CoroutineSharp.TimerNode>;

namespace CoroutineSharp
{
    public delegate void OnTimerTimeout(ITimer timer, object[] userData);

    public class TimerNode : ITimer
    {
        public uint ExpireTick;
        public OnTimerTimeout Callback;
        public object[] UserData;
    }

    public interface ITimer
    {
        
    }

    public class TimeManager
    {
        #region const
        public const int TimeNearShift = 8;
        public const int TimeNearNum = 1 << TimeNearShift;      // 256
        public const int TimeNearMask = TimeNearNum - 1;        // 0x000000ff

        public const int TimeLevelShift = 6;
        public const int TimeLevelNum = 1 << TimeLevelShift;    // 64
        public const int TimeLevelMask = TimeLevelNum - 1;      // 00 00 00 (0011 1111)
        #endregion

        private static TimeManager instance;
        public static TimeManager Instance
        {
            get { return instance ?? (instance = new TimeManager()); }
        }

        private readonly TimerNodes[] nearTimerNodes;
        private readonly TimerNodes[][] levelTimerNodes;

        private ulong originTick;
        private ulong currentTick;

        private uint index;

        public TimeManager()
        {
            nearTimerNodes = new TimerNodes[TimeNearNum];

            for (int i = 0; i < TimeNearNum; i++)
            {
                nearTimerNodes[i] = new TimerNodes();
            }

            levelTimerNodes = new[] 
            {     new TimerNodes[TimeLevelNum]
                , new TimerNodes[TimeLevelNum]
                , new TimerNodes[TimeLevelNum]
                , new TimerNodes[TimeLevelNum] };

            for (int i = 0; i < levelTimerNodes.Length; i++)
            {
                for (int j = 0; j < TimeLevelNum; j++)
                {
                    levelTimerNodes[i][j] = new TimerNodes();
                }
            }

            originTick = currentTick = GetSystemTick();
        }

        public ulong Ticks
        {
            get { return currentTick; }
        }

        public uint FixedTicks
        {
            get { return index; }
        }

        public ITimer AddTimer(float time, OnTimerTimeout callback, params object[] userData)
        {
            var tick = (uint)(time*100);

            return AddTimer(tick, callback, userData);
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

            AddTimerNode(node);

            return node;
        }

        public bool RemoveTimer(ITimer timer)
        {
            return false;
        }

        public void Update()
        {
            var tick = GetSystemTick();
            var diff = (long)(tick - currentTick);

            for (int i = 0; i < diff; i++)
            {
                FixedTick();
            }

            currentTick = tick;
        }

        public ulong GetSystemTick()
        {
            return (ulong)DateTime.Now.Ticks / (100000);
        }

        #region Internal
        public void FixedTick()
        {
            // user registers a timer of zero sometimes 
            TimerUpdate();
            TimerShift();
            TimerUpdate();
        }

        private void AddTimerNode(TimerNode node)
        {
            var expire = node.ExpireTick;

            if (expire < index)
            {
                throw new Exception();
            }

            // expire equals index at higher 24bits 
            if ((expire | TimeNearMask) == (index | TimeNearMask))
            {
                nearTimerNodes[expire & TimeNearMask].AddLast(node);
            }
            else
            {
                var shift = TimeNearShift;

                for (int i = 0; i < 4; i++)
                {
                    var lowerMask = (1 << (shift+TimeLevelShift))-1;

                    // expire equals index at higher (24-6*(i+1))bits 
                    if ((expire | lowerMask) == (index | lowerMask))
                    {
                        // take out [(8+i*6), (14+i*6)) of the bits
                        levelTimerNodes[i][(expire >> shift)&TimeLevelMask].AddLast(node);
                        break;
                    }

                    shift += TimeLevelShift;
                }
            }
        }

        private void TimerUpdate()
        {
            var id = index & TimeNearMask;
            var toDispatch = nearTimerNodes[id];

            if (toDispatch.Count == 0)
            {
                return;
            }

            nearTimerNodes[id] = new TimerNodes();
            DispatchAll(toDispatch);
        }

        private void TimerShift()
        {
            // todo wraparound
            index++;

            var ct = index;

            // mask0 : 8bit
            // mask1 : 14bit
            // mask2 : 20bit
            // mask3 : 26bit
            // mask4 : 32bit

            var partialIndex = ct & TimeNearMask;

            if (partialIndex != 0)
            {
                return;
            }

            ct >>= TimeNearShift;

            for (int i = 0; i < 4; i++)
            {
                partialIndex = ct & TimeLevelMask;

                if (partialIndex == 0)
                {
                    ct >>= TimeLevelShift;
                    continue;
                }

                ReAddAll(levelTimerNodes[i], partialIndex);
                break;
            }
        }

        private void ReAddAll(TimerNodes[] nodesArr, long id)
        {
            var toReAdd = nodesArr[id];

            if (toReAdd.Count == 0)
            {
                return;
            }

            nodesArr[id] = new TimerNodes();

            foreach (var node in toReAdd)
            {
                AddTimerNode(node);
            }
        }

        private void DispatchAll(IEnumerable<TimerNode> timerNodes)
        {
            foreach (var timerNode in timerNodes)
            {
                timerNode.Callback(timerNode, timerNode.UserData);
            }
        }

        #endregion
    }
}
