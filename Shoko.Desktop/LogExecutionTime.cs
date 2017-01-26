using System;
using System.Diagnostics;
using System.Reflection;
using NLog;
#if MEASURE_EXECUTION_TIME

using PostSharp.Aspects;
using PostSharp.Extensibility;
#endif
namespace JMMClient
{
#if MEASURE_EXECUTION_TIME
    [Serializable]
    [DebuggerStepThrough]
    public class LogExecutionTime : MethodInterceptionAspect
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public override void OnInvoke(MethodInterceptionArgs args)
        {
            Stopwatch sw=new Stopwatch();
            sw.Start();
            base.OnInvoke(args);
            sw.Stop();
            logger.Trace("Method [{0}{1}] took [{2}] milliseconds to execute", args.Method?.DeclaringType?.Name ?? string.Empty, args.Method?.Name ?? string.Empty,
                sw.ElapsedMilliseconds);
        }
    }
#else
    public class LogExecutionTime : Attribute
    {
    }
#endif
}
