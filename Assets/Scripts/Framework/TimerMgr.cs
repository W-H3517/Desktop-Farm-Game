using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class TimerMgr : BaseManager<TimerMgr>
    {
        private readonly Dictionary<int, TimerItem> _timers = new Dictionary<int, TimerItem>();
        private readonly Dictionary<int, TimerItem> _timersRealTime = new Dictionary<int, TimerItem>();
        private readonly List<int> _toRemove = new List<int>();

        /// <summary>
        /// 计时器步长，0.1s = 100ms
        /// </summary>
        private const float TimeStep = 0.1f;
        
        private Coroutine _timerFlowingCoroutine;
        private Coroutine _timerFlowingCoroutineReal;
        
        private readonly WaitForSecondsRealtime _waitForSecondsRealtime = new WaitForSecondsRealtime(TimeStep);
        private readonly WaitForSeconds _waitForSeconds = new WaitForSeconds(TimeStep);

        public TimerMgr()
        {
            Start();
        }

        public void Start()
        {
            _timerFlowingCoroutine = MonoMgr.Instance.StartCoroutine(TimerFlowing(false,_timers));
            _timerFlowingCoroutineReal =  MonoMgr.Instance.StartCoroutine(TimerFlowing(true,_timersRealTime));
        }

        public void Stop()
        {
            MonoMgr.Instance.StopCoroutine(_timerFlowingCoroutine);
            MonoMgr.Instance.StopCoroutine(_timerFlowingCoroutineReal);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator TimerFlowing(bool realtime, Dictionary<int, TimerItem> timers)
        {
            while (true)
            {
                if (realtime) yield return _waitForSecondsRealtime;
                else yield return _waitForSeconds;
                foreach (var obj in timers)
                {
                    if (!obj.Value.Active) continue;
                    //检测计时器的循环指令
                    if (obj.Value.RepeatAction != null)
                    {
                        obj.Value.RepeatTime -= (int)(TimeStep * 1000);
                        if (obj.Value.RepeatTime <= 0) obj.Value.RepeatActionInvoke();
                    }

                    //检测计时器的延迟执行指令，如果没有，则不会自动关闭。
                    if (obj.Value.DelayAction != null)
                    {
                        obj.Value.Time -= (int)(TimeStep * 1000);
                        //检测是否到时，如果到时，停止计时，并回收计时器
                        if (--obj.Value.Time <= 0)
                        {
                            obj.Value.DelayAction();
                            _toRemove.Add(obj.Key);
                        }
                    }
                }

                //检测待移除计时器
                if (_toRemove.Count > 0)
                {
                    foreach (var key in _toRemove)
                    {
                        // Debug.Log("移除一个");
                        PoolMgr.Instance.PutRecyclable(timers[key]);
                        timers.Remove(key);
                    }
                    _toRemove.Clear();
                }
            }
        }

        /// <summary>
        /// 创建一个新的计时器
        /// </summary>
        /// <param name="isRealTime">false时，受Time.timescale影响</param>
        /// <param name="time">延迟执行时间 单位s</param>
        /// <param name="action">延迟执行callback</param>
        /// <param name="repeatTime">重复执行时间间隔</param>
        /// <param name="repeatAction">重复执行callback</param>
        /// <returns>计时器ID</returns>
        public int CreateTimer(bool isRealTime,float time, Action action, float repeatTime = 0, Action repeatAction = null)
        {
            //直接从对象池取出。
            TimerItem temp = PoolMgr.Instance.GetRecyclable<TimerItem>();
            temp.SetTimeAndStart((int)(time * 1000), action, (int)(repeatTime * 1000), repeatAction);
            if (isRealTime) _timersRealTime.Add(temp.ID, temp);
            else _timers.Add(temp.ID, temp);
            return temp.ID;
        }

        /// <summary>
        /// 暂停计时器
        /// </summary>
        /// <param name="id">计时器ID</param>
        public void PauseTimer(int id)
        {
            if (_timers.TryGetValue(id, out TimerItem timer))
            {
                timer.Active = false;
            }
        }
        
        /// <summary>
        /// 暂停后重新继续计时器
        /// </summary>
        /// <param name="id">计时器ID</param>
        public void ResumeTimer(int id)
        {
            if (_timers.TryGetValue(id, out TimerItem timer))
            {
                timer.Active = true;
            }
        }

        /// <summary>
        /// 移除任意计时器，不论是否受timescale影响
        /// </summary>
        /// <param name="id">计时器ID</param>
        public void RemoveTimer(int id)
        {
            if (!_timers.Remove(id))
            {
                _timersRealTime.Remove(id);
            }
        }
        
        /// <summary>
        /// 内部TimerItem类，不想让外部直接使用
        /// </summary>
        private class TimerItem : IRecyclable
        {
            private static int _count = 0;
            public int ID { get; private set; }
            /// <summary>
            /// 倒计时，单位ms
            /// </summary>
            public int Time;
            /// <summary>
            /// 重复倒计时，单位ms
            /// </summary>
            public int RepeatTime;
            private int _repeatTimeRecord;
            public bool Active;
            public Action DelayAction { get; private set; }
            public Action RepeatAction { get; private set; }

            /// <summary>
            /// 设置时间并开始计时
            /// </summary>
            /// <param name="time">延迟时间 单位ms</param>
            /// <param name="delayAction">延迟执行callback</param>
            /// <param name="repeatTime">重复执行时间间隔 单位ms</param>
            /// <param name="repeatAction">重复执行callback</param>
            public void SetTimeAndStart(int time, Action delayAction, 
                int repeatTime = 0, Action repeatAction = null)
            {
                ID = _count++;
                Time = time;
                RepeatTime = _repeatTimeRecord = repeatTime;
                Active = true;
                DelayAction = delayAction;
                RepeatAction = repeatAction;
            }

            public void RepeatActionInvoke()
            {
                RepeatAction.Invoke();
                RepeatTime = _repeatTimeRecord;
            }

            public void Reset()
            {
                Active = false;
                DelayAction = null;
                RepeatAction = null;
            }
        }
    }
    
    
}