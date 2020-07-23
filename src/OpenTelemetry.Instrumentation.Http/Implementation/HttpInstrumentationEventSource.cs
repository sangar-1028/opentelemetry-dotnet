﻿// <copyright file="HttpInstrumentationEventSource.cs" company="OpenTelemetry Authors">
// Copyright The OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Threading;

namespace OpenTelemetry.Instrumentation.Http.Implementation
{
    /// <summary>
    /// EventSource events emitted from the project.
    /// </summary>
    [EventSource(Name = "OpenTelemetry-Instrumentation-Http")]
    internal class HttpInstrumentationEventSource : EventSource
    {
        public static HttpInstrumentationEventSource Log = new HttpInstrumentationEventSource();

        [NonEvent]
        public void FailedProcessResult(Exception ex)
        {
            if (this.IsEnabled(EventLevel.Error, (EventKeywords)(-1)))
            {
                this.FailedProcessResult(ToInvariantString(ex));
            }
        }

        [NonEvent]
        public void ExceptionInitializingInstrumentation(string instrumentationType, Exception ex)
        {
            if (this.IsEnabled(EventLevel.Error, (EventKeywords)(-1)))
            {
                this.ExceptionInitializingInstrumentation(instrumentationType, ToInvariantString(ex));
            }
        }

        [Event(3, Message = "Payload is NULL in event '{1}' from handler '{0}', span will not be recorded.", Level = EventLevel.Warning)]
        public void NullPayload(string handlerName, string eventName)
        {
            this.WriteEvent(3, handlerName, eventName);
        }

        /// <summary>
        /// Returns a culture-independent string representation of the given <paramref name="exception"/> object,
        /// appropriate for diagnostics tracing.
        /// </summary>
        private static string ToInvariantString(Exception exception)
        {
            var originalUICulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                return exception.ToString();
            }
            finally
            {
                Thread.CurrentThread.CurrentUICulture = originalUICulture;
            }
        }

        [Event(1, Message = "Failed to process result: '{0}'", Level = EventLevel.Error)]
        private void FailedProcessResult(string ex)
        {
            this.WriteEvent(1, ex);
        }

        [Event(2, Message = "Error initializing instrumentation type {0}. Exception : {1}", Level = EventLevel.Error)]
        private void ExceptionInitializingInstrumentation(string instrumentationType, string ex)
        {
            this.WriteEvent(2, instrumentationType, ex);
        }
    }
}
