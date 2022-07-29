using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

using GrEmit.Utils;

using NUnit.Framework;

#if NET45
using System.Diagnostics;
using System.Linq;

#endif

namespace GrEmit.Tests
{
    [TestFixture]
    public class Test
    {
        [Test]
        public void Test_LdElem_OnClass_ShouldEmitType()
        {
            // private static string ClearAll(StringBuilder[] input)
            // {
            //     string result = string.Empty;
            //     
            //     for (int i = 0; i < input.Length; ++i)
            //     {
            //         result = string.Concat(result, input[i]);
            //     }
            //     return result;
            // }

            DynamicMethod method = new DynamicMethod(nameof(Test_LdElem_OnClass_ShouldEmitType), typeof(string), new[] { typeof(StringBuilder[]) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                GroboIL.Local index = il.DeclareLocal(typeof(int));
                GroboIL.Local result = il.DeclareLocal(typeof(string));

                il.Ldc_I4(0);
                il.Stloc(index);

                il.Ldstr(string.Empty);
                il.Stloc(result);

                GroboIL.Label start = il.DefineLabel("start");
                GroboIL.Label compare = il.DefineLabel("compare");

                il.Br(compare);
                il.MarkLabel(start);

                il.Ldloc(result);

                il.Ldarg(0);
                il.Ldloc(index);
                il.Ldelem(typeof(StringBuilder));
                il.Call(typeof(object).GetMethod(nameof(ToString)));

                il.Call(typeof(string).GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) }));
                il.Stloc(result);

                il.Ldloc(index);
                il.Ldc_I4(1);
                il.Add();
                il.Stloc(index);

                il.MarkLabel(compare);
                il.Ldloc(index);
                il.Ldarg(0);
                il.Ldlen();
                il.Conv<int>();
                il.Blt(start, false);

                il.Ldloc(result);
                il.Ret();
            }

            string expectedResult = "12345";

            StringBuilder[] funcArguments = new StringBuilder[expectedResult.Length];
            for (int i = 0; i < expectedResult.Length; i++)
            {
                funcArguments[i] = new StringBuilder(expectedResult[i].ToString());
            }

            Func<StringBuilder[], string> func = (Func<StringBuilder[], string>)method.CreateDelegate(typeof(Func<StringBuilder[], string>));
            Assert.That(func(funcArguments), Is.EqualTo(expectedResult));
        }

        [Test]
        public void TestLocalloc()
        {
            // private static unsafe int GetSum(byte length)
            // {
            //     byte* bytes = stackalloc byte[length];
            //     for (byte i = 0; i < length; ++i)
            //     {
            //         bytes[i] = i;
            //     }
            //     int sum = 0;
            //     for (byte i = 0; i < length; ++i)
            //     {
            //         sum += bytes[i];
            //     }
            //     return sum;
            // }
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(byte) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0); // stack: [length]
                il.Conv<UIntPtr>();
                il.Localloc(); // stack: [*pointer]
                GroboIL.Local pointer = il.DeclareLocal(typeof(UIntPtr));
                il.Stloc(pointer); // pointer = value; stack: []

                il.Ldc_I4(0); // stack: [0]
                GroboIL.Local i = il.DeclareLocal(typeof(byte));
                il.Stloc(i); // i = 0; stack: []
                GroboIL.Label loop1Start = il.DefineLabel("loop1_start");
                GroboIL.Label loop1End = il.DefineLabel("loop1_end");
                il.MarkLabel(loop1Start);
                {
                    il.Ldloc(i); // stack: [i]
                    il.Ldarg(0); // stack: [i, length]
                    il.Bge(loop1End, unsigned: true); // if (i >= length) goto end; stack: []
                    il.Ldloc(pointer); //stack: [pointer]
                    il.Ldloc(i); // stack: [pointer, i]
                    il.Add(); // stack: [pointer + i]
                    il.Ldloc(i); // stack: [pointer + i, i]
                    il.Stind(typeof(byte)); // *(pointer + i) = i; stack: []
                    il.Ldloc(i); // stack: [i]
                    il.Ldc_I4(1); // stack: [i, 1]
                    il.Add(); // stack: [i + 1]
                    il.Conv<byte>();
                    il.Stloc(i); // i = i + 1; stack: []
                    il.Br(loop1Start);
                }
                il.MarkLabel(loop1End);

                il.Ldc_I4(0); // stack: [0]
                il.Dup(); // stack: [0, 0]
                GroboIL.Local sum = il.DeclareLocal(typeof(int));
                il.Stloc(sum); // sum = 0; stack: [0]
                il.Stloc(i); // i = 0; stack: []
                GroboIL.Label loop2Start = il.DefineLabel("loop2_start");
                GroboIL.Label loop2End = il.DefineLabel("loop2_end");
                il.MarkLabel(loop2Start);
                {
                    il.Ldloc(i); // stack: [i]
                    il.Ldarg(0); // stack: [i, length]
                    il.Bge(loop2End, unsigned: true); // if i >= length goto end; stack:[]
                    il.Ldloc(pointer); // stack: [pointer]
                    il.Ldloc(i); // stack: [pointer, i]
                    il.Add(); // stack: [pointer + i]
                    il.Ldind(typeof(byte)); // stack: [*(pointer + i)]
                    il.Ldloc(sum); // stack: [*(pointer + i), sum]
                    il.Add(); // stack: [*(pointer + i) + sum]
                    il.Stloc(sum); // sum = *(pointer + i) + sum; stack: []
                    il.Ldloc(i); // stack: [i]
                    il.Ldc_I4(1); // stack: [i, 1]
                    il.Add(); // stack: [i + 1]
                    il.Conv<byte>();
                    il.Stloc(i); // i = (i + 1); // stack: []
                    il.Br(loop2Start);
                }
                il.MarkLabel(loop2End);

                il.Ldloc(sum); // stack: [sum]
                il.Ret();
            }
            Func<byte, int> func = (Func<byte, int>)method.CreateDelegate(typeof(Func<byte, int>));
            Assert.That(func(6), Is.EqualTo(15));
        }

        [Test]
        public void TestDifferentStructs()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), new[] { typeof(int) }, typeof(Test));
            GroboIL il = new GroboIL(method);
            GroboIL.Local loc1 = il.DeclareLocal(typeof(int?));
            GroboIL.Local loc2 = il.DeclareLocal(typeof(Qxx?));
            GroboIL.Label label1 = il.DefineLabel("zzz");
            il.Ldarg(0);
            il.Brfalse(label1);
            il.Ldloc(loc1);
            GroboIL.Label label2 = il.DefineLabel("qxx");
            il.Br(label2);
            il.MarkLabel(label1);
            il.Ldloc(loc2);
            Assert.Throws<InvalidOperationException>(() => il.MarkLabel(label2));
        }

        [Test]
        public void TestRet()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestAPlusB()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(int), typeof(int) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                il.Add();
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestZ()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(bool), typeof(float), typeof(double) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                GroboIL.Label label1 = il.DefineLabel("label");
                il.Brfalse(label1);
                il.Ldarg(1);
                GroboIL.Label label2 = il.DefineLabel("label");
                il.Br(label2);
                il.MarkLabel(label1);
                il.Ldarg(2);
                il.MarkLabel(label2);
                il.Ldc_I4(1);
                il.Conv<float>();
                il.Add();
                il.Conv<int>();
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestZ2()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(A), new[] { typeof(bool), typeof(B), typeof(C) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                GroboIL.Label label1 = il.DefineLabel("label");
                il.Brfalse(label1);
                il.Ldarg(1);
                GroboIL.Label label2 = il.DefineLabel("label");
                il.Br(label2);
                il.MarkLabel(label1);
                il.Ldarg(2);
                il.MarkLabel(label2);
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestHelloWorld()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(string), Type.EmptyTypes, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldstr("Hello World");
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestMax()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(int), typeof(int) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                GroboIL.Label returnSecondLabel = il.DefineLabel("returnSecond");
                il.Ble(returnSecondLabel, false);
                il.Ldarg(0);
                il.Ret();
                il.MarkLabel(returnSecondLabel);
                il.Ldarg(1);
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestPrefixes()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(IntPtr), typeof(int) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                il.Add();
                il.Ldind(typeof(int));
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestFarsh()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(int) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                GroboIL.Local temp = il.DeclareLocal(typeof(int));
                il.Ldarg(0); // stack: [x]
                GroboIL.Label label0 = il.DefineLabel("L");
                il.Br(label0); // goto L_0; stack: [x]

                il.Ldstr("zzz");
                il.Ldobj(typeof(DateTime));
                il.Mul();
                il.Initobj(typeof(int));

                GroboIL.Label label1 = il.DefineLabel("L");
                il.MarkLabel(label1); // stack: [x, 2]
                il.Stloc(temp); // temp = 2; stack: [x]
                GroboIL.Label label2 = il.DefineLabel("L");
                il.MarkLabel(label2); // stack: [cur]
                il.Ldarg(0); // stack: [cur, x]
                il.Mul(); // stack: [cur * x = cur]
                il.Ldloc(temp); // stack: [cur, temp]
                il.Ldc_I4(1); // stack: [cur, temp, 1]
                il.Sub(); // stack: [cur, temp - 1]
                il.Stloc(temp); // temp = temp - 1; stack: [cur]
                il.Ldloc(temp); // stack: [cur, temp]
                il.Ldc_I4(0); // stack: [cur, temp, 0]
                il.Bgt(label2, false); // if(temp > 0) goto L_2; stack: [cur]
                GroboIL.Label label3 = il.DefineLabel("L");
                il.Br(label3); // goto L_3; stack: [cur]
                il.MarkLabel(label0); // stack: [x]
                il.Ldc_I4(2); // stack: [x, 2]
                il.Br(label1); // goto L_1; stack: [x, 2]
                il.MarkLabel(label3); // stack: [cur]
                il.Ret(); // return cur; stack: []
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestConstructorCall()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), new[] { typeof(Zzz) }, typeof(Test));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldc_I4(8);
                il.Call(typeof(Zzz).GetConstructor(new[] { typeof(int) }));
                il.Ret();
                Console.Write(il.GetILCode());
            }
            Action<Zzz> action = (Action<Zzz>)method.CreateDelegate(typeof(Action<Zzz>));
            Zzz zzz = new Zzz(3);
            action(zzz);
            Assert.AreEqual(8, zzz.X);
        }

        public static void F1(I1 i1)
        {
        }

        public static void F2(I2 i2)
        {
        }

        [Test]
        public void TestDifferentPaths()
        {
            Console.WriteLine(Formatter.Format(typeof(Dictionary<string, int>).GetProperty("Values", BindingFlags.Public | BindingFlags.Instance).GetGetMethod()));
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), new[] { typeof(bool), typeof(C1), typeof(C2) }, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                GroboIL.Label label1 = il.DefineLabel("L1");
                il.Brfalse(label1);
                il.Ldarg(1);
                GroboIL.Label label2 = il.DefineLabel("L2");
                il.Br(label2);
                il.MarkLabel(label1);
                il.Ldarg(2);
                il.MarkLabel(label2);
                il.Dup();
                il.Call(HackHelpers.GetMethodDefinition<I1>(x => F1(x)));
                il.Call(HackHelpers.GetMethodDefinition<I2>(x => F2(x)));
                il.Ret();
                Console.Write(il.GetILCode());
            }
            Delegate action = method.CreateDelegate(typeof(Action<bool, C1, C2>));
        }

        public static void F1<T>(I1<T> i1)
        {
        }

        public static void F2<T>(I2<T> i2)
        {
        }

        [Test]
        public void TestDifferentPathsGeneric()
        {
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder module = assembly.DefineDynamicModule(Guid.NewGuid().ToString());
            TypeBuilder type = module.DefineType("Zzz", TypeAttributes.Class | TypeAttributes.Public);
            MethodBuilder method = type.DefineMethod("Qzz", MethodAttributes.Public | MethodAttributes.Static);
            GenericTypeParameterBuilder[] genericParameters = method.DefineGenericParameters("TZzz");
            GenericTypeParameterBuilder parameter = genericParameters[0];
            method.SetParameters(typeof(bool), typeof(C1<>).MakeGenericType(parameter), typeof(C2<>).MakeGenericType(parameter));
            method.SetReturnType(typeof(void));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                GroboIL.Label label1 = il.DefineLabel("L1");
                il.Brfalse(label1);
                il.Ldarg(1);
                GroboIL.Label label2 = il.DefineLabel("L2");
                il.Br(label2);
                il.MarkLabel(label1);
                il.Ldarg(2);
                il.MarkLabel(label2);
                il.Dup();
                il.Call(HackHelpers.GetMethodDefinition<I1<int>>(x => F1(x)).GetGenericMethodDefinition().MakeGenericMethod(parameter));
                il.Call(HackHelpers.GetMethodDefinition<I2<int>>(x => F2(x)).GetGenericMethodDefinition().MakeGenericMethod(parameter));
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestBrfalse()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), new[] { typeof(C2) }, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Dup();
                GroboIL.Label label = il.DefineLabel("L");
                il.Brfalse(label);
                il.Call(HackHelpers.GetMethodDefinition<I1>(x => x.GetI2()));
                il.MarkLabel(label);
                il.Call(HackHelpers.GetMethodDefinition<int>(x => F2(null)));
                il.Ret();
                Console.WriteLine(il.GetILCode());
            }
        }

        [Test]
        public void TestBrtrue()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), new[] { typeof(C2) }, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Dup();
                GroboIL.Label label = il.DefineLabel("L");
                il.Brtrue(label);
                GroboIL.Label label2 = il.DefineLabel("L");
                il.Br(label2);
                il.MarkLabel(label);
                il.Call(HackHelpers.GetMethodDefinition<I1>(x => x.GetI2()));
                il.MarkLabel(label2);
                il.Call(HackHelpers.GetMethodDefinition<int>(x => F2(null)));
                il.Ret();
                Console.WriteLine(il.GetILCode());
            }
        }

        [Test]
        public void TestConstrained()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(string), new[] { typeof(int) }, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarga(0);
                il.Call(typeof(object).GetMethod("ToString"), typeof(int));
                il.Ret();
                Console.WriteLine(il.GetILCode());
            }
        }

        public static void F<T>(ref T x)
        {
        }

        [Test]
        public void TestLdDec()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(decimal), Type.EmptyTypes);
            using (GroboIL il = new GroboIL(method))
            {
                il.LdDec(-12.3m);
                il.LdDec(34.5m);
                il.Call(typeof(decimal).GetMethod("Add", new[] { typeof(decimal), typeof(decimal) }));
                il.Ret();
                Console.WriteLine(il.GetILCode());
            }

            Func<decimal> compiledMethod = (Func<decimal>)method.CreateDelegate(typeof(Func<decimal>));
            Assert.That(compiledMethod(), Is.EqualTo(22.2m));
        }

        [Test]
        public void TestByRefGeneric()
        {
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder module = assembly.DefineDynamicModule(Guid.NewGuid().ToString());
            TypeBuilder type = module.DefineType("Zzz", TypeAttributes.Class | TypeAttributes.Public);
            MethodBuilder method = type.DefineMethod("Qzz", MethodAttributes.Public | MethodAttributes.Static);
            GenericTypeParameterBuilder[] genericParameters = method.DefineGenericParameters("TZzz");
            GenericTypeParameterBuilder parameter = genericParameters[0];
            method.SetParameters(parameter);
            method.SetReturnType(typeof(void));
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarga(0);
                il.Call(HackHelpers.GetMethodDefinition<int>(x => F(ref x)).GetGenericMethodDefinition().MakeGenericMethod(parameter));
                il.Ret();
                Console.Write(il.GetILCode());
            }
        }

        [Test]
        public void TestCalliWithNativeCallingConvention()
        {
            AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
            ModuleBuilder module = assembly.DefineDynamicModule(Guid.NewGuid().ToString());
            TypeBuilder typeBuilder = module.DefineType("TestType", TypeAttributes.Class | TypeAttributes.Public);
            const string methodName = "TestMethod";
            MethodBuilder methodBuilder = typeBuilder.DefineMethod(methodName, MethodAttributes.Public | MethodAttributes.Static, typeof(int), new Type[] { typeof(IntPtr) });
#if NETCOREAPP2_0
            Assert.Throws<PlatformNotSupportedException>(() => BuildIlWithNativeCallingConvention(methodBuilder));
#else
            BuildIlWithNativeCallingConvention(methodBuilder);
            Type type = typeBuilder.CreateType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            object invocationResult = method.Invoke(null, new object[] { Marshal.GetFunctionPointerForDelegate(new FooDelegate(Foo)) });
            Assert.That(invocationResult, Is.EqualTo(15));
#endif
        }

        private static void BuildIlWithNativeCallingConvention(MethodBuilder methodBuilder)
        {
            using (GroboIL il = new GroboIL(methodBuilder))
            {
                il.Ldc_I4(10);
                il.Ldarg(0);
                il.Calli(CallingConvention.StdCall, typeof(int), new[] { typeof(int) });
                il.Ret();
            }
        }

        private delegate int FooDelegate(int x);

        private static int Foo(int x)
        {
            return x + 5;
        }

#if NET45
        [Test, Ignore("Is used for debugging")]
        public void TestEnumerable()
        {
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = assembly.DefineDynamicModule(Guid.NewGuid().ToString(), "test.cil", true);
            System.Diagnostics.SymbolStore.ISymbolWriter symWriter = module.GetSymWriter();
            System.Diagnostics.SymbolStore.ISymbolDocumentWriter symbolDocumentWriter = symWriter.DefineDocument("test.cil", Guid.Empty, Guid.Empty, Guid.Empty);
            TypeBuilder typeBuilder = module.DefineType("Zzz", TypeAttributes.Class | TypeAttributes.Public);
            MethodBuilder method = typeBuilder.DefineMethod("Qzz", MethodAttributes.Public);
            GenericTypeParameterBuilder[] genericParameters = method.DefineGenericParameters("TZzz");
            GenericTypeParameterBuilder parameter = genericParameters[0];
            method.SetParameters(typeof(List<>).MakeGenericType(parameter), typeof(Func<,>).MakeGenericType(parameter, typeof(int)));
            method.DefineParameter(2, ParameterAttributes.In, "list");
            method.SetReturnType(typeof(int));
            using (GroboIL il = new GroboIL(method, symbolDocumentWriter))
            {
                il.Ldarg(1);
                il.Dup();
                GroboIL.Label notNullLabel = il.DefineLabel("notNull");
                il.Brtrue(notNullLabel);
                il.Pop();
                il.Ldc_I4(0);
                il.Newarr(parameter);
                il.MarkLabel(notNullLabel);
                GroboIL.Local temp = il.DeclareLocal(typeof(IEnumerable<>).MakeGenericType(parameter), "arr");
                il.Stloc(temp);
                il.Ldloc(temp);
                il.Ldarg(2);
                il.Call(HackHelpers.GetMethodDefinition<int[]>(ints => ints.Sum(x => x)).GetGenericMethodDefinition().MakeGenericMethod(parameter));
                il.Ret();
                Console.WriteLine(il.GetILCode());
            }
            Type type = typeBuilder.CreateType();
            object instance = Activator.CreateInstance(type);
            type.GetMethod("Qzz", BindingFlags.Instance | BindingFlags.Public).MakeGenericMethod(typeof(int)).Invoke(instance, new object[] { null, null });
        }
#endif

        public static string[] Create(string[] values)
        {
            CheckDifferent(values);
            HashSet<int> hashSet = new HashSet<int>();
            for (int n = Math.Max(values.Length, 1); ; ++n)
            {
                hashSet.Clear();
                bool ok = true;
                foreach (string str in values)
                {
                    int idx = str.GetHashCode() % n;
                    if (idx < 0)
                    {
                        idx += n;
                    }

                    if (hashSet.Contains(idx))
                    {
                        ok = false;
                        break;
                    }
                    hashSet.Add(idx);
                }
                if (ok)
                {
                    string[] result = new string[n];
                    foreach (string str in values)
                    {
                        int idx = str.GetHashCode() % n;
                        if (idx < 0)
                        {
                            idx += n;
                        }

                        result[idx] = str;
                    }
                    return result;
                }
            }
        }

#if NET45
        [Test, Ignore("Is used for debugging")]
        public void TestPerformance_Ifs_vs_Switch()
        {
            AssemblyBuilder assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder module = assembly.DefineDynamicModule("Zzz", true);

            for (int numberOfCases = 1; numberOfCases <= 20; ++numberOfCases)
            {
                Console.WriteLine("#" + numberOfCases);
                string[] keys = new string[numberOfCases];
                for (int i = 0; i < numberOfCases; ++i)
                {
                    keys[i] = Guid.NewGuid().ToString();
                }

                IQxx ifs = BuildIfs(module, keys);
                IQxx switCh = BuildSwitch(module, keys);
                const int iterations = 100_000_000;

                Console.WriteLine("Worst case:");

                Stopwatch stopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations / numberOfCases; ++iter)
                {
                    for (int i = 0; i < numberOfCases; ++i)
                    {
                        ifs.Set("zzz", iter);
                    }

                    ifs.Set("zzz", iter);
                }
                TimeSpan elapsedIfs = stopwatch.Elapsed;
                Console.WriteLine("Ifs: " + elapsedIfs.TotalMilliseconds * 1000 / iterations + " microseconds (" + Math.Round(1000.0 * iterations / elapsedIfs.TotalMilliseconds) + " runs per second)");

                stopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations / numberOfCases; ++iter)
                {
                    for (int i = 0; i < numberOfCases; ++i)
                    {
                        switCh.Set("zzz", iter);
                    }

                    switCh.Set("zzz", iter);
                }
                TimeSpan elapsedSwitch = stopwatch.Elapsed;
                Console.WriteLine("Switch: " + elapsedSwitch.TotalMilliseconds * 1000 / iterations + " microseconds (" + Math.Round(1000.0 * iterations / elapsedSwitch.TotalMilliseconds) + " runs per second)");
                Console.WriteLine(elapsedSwitch.TotalMilliseconds / elapsedIfs.TotalMilliseconds);

                Console.WriteLine("Average:");

                stopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations / numberOfCases; ++iter)
                {
                    for (int i = 0; i < numberOfCases; ++i)
                    {
                        ifs.Set(keys[i], iter);
                    }

                    ifs.Set("zzz", iter);
                }
                elapsedIfs = stopwatch.Elapsed;
                Console.WriteLine("Ifs: " + elapsedIfs.TotalMilliseconds * 1000 / iterations + " microseconds (" + Math.Round(1000.0 * iterations / elapsedIfs.TotalMilliseconds) + " runs per second)");

                stopwatch = Stopwatch.StartNew();
                for (int iter = 0; iter < iterations / numberOfCases; ++iter)
                {
                    for (int i = 0; i < numberOfCases; ++i)
                    {
                        switCh.Set(keys[i], iter);
                    }

                    switCh.Set("zzz", iter);
                }
                elapsedSwitch = stopwatch.Elapsed;
                Console.WriteLine("Switch: " + elapsedSwitch.TotalMilliseconds * 1000 / iterations + " microseconds (" + Math.Round(1000.0 * iterations / elapsedSwitch.TotalMilliseconds) + " runs per second)");
                Console.WriteLine(elapsedSwitch.TotalMilliseconds / elapsedIfs.TotalMilliseconds);

                Console.WriteLine();
            }
        }
#endif

        [Test]
        public void Test_CreateStruct_ThenGetField()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), Type.EmptyTypes);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldc_I4(5);
                il.Newobj(HackHelpers.GetObjectConstruction(() => new TestStruct(0)));
                il.Ldfld(HackHelpers.GetField<TestStruct>(x => x.Value));
                il.Ret();
            }
            Assert.That(method.Invoke(null, null), Is.EqualTo(5));
        }

        public struct TestStruct
        {
            public TestStruct(int value)
            {
                Value = value;
            }

            public readonly int Value;
        }

        [Test]
        public void TestCallFormat()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(string), new[] { typeof(string), typeof(decimal), typeof(decimal) }, typeof(string), true);
            GroboIL il = new GroboIL(method);
            il.Ldarg(0);
            il.Ldarg(1);
            il.Ldarg(2);
            Assert.Throws<InvalidOperationException>(() => il.Call(HackHelpers.GetMethodDefinition<string>(s => string.Format(s, "", ""))));
        }

        public interface I1
        {
            I2 GetI2();
        }

        public interface I2
        {
        }

        public interface I1<T>
        {
        }

        public interface I2<T>
        {
        }

        public interface IQxx
        {
            void Set(string key, int value);
        }

        private static void CheckDifferent(string[] values)
        {
            HashSet<string> hashSet = new HashSet<string>();
            foreach (string str in values)
            {
                if (hashSet.Contains(str))
                {
                    throw new InvalidOperationException($"Duplicate value '{str}'");
                }

                hashSet.Add(str);
            }
        }

        private IQxx BuildIfs(ModuleBuilder module, string[] keys)
        {
            int numberOfCases = keys.Length;
            TypeBuilder typeBuilder = module.DefineType("Ifs" + Guid.NewGuid(), TypeAttributes.Class | TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(typeof(IQxx));
            FieldInfo[] fields = new FieldInfo[numberOfCases];
            for (int i = 0; i < numberOfCases; ++i)
            {
                fields[i] = typeBuilder.DefineField(keys[i], typeof(int), FieldAttributes.Public);
            }

            MethodBuilder method = typeBuilder.DefineMethod("Set", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new[] { typeof(string), typeof(int) });
            method.DefineParameter(1, ParameterAttributes.In, "key");
            method.DefineParameter(2, ParameterAttributes.In, "value");
            using (GroboIL il = new GroboIL(method))
            {
                GroboIL.Label doneLabel = il.DefineLabel("done");
                for (int i = 0; i < numberOfCases; ++i)
                {
                    il.Ldarg(1); // stack: [key]
                    il.Ldstr(keys[i]); // stack: [key, keys[i]]
                    il.Call(stringEqualityOperator); // stack: [key == keys[i]]
                    GroboIL.Label nextKeyLabel = il.DefineLabel("nextKey");
                    il.Brfalse(nextKeyLabel); // if(key != keys[i]) goto nextKey; stack: []
                    il.Ldarg(0);
                    il.Ldarg(2);
                    il.Stfld(fields[i]);
                    il.Br(doneLabel);
                    il.MarkLabel(nextKeyLabel);
                }
                il.MarkLabel(doneLabel);
                il.Ret();
            }
            typeBuilder.DefineMethodOverride(method, typeof(IQxx).GetMethod("Set"));
            Type type = typeBuilder.CreateType();
            return (IQxx)Activator.CreateInstance(type);
        }

        private IQxx BuildSwitch(ModuleBuilder module, string[] keys)
        {
            int numberOfCases = keys.Length;
            TypeBuilder typeBuilder = module.DefineType("Switch" + Guid.NewGuid(), TypeAttributes.Class | TypeAttributes.Public);
            typeBuilder.AddInterfaceImplementation(typeof(IQxx));
            FieldInfo[] fields = new FieldInfo[numberOfCases];
            for (int i = 0; i < numberOfCases; ++i)
            {
                fields[i] = typeBuilder.DefineField(keys[i], typeof(int), FieldAttributes.Public);
            }

            string[] tinyHashtable = Create(keys);
            int n = tinyHashtable.Length;
            FieldBuilder keysField = typeBuilder.DefineField("keys", typeof(string[]), FieldAttributes.Public);
            ConstructorBuilder constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(string[]) });
            using (GroboIL il = new GroboIL(constructor))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                il.Stfld(keysField);
                il.Ret();
            }

            MethodBuilder method = typeBuilder.DefineMethod("Set", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new[] { typeof(string), typeof(int) });
            method.DefineParameter(1, ParameterAttributes.In, "key");
            method.DefineParameter(2, ParameterAttributes.In, "value");
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldfld(keysField);
                il.Ldarg(1);
                il.Call(HackHelpers.GetMethodDefinition<object>(o => o.GetHashCode()));
                il.Ldc_I4(n);
                il.Rem(true);
                GroboIL.Local idx = il.DeclareLocal(typeof(int));
                il.Dup();
                il.Stloc(idx);
                il.Ldelem(typeof(string));
                il.Ldarg(1);
                il.Call(stringEqualityOperator);
                GroboIL.Label doneLabel = il.DefineLabel("done");
                il.Brfalse(doneLabel);

                GroboIL.Label[] labels = new GroboIL.Label[n];
                for (int i = 0; i < n; ++i)
                {
                    labels[i] = doneLabel;
                }

                foreach (string key in keys)
                {
                    int index = key.GetHashCode() % n;
                    if (index < 0)
                    {
                        index += n;
                    }

                    GroboIL.Label label = il.DefineLabel("set_" + key);
                    labels[index] = label;
                }
                il.Ldloc(idx);
                il.Switch(labels);
                for (int i = 0; i < keys.Length; ++i)
                {
                    int index = keys[i].GetHashCode() % n;
                    if (index < 0)
                    {
                        index += n;
                    }

                    il.MarkLabel(labels[index]);
                    il.Ldarg(0);
                    il.Ldarg(2);
                    il.Stfld(fields[i]);
                    il.Br(doneLabel);
                }
                il.MarkLabel(doneLabel);
                il.Ret();
            }
            typeBuilder.DefineMethodOverride(method, typeof(IQxx).GetMethod("Set"));
            Type type = typeBuilder.CreateType();
            return (IQxx)Activator.CreateInstance(type, new object[] { tinyHashtable });
        }

        private struct Qxx
        {
#pragma warning disable 169
            private Guid x1;
            private Guid x2;
            private Guid x3;
            private Guid x4;
            private Guid x5;
            private Guid x6;
            private Guid x7;
            private Guid x8;
            private Guid x9;
#pragma warning restore 169
        }

        private static readonly MethodInfo stringEqualityOperator = HackHelpers.GetMethodDefinition<string>(s => s == "");

        public class C1 : I1, I2
        {
            public I2 GetI2()
            {
                throw new NotImplementedException();
            }
        }

        public class C2 : I1, I2
        {
            public I2 GetI2()
            {
                throw new NotImplementedException();
            }
        }

        public class Base<T>
        {
        }

        public class C1<T> : Base<T>, I1<T>, I2<T>
        {
        }

        public class C2<T> : Base<T>, I1<T>, I2<T>
        {
        }

        private class A
        {
        }

        private class B : A
        {
        }

        private class C : A
        {
        }

        private class Zzz
        {
            public Zzz(int x)
            {
                X = x;
            }

            public int X { get; private set; }
        }
    }
}