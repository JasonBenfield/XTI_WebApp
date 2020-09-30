﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XTI_App.Api
{
    public interface ValueTemplate
    {
        Type DataType { get; }
        IEnumerable<ObjectValueTemplate> ObjectTemplates();
    }
    public sealed class SimpleValueTemplate : ValueTemplate, IEquatable<SimpleValueTemplate>
    {
        public SimpleValueTemplate(Type dataType, bool isNullable)
        {
            DataType = dataType;
            IsNullable = isNullable;
            value = $"{DataType}|{isNullable}";
            hashCode = value.GetHashCode();
        }

        private readonly string value;
        private readonly int hashCode;

        public Type DataType { get; }
        public bool IsNullable { get; }

        public IEnumerable<ObjectValueTemplate> ObjectTemplates() => new ObjectValueTemplate[] { };

        public override bool Equals(object obj) => Equals(obj as SimpleValueTemplate);

        public bool Equals(SimpleValueTemplate other) => value == other?.value;

        public override int GetHashCode() => hashCode;

        public override string ToString() => $"{nameof(SimpleValueTemplate)} {value}";
    }
    public sealed class ObjectValueTemplate : ValueTemplate, IEquatable<ObjectValueTemplate>
    {
        public ObjectValueTemplate(Type dataType)
        {
            DataType = dataType;
            PropertyTemplates = dataType.GetProperties().Select(p => new ObjectPropertyTemplate(p));
            var str = string.Join(";", PropertyTemplates);
            value = $"{DataType}|{str}";
            hashCode = value.GetHashCode();
        }

        private readonly string value;
        private readonly int hashCode;

        public Type DataType { get; }
        public IEnumerable<ObjectPropertyTemplate> PropertyTemplates { get; }

        public IEnumerable<ObjectValueTemplate> ObjectTemplates()
        {
            return new ObjectValueTemplate[] { this }
                .Union
                (
                    PropertyTemplates.SelectMany(pt => pt.ValueTemplate.ObjectTemplates())
                )
                .Distinct();
        }

        public override bool Equals(object obj) => Equals(obj as ObjectValueTemplate);

        public bool Equals(ObjectValueTemplate other) => value == other?.value;

        public override int GetHashCode() => hashCode;
    }
    public sealed class ObjectPropertyTemplate : IEquatable<ObjectPropertyTemplate>
    {
        public ObjectPropertyTemplate(PropertyInfo propertyInfo)
        {
            Name = propertyInfo.Name;
            ValueTemplate = new ValueTemplateFromType(propertyInfo.PropertyType).Template();
            CanRead = propertyInfo.CanRead;
            CanWrite = propertyInfo.CanWrite;
            value = $"{Name}|{ValueTemplate}|{CanRead}|{CanWrite}";
            hashCode = value.GetHashCode();
        }

        public ObjectPropertyTemplate()
        {
        }

        private readonly string value;
        private readonly int hashCode;

        public string Name { get; }
        public ValueTemplate ValueTemplate { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }

        public override string ToString() => $"{nameof(ObjectPropertyTemplate)} {value}";

        public override bool Equals(object obj) => Equals(obj as ObjectPropertyTemplate);

        public bool Equals(ObjectPropertyTemplate other) => value == other?.value;

        public override int GetHashCode() => hashCode;
    }
    public sealed class ArrayValueTemplate : ValueTemplate, IEquatable<ArrayValueTemplate>
    {
        public ArrayValueTemplate(Type source)
        {
            DataType = source;
            var elType = getEnumerableElementType(source);
            ElementTemplate = new ValueTemplateFromType(elType).Template();
            value = $"{DataType}|{ElementTemplate}";
            hashCode = value.GetHashCode();
        }

        private readonly string value;
        private readonly int hashCode;

        private static Type getEnumerableElementType(Type type)
        {
            Type elementType;
            if (type.IsArray)
            {
                elementType = type.GetElementType();
            }
            else
            {
                elementType = type.GetGenericArguments()[0];
            }
            return elementType;
        }

        public Type DataType { get; }
        public ValueTemplate ElementTemplate { get; }

        public IEnumerable<ObjectValueTemplate> ObjectTemplates() => ElementTemplate.ObjectTemplates();

        public override bool Equals(object obj) => Equals(obj as ArrayValueTemplate);

        public bool Equals(ArrayValueTemplate other) => value == other?.value;

        public override int GetHashCode() => hashCode;

        public override string ToString() => $"{nameof(ArrayValueTemplate)} {value}";

    }
}
