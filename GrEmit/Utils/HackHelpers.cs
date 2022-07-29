using System;
using System.Linq.Expressions;
using System.Reflection;

namespace GrEmit.Utils
{
    public static class HackHelpers
    {
        public static Type GetValueTypeForNullableOrNull(Type mayBeNullable)
        {
            if (!mayBeNullable.IsGenericType || mayBeNullable.IsGenericTypeDefinition ||
                mayBeNullable.GetGenericTypeDefinition() != typeof(Nullable<>))
            {
                return null;
            }

            Type valueType = mayBeNullable.GetGenericArguments()[0];
            return valueType;
        }

        //NOTE MetadataToken's reassigned on compilation !!! use inside appdomain
        public static ulong GetMemberUniqueToken(MemberInfo mi)
        {
            return ((ulong)mi.Module.MetadataToken) << 32 | (ulong)mi.MetadataToken;
        }

        //BUG может быть одинаков для типов из разных сборок
        public static ulong GetTypeUniqueToken(Type type)
        {
            return ((ulong)type.Module.MetadataToken) << 32 | (ulong)type.MetadataToken;
        }

        public static ConstructorInfo GetObjectConstruction<T>(Expression<Func<T>> constructorCall,
                                                               params Type[] classGenericArgs)
        {
            Expression expression = constructorCall.Body;
            ConstructorInfo sourceCi = ObjectConstruction(EliminateConvert(expression));
            if (typeof(T).IsValueType && sourceCi == null)
            {
                throw new NotSupportedException("Struct creation without arguments");
            }

            Type type = sourceCi.ReflectedType;
            Type resultReflectedType = type.IsGenericType && classGenericArgs != null && classGenericArgs.Length > 0
                                          ? type.GetGenericTypeDefinition().MakeGenericType(classGenericArgs)
                                          : type;
            MethodBase methodBase = MethodBase.GetMethodFromHandle(
                sourceCi.MethodHandle, resultReflectedType.TypeHandle);
            ConstructorInfo constructedCtorForResultType = (ConstructorInfo)methodBase;
            return constructedCtorForResultType;
        }

        public static MethodInfo ConstructMethodDefinition<T>(Expression<Action<T>> callExpr, Type[] methodGenericArgs)
        {
            MethodCallExpression methodCallExpression = (MethodCallExpression)callExpr.Body;
            MethodInfo methodInfo = methodCallExpression.Method;
            if (!methodInfo.IsGenericMethod)
            {
                return methodInfo;
            }

            return methodInfo.GetGenericMethodDefinition().MakeGenericMethod(methodGenericArgs);
        }

        public static MethodInfo ConstructStaticMethodDefinition(Expression<Action> callExpr, Type[] methodGenericArgs)
        {
            MethodCallExpression methodCallExpression = (MethodCallExpression)callExpr.Body;
            MethodInfo methodInfo = methodCallExpression.Method;
            if (!methodInfo.IsGenericMethod)
            {
                return methodInfo;
            }

            return methodInfo.GetGenericMethodDefinition().MakeGenericMethod(methodGenericArgs);
        }

        public static MethodInfo GetMethodDefinition<T>(Expression<Action<T>> callExpr)
        {
            MethodCallExpression methodCallExpression = (MethodCallExpression)callExpr.Body;
            return methodCallExpression.Method;
        }

        public static MethodInfo GetMethodDefinition<T>(Expression<Func<T, object>> callExpr)
        {
            return GetMethodDefinitionImpl(EliminateConvert(callExpr.Body));
        }

        public static object CallMethod<T>(T target, Expression<Action<T>> callExpr, Type[] methodGenericArgs,
                                           object[] methodArgs)
        {
            MethodInfo methodInfo = ConstructMethodDefinition(callExpr, methodGenericArgs);
            return methodInfo.Invoke(target, methodArgs);
        }

        public static object CallStaticMethod(Expression<Action> callExpr, Type[] methodGenericArgs,
                                              object[] methodArgs)
        {
            MethodInfo methodInfo = ConstructStaticMethodDefinition(callExpr, methodGenericArgs);
            return methodInfo.Invoke(null, methodArgs);
        }

        public static PropertyInfo GetStaticProperty(Expression<Func<object>> callExpr)
        {
            Expression expression = EliminateConvert(callExpr.Body);
            MemberInfo memberInfo = ((MemberExpression)expression).Member;
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Bad expression. {memberInfo} is not a PropertyInfo");
            }

            return propertyInfo;
        }

        public static FieldInfo GetStaticField(Expression<Func<object>> callExpr)
        {
            Expression expression = EliminateConvert(callExpr.Body);
            MemberInfo memberInfo = ((MemberExpression)expression).Member;
            FieldInfo fi = memberInfo as FieldInfo;
            if (fi == null)
            {
                throw new ArgumentException($"Bad expression. {memberInfo} is not a FieldInfo");
            }

            return fi;
        }

        public static PropertyInfo GetProp<T>(Expression<Func<T, object>> readPropFunc, params Type[] classGenericArgs)
        {
            Expression expression = EliminateConvert(readPropFunc.Body);
            MemberInfo memberInfo = ((MemberExpression)expression).Member;
            PropertyInfo propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Bad expression. {memberInfo} is not a PropertyInfo");
            }

            if (classGenericArgs.Length == 0)
            {
                return propertyInfo;
            }

            int mt = propertyInfo.MetadataToken;
            Type type = propertyInfo.ReflectedType;
            Type resultReflectedType = type.GetGenericTypeDefinition().MakeGenericType(classGenericArgs);
            PropertyInfo[] propertyInfos = resultReflectedType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo info in propertyInfos)
            {
                if (info.MetadataToken == mt)
                {
                    return info;
                }
            }
            throw new NotSupportedException("not found");
        }

        public static FieldInfo GetField<T>(Expression<Func<T, object>> readPropFunc)
        {
            Expression expression = EliminateConvert(readPropFunc.Body);
            MemberInfo memberInfo = ((MemberExpression)expression).Member;
            FieldInfo propertyInfo = memberInfo as FieldInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException($"Bad expression. {memberInfo} is not a FieldInfo");
            }

            return propertyInfo;
        }

        public static MethodInfo ConstructGenericMethodDefinitionForGenericClass<T>(Expression<Action<T>> callExpr,
                                                                                    Type[] classGenericArgs,
                                                                                    Type[] methodGenericArgs)
        {
            MethodCallExpression methodCallExpression = (MethodCallExpression)callExpr.Body;
            MethodInfo methodInfo = methodCallExpression.Method;
            return ConstructGenericMethodDefinitionForGenericClass(methodInfo.ReflectedType, methodInfo, classGenericArgs, methodGenericArgs);
        }

        public static MethodInfo GetMethodByMetadataToken(Type type, int methodMetedataToken)
        {
            MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            //todo сделать нормально
            foreach (MethodInfo methodInfo in methodInfos)
            {
                if (methodInfo.MetadataToken == methodMetedataToken)
                {
                    return methodInfo;
                }
            }
            return null;
        }

        public static MethodInfo ConstructGenericMethodDefinitionForGenericClass(Type type, MethodInfo methodInfo, Type[] classGenericArgs, Type[] methodGenericArgs)
        {
            Type resultReflectedType = type.IsGenericType
                                          ? type.GetGenericTypeDefinition().MakeGenericType(classGenericArgs)
                                          : type;
            MethodInfo sourceMethodDefinition = methodInfo.IsGenericMethod
                                             ? methodInfo.GetGenericMethodDefinition()
                                             : methodInfo;

            MethodBase methodBase = MethodBase.GetMethodFromHandle(
                sourceMethodDefinition.MethodHandle, resultReflectedType.TypeHandle);
            MethodInfo constructedMethodForResultType = (MethodInfo)methodBase;
            if (methodInfo.IsGenericMethod)
            {
                return constructedMethodForResultType.MakeGenericMethod(methodGenericArgs);
            }

            return constructedMethodForResultType;
        }

        //public static FieldInfo ConstructFieldDefinitionForGenericClass<T>(Expression<Func<T, object>> callExpr, Type[] classGenericArgs)
        //{
        //    var methodCallExpression = (MemberExpression)EliminateConvert(callExpr.Body);
        //    Type type = typeof(T);
        //    MemberInfo memberInfo = methodCallExpression.Member;
        //    var fieldInfo = memberInfo as FieldInfo;
        //    if(fieldInfo != null)
        //        return ConstructFieldForGenericClass(type, fieldInfo, classGenericArgs);
        //    throw new ArgumentException("Expression access not a field");
        //}

        //public static FieldInfo ConstructFieldForGenericClass(Type classType, FieldInfo fieldInfo, Type[] classGenericArgs)
        //{
        //    Type resultReflectedType = classType.IsGenericType
        //                                   ? classType.GetGenericTypeDefinition().MakeGenericType(classGenericArgs)
        //                                   : classType;
        //BUG not works same as MethodBase.GetMethodFromHandle
        //    FieldInfo result = FieldInfo.GetFieldFromHandle(
        //        fieldInfo.FieldHandle, resultReflectedType.TypeHandle);
        //    return result;
        //}

        private static Expression EliminateConvert(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                return ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        private static ConstructorInfo ObjectConstruction(Expression expression)
        {
            NewExpression newExpression = (NewExpression)expression;
            return newExpression.Constructor;
        }

        private static MethodInfo GetMethodDefinitionImpl(Expression body)
        {
            BinaryExpression binaryExpression = body as BinaryExpression;
            if (binaryExpression != null)
            {
                return (binaryExpression).Method;
            }

            UnaryExpression unaryExpression = body as UnaryExpression;
            if (unaryExpression != null)
            {
                return (unaryExpression).Method;
            }

            MethodCallExpression methodCallExpression = body as MethodCallExpression;
            if (methodCallExpression != null)
            {
                return (methodCallExpression).Method;
            }

            throw new InvalidOperationException("unknown expression " + body);
        }
    }
}