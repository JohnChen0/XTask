﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Tests.Settings
{
    using XTask.Settings;
    using Xunit;

    public class IPropertyTests
    {
        [Fact]
        public void CovarianceTest()
        {
            // We want the base property to be able to be considered covariant so we can aggregate to base types
            IProperty<object> foo = new StringProperty("foo", "bar");
        }
    }
}
