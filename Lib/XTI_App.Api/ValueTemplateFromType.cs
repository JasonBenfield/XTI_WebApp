﻿using System;
using System.Collections.Generic;

namespace XTI_App.Api
{
    public class ValueTemplateFromType
    {
        public ValueTemplateFromType(Type source)
        {
            this.source = source;
        }

        private readonly Type source;

        public ValueTemplate Template()
        {
            ValueTemplate valueTemplate;
            var isNullable = isNullableType(source);
            if (isNullable || source.IsValueType)
            {
                if (isNullable)
                {
                    var underlyingType = getNullableType(source);
                    valueTemplate = new SimpleValueTemplate(underlyingType, true);
                }
                else
                {
                    valueTemplate = new SimpleValueTemplate(source, false);
                }
            }
            else if (source == typeof(string))
            {
                valueTemplate = new SimpleValueTemplate(source, true);
            }
            else if (isArrayOrEnumerable(source))
            {
                valueTemplate = new ArrayValueTemplate(source);
            }
            else
            {
                valueTemplate = new ObjectValueTemplate(source);
            }
            return valueTemplate;
        }

        private static bool isNullableType(Type targetType)
        {
            return targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static Type getNullableType(Type targetType)
        {
            return targetType.GetGenericArguments()[0];
        }

        private static bool isArrayOrEnumerable(Type targetType)
        {
            return targetType != typeof(string) && (targetType.IsArray || isEnumerable(targetType));
        }

        private static bool isEnumerable(Type targetType)
        {
            return targetType.IsGenericType && (typeof(IEnumerable<>)).IsAssignableFrom(targetType.GetGenericTypeDefinition());
        }

    }
}