﻿// ----------------------
//    xTask Framework
// ----------------------

// Copyright (c) Jeremy W. Kuhne. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace XTask.Services
{
    using System;
    using System.Collections.Generic;

    public class SimpleServiceProvider : ITypedServiceProvider
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<T>(T service) where T : class
        {
            _services.Add(typeof(T), service);
        }

        public T GetService<T>() where T : class
        {
            object value;
            _services.TryGetValue(typeof(T), out value);
            return value as T;
        }
    }
}
