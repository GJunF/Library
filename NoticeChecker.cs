using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utils
{
    public class NoticeChecker<T>
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(NoticeChecker<T>));
        NoticeCallback<T> callback;//一个 NoticeCallback 委托，表示要执行的方法。
        T callbackArgs;//执行 callback 方法时传入的参数。
        int invokeCount;//当前执行 callback 方法的次数。
        int maxCount;//执行 callback 方法失败的情况下的最大执行次数。
        int dueTime;//调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。
        int period;//调用 callback 的时间间隔（以毫秒为单位）。
        /// <summary>
        /// 初始化异步通知类
        /// </summary>
        /// <param name="callback">一个 NoticeCallback 委托，表示要执行的方法。</param>
        /// <param name="maxNoticeCount">执行 callback 方法失败的情况下的最大执行次数。</param>
        /// <param name="dueTime">调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。</param>
        /// <param name="period">调用 callback 的时间间隔（以毫秒为单位）。</param>
        public NoticeChecker(NoticeCallback<T> callback, int maxNoticeCount, int dueTime, int period)
        {
            this.callback = callback;
            invokeCount = 0;
            maxCount = maxNoticeCount;
            this.dueTime = dueTime;
            this.period = period;
        }
        /// <summary>
        /// 启动异步任务，调用 callback 方法。
        /// callback 方法会在调用该方法后 dueTime 毫秒后被调用。
        /// 如果调用 callback 方法，返回 false，会在间隔 period 毫秒后再次调用 callback 方法。
        /// 最大调用次数为 maxNoticeCount 次。
        /// </summary>
        /// <param name="args">执行 callback 方法时传入的参数。</param>
        public void Notice(T args)
        {
            callbackArgs = args;
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                System.Threading.AutoResetEvent autoEvent = new System.Threading.AutoResetEvent(false);
                System.Threading.TimerCallback timerDelegate = new System.Threading.TimerCallback((Object stateInfo) => {
                    AutoResetEvent autoResetEvent = (AutoResetEvent)stateInfo;
                    if (invokeCount == maxCount)
                    {
                        invokeCount = 0;
                        autoResetEvent.Set();
                    }
                    else
                    {
                        ++invokeCount;
                        bool result = false;

                        if (callback != null)
                            result = callback(callbackArgs, invokeCount);

                        if (result)
                        {
                            invokeCount = 0;
                            autoResetEvent.Set();
                        }
                    }
                });
                _Log.DebugFormat("{0:h:mm:ss.fff} Creating timer.\n", DateTime.Now);
                System.Threading.Timer stateTimer = new System.Threading.Timer(timerDelegate, autoEvent, dueTime, period);
                autoEvent.WaitOne();
                stateTimer.Dispose();
                _Log.DebugFormat("\nDestroying timer.");
            });
        }
    }
    public delegate bool NoticeCallback<T>(T state, int invokeCount);
}
