//-----------------------------------------------------------------------
// <copyright file="NLogLogger.cs" company="Akka.NET Project">
//     Copyright (C) 2009-2015 Typesafe Inc. <http://www.typesafe.com>
//     Copyright (C) 2013-2015 Akka.NET project <https://github.com/akkadotnet/akka.net>
// </copyright>
//-----------------------------------------------------------------------

using System;
using Akka.Actor;
using Akka.Event;
using Serilog;

namespace AkkaSemanticLogger
{
    public class SemanticLogger : ReceiveActor
    {
        private readonly ILoggingAdapter _log = Context.GetLogger();

        private string GetFormat(object message)
        {
            var logMessage = message as LogMessage;
            if (logMessage != null)
            {
                return TemplateTransform.Transform(logMessage.Format);
            }
            var eventBase = message as LogEvent;
            if (eventBase != null)
            {
                return "[{LogLevel}][{Timestamp}][Thread {Thread}][{LogSource}] {Message}";
            }

            return message.ToString();
        }

        private object[] GetArguments(object message)
        {
            var logMessage = message as LogMessage;
            if (logMessage != null)
            {
                return logMessage.Args;
            }
            var eventBase = message as LogEvent;
            if (eventBase != null)
            {
                return new[]
                {
                    eventBase.LogLevel().ToString().Replace("Level", "").ToUpperInvariant(),
                    eventBase.Timestamp,
                    eventBase.Thread.ManagedThreadId.ToString().PadLeft(4, '0'),
                    eventBase.LogSource,
                    eventBase.Message
                };
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

