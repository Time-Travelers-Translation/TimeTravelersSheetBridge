﻿using Castle.DynamicProxy;
using CrossCutting.Core.Contract.Configuration;
using CrossCutting.Core.Contract.Configuration.DataClasses;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CrossCutting.Core.Configuration.ConfigObjects
{
    public class ConfigObjectInterceptor : IInterceptor
    {
        private readonly IConfigurator _configurator;

        private readonly IDictionary<Type, object> _defaultInstanceCache;

        public ConfigObjectInterceptor(IConfigurator configurator)
        {
            _configurator = configurator ?? throw new ArgumentNullException(nameof(configurator));
            _defaultInstanceCache = new Dictionary<Type, object>();
        }

        public void Intercept(IInvocation invocation)
        {
            if (!invocation.Method.Name.StartsWith("get_"))
            {
                invocation.Proceed();
                return;
            }

            Type originalType = invocation.TargetType;

            string propertyName = invocation.Method.Name.Split('_')[1];
            PropertyInfo propertyInfo = originalType.GetProperty(propertyName)!;

            if (!_defaultInstanceCache.TryGetValue(originalType, out object defaultInstance))
                _defaultInstanceCache[originalType] = defaultInstance = Activator.CreateInstance(originalType);

            var attribute = propertyInfo.GetCustomAttribute<ConfigMapAttribute>();
            if (attribute == null)
                return;

            foreach (string key in attribute.Keys)
            {
                if (!_configurator.Contains(attribute.Category, key))
                    continue;

                var value = _configurator.Get<object>(attribute.Category, key);
                if (propertyInfo.PropertyType.IsEnum)
                    invocation.ReturnValue = Enum.Parse(propertyInfo.PropertyType, (string)value);
                else
                {
                    if (!value.GetType().IsArray && propertyInfo.PropertyType.IsArray)
                    {
                        Type? arrayType = propertyInfo.PropertyType.GetElementType();
                        if (arrayType == null) 
                            return;

                        var array = Array.CreateInstance(arrayType, 1);
                        array.SetValue(Convert.ChangeType(value, arrayType), 0);

                        invocation.ReturnValue = array;
                    }
                    else
                        invocation.ReturnValue = Convert.ChangeType(value, propertyInfo.PropertyType);
                }

                return;
            }

            invocation.ReturnValue = propertyInfo.GetValue(defaultInstance);
        }
    }
}
