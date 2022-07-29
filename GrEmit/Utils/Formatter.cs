using System;
using System.Linq;
using System.Reflection;

namespace GrEmit.Utils
{
    public static class Formatter
    {
        public static string Format(Type type)
        {
            if (type == null)
            {
                return "null";
            }

            if (type.IsByRef)
            {
                return Format(type.GetElementType()) + "&";
            }

            if (type.IsPointer)
            {
                return Format(type.GetElementType()) + "*";
            }

            if (type.IsArray)
            {
                int rank = type.GetArrayRank();
                return Format(type.GetElementType()) + $"[{(rank == 1 ? "" : new string(',', rank - 1))}]";
            }
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            int index = type.Name.LastIndexOf('`');
            return (index < 0 ? type.Name : type.Name.Substring(0, index)) + "<" + string.Join(", ", type.GetGenericArguments().Select(Format).ToArray()) + ">";
        }

        public static string Format(FieldInfo field)
        {
            return Format(field.ReflectedType) + "." + field.Name;
        }

        public static string Format(ConstructorInfo constructor)
        {
            return Format(constructor.ReflectedType) + ".ctor" + "(" + string.Join(", ", ReflectionExtensions.GetParameterTypes(constructor).Select(Format).ToArray()) + ")";
        }

        public static string Format(MethodInfo method)
        {
            if (ReferenceEquals(method.ReflectedType, null))
            {
                return Format(ReflectionExtensions.GetReturnType(method)) + " " + FormatMethodWithoutParameters(method) + "(" + string.Join(", ", ReflectionExtensions.GetParameterTypes(method).Select(Format).ToArray()) + ")";
            }

            return Format(ReflectionExtensions.GetReturnType(method)) + " " + Format(method.ReflectedType) + "." + FormatMethodWithoutParameters(method) + "(" + string.Join(", ", ReflectionExtensions.GetParameterTypes(method).Select(Format).ToArray()) + ")";
        }

        internal static string Format(ESType esType)
        {
            SimpleESType simpleESType = esType as SimpleESType;
            if (simpleESType != null)
            {
                return Format(simpleESType.Type);
            }

            ComplexESType complexESType = (ComplexESType)esType;
            if (complexESType.BaseType == typeof(object) && complexESType.Interfaces.Length == 1)
            {
                return Format(complexESType.Interfaces.Single());
            }

            return "{" + Format(complexESType.BaseType) + ": " + string.Join(", ", complexESType.Interfaces.Select(Format).ToArray()) + "}";
        }

        private static string FormatMethodWithoutParameters(MethodInfo method)
        {
            if (!method.IsGenericMethod)
            {
                return method.Name;
            }

            return method.Name + "<" + string.Join(", ", method.GetGenericArguments().Select(Format).ToArray()) + ">";
        }
    }
}