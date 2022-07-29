using System;
using System.Reflection;
using System.Reflection.Emit;

namespace GrEmit.Utils
{
    //TODO test
    public class EmitHelpers
    {
        public static T CreateDelegate<T>(DynamicMethod dm, object target) where T : class
        {
            Delegate @delegate = dm.CreateDelegate(typeof(T), target);
            T result = (@delegate as T);
            if (result == null)
            {
                throw new ArgumentException($"Type {typeof(T)} is not a delegate");
            }

            return result;
        }

        public static T CreateDelegate<T>(DynamicMethod dm) where T : class
        {
            Delegate @delegate = dm.CreateDelegate(typeof(T));
            T result = (@delegate as T);
            if (result == null)
            {
                throw new ArgumentException($"Type {typeof(T)} is not a delegate");
            }

            return result;
        }

        public static T EmitDynamicMethod<T>(string name, Module m, Action<GroboIL> emitCode) where T : class
        {
            Type delegateType = typeof(T);
            //HACK
            MethodInfo methodInfo = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo == null)
            {
                throw new ArgumentException($"Type {delegateType} is not a Delegate");
            }

            DynamicMethod dynamicMethod = new DynamicMethod(name, methodInfo.ReturnType, ReflectionExtensions.GetParameterTypes(methodInfo), m, true);
            using (GroboIL il = new GroboIL(dynamicMethod))
            {
                emitCode(il);
            }

            return CreateDelegate<T>(dynamicMethod);
        }

        public static T EmitDynamicMethod<T, TTarget>(string name, Module m, Action<GroboIL> emitCode, TTarget target) where T : class
        {
            Type delegateType = typeof(T);
            //HACK
            MethodInfo methodInfo = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            if (methodInfo == null)
            {
                throw new ArgumentException($"Type {delegateType} is not a Delegate");
            }

            DynamicMethod dynamicMethod = new DynamicMethod(name, methodInfo.ReturnType, Concat(typeof(TTarget), ReflectionExtensions.GetParameterTypes(methodInfo)), m, true);
            using (GroboIL il = new GroboIL(dynamicMethod))
            {
                emitCode(il);
            }

            return CreateDelegate<T>(dynamicMethod, target);
        }

        private static Type[] Concat(Type a, Type[] b)
        {
            Type[] result = new Type[b.Length + 1];
            result[0] = a;
            Array.Copy(b, 0, result, 1, b.Length);
            return result;
        }
    }
}