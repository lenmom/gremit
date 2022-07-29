﻿using GrEmit.Utils;

using NUnit.Framework;

using System;
using System.Reflection.Emit;

namespace GrEmit.Tests.OpCodesTests
{
    [TestFixture]
    public class TestCalls
    {
        public static void VoidStaticMethod_NoParameters()
        {
        }

        public static void VoidStaticMethod_WithParameters(int x, string s)
        {
        }

        public static int NonVoidStaticMethod_NoParameters()
        {
            return 0;
        }

        public static int NonVoidStaticMethod_WithParameters(int x, string s)
        {
            return 0;
        }

        [Test]
        public void Test_StaticMethod_Void_NoParameters()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Call(HackHelpers.GetMethodDefinition<int>(x => VoidStaticMethod_NoParameters()));
                il.Ret();
            }
        }

        [Test]
        public void Test_StaticMethod_Void_WithParameters()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), new[] { typeof(int), typeof(string) }, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                il.Call(HackHelpers.GetMethodDefinition<int>(x => VoidStaticMethod_WithParameters(x, "")));
                il.Ret();
            }
        }

        [Test]
        public void Test_StaticMethod_NonVoid_NoParameters()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), Type.EmptyTypes, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Call(HackHelpers.GetMethodDefinition<int>(x => NonVoidStaticMethod_NoParameters()));
                il.Ret();
            }
        }

        [Test]
        public void Test_StaticMethod_NonVoid_WithParameters()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(int), new[] { typeof(int), typeof(string) }, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Ldarg(0);
                il.Ldarg(1);
                il.Call(HackHelpers.GetMethodDefinition<int>(x => NonVoidStaticMethod_WithParameters(x, "")));
                il.Ret();
            }
        }

        public interface I1
        {
            void Zzz();
        }

        [Test]
        public void Test_ldvirtftn1()
        {
            C1 c1 = new C1();
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Newobj(typeof(C1).GetConstructor(Type.EmptyTypes));
                il.Ldvirtftn(typeof(C1).GetMethod("Zzz"));
                il.Pop();
                il.Ret();
            }
        }

        [Test]
        public void Test_ldvirtftn2()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.Newobj(typeof(C1).GetConstructor(Type.EmptyTypes));
                il.Ldvirtftn(typeof(I1).GetMethod("Zzz"));
                il.Pop();
                il.Ret();
            }
        }

        [Test]
        public void Test_ldvirtftn3()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            GroboIL il = new GroboIL(method);
            il.Newobj(typeof(C2).GetConstructor(Type.EmptyTypes));
            Assert.Throws<InvalidOperationException>(() => il.Ldvirtftn(typeof(I1).GetMethod("Zzz")));
        }

        public class C1 : I1
        {
            public void Zzz()
            {
            }
        }

        public class C2
        {
        }
    }
}