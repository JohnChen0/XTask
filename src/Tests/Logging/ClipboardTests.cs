﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Logging
{
    using System;
    using FluentAssertions;
    using XTask.Logging;
    using Xunit;

    public class ClipboardTests
    {
        [Theory,
            InlineData(ClipboardFormat.CommaSeparatedValues, "Csv"),
            InlineData(ClipboardFormat.Html, "HTML Format"),
            InlineData(ClipboardFormat.RichText, "Rich Text Format"),
            InlineData(ClipboardFormat.XmlSpreadsheet, "Xml Spreadsheet"),
            InlineData(ClipboardFormat.Text, "Text")
            ]
        public void TestFormatToString(ClipboardFormat format, string expected)
        {
            Clipboard.GetDataObjectFormatString(format).Should().Be(expected);
        }

        [Fact]
        public void AddToClipboardThrowsArgumentNull()
        {
            Action action = () => Clipboard.AddToClipboard(null);
            action.ShouldThrow<ArgumentNullException>();
        }
    }
}
