//-----------------------------------------------------------------------
// <copyright file="NLogLogger.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System.Collections.Generic;
using Akka.Actor;
using Akka.Event;
using Akka.Util.Internal;
using Serilog;

namespace AkkaSemanticLogger
{
    public class SemanticLogger : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private string GetFormat(object message)
        {

            var eventBase = message as LogEvent;
            if (eventBase != null)
            {
                var format = "{Message}";
                var logMessage = eventBase.Message as LogMessage;
                if (logMessage != null)
                {
                    format = TemplateTransform.Transform(logMessage.Format);

                }


                return "[{Origin}][Thread {Thread}][{LogSource}] " + format;
            }

            return message.ToString();
        }

        
        private object[] GetArguments(object message)
        {
            var eventBase = message as LogEvent;
            if (eventBase != null)
            {
                var format = new[] {eventBase.Message};
                var logMessage = eventBase.Message as LogMessage;
                if (logMessage != null)
                {
                    format = logMessage.Args;
                }

                var address = Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.DefaultAddress;
                var origin = $"{address?.System}@{address?.Host}:{address?.Port}";
                var args = new List<object>()
                {
                    origin,
                    eventBase.Thread.ManagedThreadId.ToString().PadLeft(4, '0'),
                    eventBase.LogSource,
                };
                args.AddRange(format);
                var res = args.ToArray();
                return res;
            }
            return new object[0];
        }

        public SemanticLogger()
        {
            Receive<Error>(m => {
                Log.Error(m.Cause, GetFormat(m), GetArguments(m));
            });
            Receive<Warning>(m => {
                Log.Warning(GetFormat(m), GetArguments(m));
            });
            Receive<Info>(m =>
            {
                Log.Information(GetFormat(m), GetArguments(m));
            });
            Receive<Debug>(m =>
            {
                Log.Debug(GetFormat(m), GetArguments(m));
            });
            Receive<InitializeLogger>(m =>
            {
                _log.Info("NLogLogger started");
                Sender.Tell(new LoggerInitialized());
            });
        }
    }
}

