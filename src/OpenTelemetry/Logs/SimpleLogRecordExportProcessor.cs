// <copyright file="SimpleLogRecordExportProcessor.cs" company="OpenTelemetry Authors">
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

#nullable enable

using OpenTelemetry.Logs;

namespace OpenTelemetry
{
    /// <summary>
    /// Implements a simple log record export processor.
    /// </summary>
    public class SimpleLogRecordExportProcessor : SimpleExportProcessor<LogRecord>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLogRecordExportProcessor"/> class.
        /// </summary>
        /// <param name="exporter">Log record exporter.</param>
        public SimpleLogRecordExportProcessor(BaseExporter<LogRecord> exporter)
            : base(exporter)
        {
        }
    }
}
