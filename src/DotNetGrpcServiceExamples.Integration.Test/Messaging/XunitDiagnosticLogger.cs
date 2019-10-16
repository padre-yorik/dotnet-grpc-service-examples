using System;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace NewVoiceMedia.DotNetGrpcServiceExamples.Integration.Test.Messaging
{
    public class XunitDiagnosticLogger<T> : ILogger<T>
    {
        private readonly IMessageSink _diagnosticMessageSink;

        public XunitDiagnosticLogger(IMessageSink diagnosticMessageSink)
        {
            _diagnosticMessageSink = diagnosticMessageSink;
        }
        
        public IDisposable BeginScope<TState>(TState state)
            => throw new NotImplementedException();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Log(logLevel, formatter(state, exception));

        public void Log(LogLevel logLevel, string message)
        {
            _diagnosticMessageSink.OnMessage(new DiagnosticMessage($"{typeof(T)} {logLevel}: {message}"));
        }
    }
}