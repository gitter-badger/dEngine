// ReflectionExtensions.cs - dEngine
// Copyright (C) 2016-2016 DanDev (dandev.sco@gmail.com)
// 
// This library is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// You should have received a copy of the GNU Lesser General Public
// along with this program. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Dynamitey;

namespace dEngine.Utility.Extensions
{
    internal static class ReflectionExtensions
    {
        private static readonly Dictionary<Type, Func<object>> _constructorCache = new Dictionary<Type, Func<object>>();

        public static object FastConstruct(this Type type)
        {
            Func<object> constructor;
            if (!_constructorCache.TryGetValue(type, out constructor))
            {
                var lambda = Expression.Lambda<Func<object>>(Expression.New(type.GetConstructor(Type.EmptyTypes)));
                constructor = lambda.Compile();
                _constructorCache[type] = constructor;
            }
            return constructor.FastDynamicInvoke();
        }

        public static Expression<Func<object>> GetCacheableMethod(this Type type, string name, BindingFlags flags)
        {
            var method = type.GetMethod(name, flags);
            return () => method.Invoke(null, new object[] { });
        }

        public static Expression<Func<object, object>> GetValueGetter(this PropertyInfo propertyInfo)
        {
            var getter = propertyInfo.GetGetMethod(true);
            return (obj) => getter.Invoke(obj, new object[] { });
        }

        public static Expression<Action<object, object>> GetValueSetter(this PropertyInfo propertyInfo)
        {
            var setter = propertyInfo.GetSetMethod(true);
            return (obj, val) => setter.Invoke(obj, new[] { val });
        }

        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}