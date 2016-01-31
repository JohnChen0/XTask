﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tasks
{
    using Services;
    using System;
    using System.Collections.Generic;
    using XTask.Logging;
    using XTask.Settings;

    public sealed class ConsoleTaskInteraction : TaskInteraction, IDisposable
    {
        private Lazy<ConsoleTaskLoggers> _loggers;

        private ConsoleTaskInteraction(ITask task, IArgumentProvider arguments, ITypedServiceProvider services)
            : base (arguments, services)
        {
            _loggers = new Lazy<ConsoleTaskLoggers>(() => new ConsoleTaskLoggers(task, arguments));
        }

        public static ITaskInteraction Create(ITask task, IArgumentProvider arguments, ITypedServiceProvider services)
        {
            return new ConsoleTaskInteraction(task, arguments, services);
        }

        protected override ILoggers GetDefaultLoggers()
        {
            return _loggers.Value;
        }

        private sealed class ConsoleTaskLoggers : Loggers, IDisposable
        {
            private RichTextLogger _richTextLogger;
            private TextLogger _textLogger;
            private CsvLogger _csvLogger;
            private XmlSpreadsheetLogger _spreadsheetLogger;
            private AggregatedLogger _aggregatedLogger;

            public ConsoleTaskLoggers(ITask task, IArgumentProvider arguments)
            {
                if (arguments.GetOption<bool?>(StandardOptions.Clipboard) ?? task.GetOptionDefault<bool>(StandardOptions.Clipboard[0]))
                {
                    _richTextLogger = new RichTextLogger();
                    _csvLogger = new CsvLogger();
                    _textLogger = new TextLogger();
                    _spreadsheetLogger = new XmlSpreadsheetLogger();
                    _aggregatedLogger = new AggregatedLogger(
                        ConsoleLogger.Instance,
                        _richTextLogger,
                        _spreadsheetLogger,
                        _csvLogger,
                        _textLogger);

                    RegisterLogger(LoggerType.Result, _aggregatedLogger);
                }
                else
                {
                    RegisterLogger(LoggerType.Result, ConsoleLogger.Instance);
                }

                RegisterLogger(LoggerType.Status, ConsoleLogger.Instance);
            }

            public void Dispose()
            {
                if (_aggregatedLogger != null)
                {
                    List<ClipboardData> allData = new List<ClipboardData>();
                    allData.Add(_richTextLogger.GetClipboardData());
                    allData.Add(_textLogger.GetClipboardData());
                    allData.Add(_csvLogger.GetClipboardData());
                    allData.Add(_spreadsheetLogger.GetClipboardData());

                    Clipboard.AddToClipboard(allData.ToArray());
                    _richTextLogger = null;

                    _csvLogger.Dispose();
                    _spreadsheetLogger.Dispose();
                }
            }
        }

        public void Dispose()
        {
            if (_loggers.IsValueCreated) _loggers.Value.Dispose();
            _loggers = null;
        }
    }
}