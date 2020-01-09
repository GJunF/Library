using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Utils
{
    public class NoticeChecker
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(NoticeChecker));

        Func<int, bool> callback;//一个 Callback 委托，表示要执行的方法。

        int maxCount;//确定回调方法应调用的最大次数。
        int dueTime;//调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。
        int period;//调用 callback 的时间间隔（以毫秒为单位）。

        /// <summary>
        /// 初始化异步通知类
        /// </summary>
        /// <param name="callback">一个 NoticeCallback 委托，表示要执行的方法。</param>
        /// <param name="maxNoticeCount">执行 callback 方法失败的情况下的最大执行次数。</param>
        /// <param name="dueTime">调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。</param>
        /// <param name="period">调用 callback 的时间间隔（以毫秒为单位）。</param>
        public NoticeChecker(Func<int, bool> callback, int maxNoticeCount, int dueTime, int period)
        {
            this.callback = callback;
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
        public void Notice()
        {
            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            var statusChecker = new StatusChecker(maxCount, callback);

            // Create a timer that invokes CheckStatus after one second, 
            // and every 1/4 second thereafter.
            _Log.DebugFormat("{0:HH:mm:ss.fff} Creating timer.", DateTime.Now);
            var stateTimer = new Timer(statusChecker.CheckStatus, autoEvent, dueTime, period);

            // When autoEvent signals the second time, dispose of the timer.
            autoEvent.WaitOne();
            stateTimer.Dispose();
            _Log.DebugFormat("Destroying timer.");
        }
    }
    public class NoticeChecker<T>
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(NoticeChecker<T>));

        Func<int, T, bool> callback;//一个 Callback 委托，表示要执行的方法。

        int maxCount;//确定回调方法应调用的最大次数。
        int dueTime;//调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。
        int period;//调用 callback 的时间间隔（以毫秒为单位）。

        /// <summary>
        /// 初始化异步通知类
        /// </summary>
        /// <param name="callback">一个 NoticeCallback 委托，表示要执行的方法。</param>
        /// <param name="maxNoticeCount">执行 callback 方法失败的情况下的最大执行次数。</param>
        /// <param name="dueTime">调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。</param>
        /// <param name="period">调用 callback 的时间间隔（以毫秒为单位）。</param>
        public NoticeChecker(Func<int, T, bool> callback, int maxNoticeCount, int dueTime, int period)
        {
            this.callback = callback;
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
        /// <param name="arg1">执行 callback 方法时传入的参数。</param>
        public void Notice(T arg1)
        {
            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            var statusChecker = new StatusChecker<T>(maxCount, callback, arg1);

            // Create a timer that invokes CheckStatus after one second, 
            // and every 1/4 second thereafter.
            _Log.DebugFormat("{0:HH:mm:ss.fff} Creating timer.", DateTime.Now);
            var stateTimer = new Timer(statusChecker.CheckStatus, autoEvent, dueTime, period);

            // When autoEvent signals the second time, dispose of the timer.
            autoEvent.WaitOne();
            stateTimer.Dispose();
            _Log.DebugFormat("Destroying timer.");
        }
    }
    public class NoticeChecker<T1, T2>
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(NoticeChecker<T1, T2>));

        Func<int, T1, T2, bool> callback;//一个 Callback 委托，表示要执行的方法。

        int maxCount;//确定回调方法应调用的最大次数。
        int dueTime;//调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。
        int period;//调用 callback 的时间间隔（以毫秒为单位）。

        /// <summary>
        /// 初始化异步通知类
        /// </summary>
        /// <param name="callback">一个 NoticeCallback 委托，表示要执行的方法。</param>
        /// <param name="maxNoticeCount">执行 callback 方法失败的情况下的最大执行次数。</param>
        /// <param name="dueTime">调用 callback 之前延迟的时间量（以毫秒为单位）。指定零 (0) 可立即启动计时器。</param>
        /// <param name="period">调用 callback 的时间间隔（以毫秒为单位）。</param>
        public NoticeChecker(Func<int, T1, T2, bool> callback, int maxNoticeCount, int dueTime, int period)
        {
            this.callback = callback;
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
        /// <param name="arg1">执行 callback 方法时传入的参数。</param>
        /// <param name="arg2">执行 callback 方法时传入的参数。</param>
        public void Notice(T1 arg1, T2 arg2)
        {

            // Create an AutoResetEvent to signal the timeout threshold in the
            // timer callback has been reached.
            var autoEvent = new AutoResetEvent(false);

            var statusChecker = new StatusChecker<T1, T2>(maxCount, callback, arg1, arg2);

            // Create a timer that invokes CheckStatus after one second, 
            // and every 1/4 second thereafter.
            _Log.DebugFormat("{0:HH:mm:ss.fff} Creating timer.", DateTime.Now);
            var stateTimer = new Timer(statusChecker.CheckStatus, autoEvent, dueTime, period);

            // When autoEvent signals the second time, dispose of the timer.
            autoEvent.WaitOne();
            stateTimer.Dispose();
            _Log.DebugFormat("Destroying timer.");
        }
    }

    class StatusChecker
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(StatusChecker));
        private int invokeCount;//指示回调方法被调用的次数。
        private int maxCount;//确定回调方法应调用的最大次数。
        private Func<int, bool> func;//一个 Callback 委托，表示要执行的方法。

        public StatusChecker(int count, Func<int, bool> callback)
        {
            invokeCount = 0;
            maxCount = count;
            func = callback;
        }

        // This method is called by the timer delegate.
        public void CheckStatus(object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            ++invokeCount;

            bool result = false;

            if (func != null)
                result = func(invokeCount);

            _Log.DebugFormat("{0} Checking status {1}.", DateTime.Now.ToString("HH:mm:ss.fff"), invokeCount);

            if (result || invokeCount == maxCount)
            {
                // Reset the counter and signal the waiting thread.
                invokeCount = 0;
                autoEvent.Set();
            }
        }
    }
    class StatusChecker<T>
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(StatusChecker));
        private int invokeCount;//指示回调方法被调用的次数。
        private int maxCount;//确定回调方法应调用的最大次数。
        private Func<int, T, bool> func;//一个 Callback 委托，表示要执行的方法。
        private T arg1;

        public StatusChecker(int count, Func<int, T, bool> callback, T arg1)
        {
            invokeCount = 0;
            maxCount = count;
            func = callback;
            this.arg1 = arg1;
        }

        // This method is called by the timer delegate.
        public void CheckStatus(object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            ++invokeCount;

            bool result = false;

            if (func != null)
                result = func(invokeCount, arg1);

            _Log.DebugFormat("{0} Checking status {1}.", DateTime.Now.ToString("HH:mm:ss.fff"), invokeCount);

            if (result || invokeCount == maxCount)
            {
                // Reset the counter and signal the waiting thread.
                invokeCount = 0;
                autoEvent.Set();
            }
        }
    }
    class StatusChecker<T1, T2>
    {
        static log4net.ILog _Log = log4net.LogManager.GetLogger(typeof(StatusChecker));
        private int invokeCount;//指示回调方法被调用的次数。
        private int maxCount;//确定回调方法应调用的最大次数。
        private Func<int, T1, T2, bool> func;//一个 Callback 委托，表示要执行的方法。
        private T1 arg1;
        private T2 arg2;

        public StatusChecker(int count, Func<int, T1, T2, bool> callback, T1 arg1, T2 arg2)
        {
            invokeCount = 0;
            maxCount = count;
            func = callback;
            this.arg1 = arg1;
            this.arg2 = arg2;
        }

        // This method is called by the timer delegate.
        public void CheckStatus(object stateInfo)
        {
            AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;
            ++invokeCount;

            bool result = false;

            if (func != null)
                result = func(invokeCount, arg1, arg2);

            _Log.DebugFormat("{0} Checking status {1}.", DateTime.Now.ToString("HH:mm:ss.fff"), invokeCount);

            if (result || invokeCount == maxCount)
            {
                // Reset the counter and signal the waiting thread.
                invokeCount = 0;
                autoEvent.Set();
            }
        }
    }
}
