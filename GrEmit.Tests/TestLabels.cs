using NUnit.Framework;

using System;
using System.Reflection.Emit;

namespace GrEmit.Tests
{
    [TestFixture]
    public class TestLabels
    {
        [Test]
        public void TestLabelHasNotBeenMarked()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            GroboIL il = new GroboIL(method);
            GroboIL.Label label = il.DefineLabel("L");
            il.Ldc_I4(0);
            il.Brfalse(label);
            il.Ret();
            InvalidOperationException e = Assert.Throws<InvalidOperationException>(il.Seal);
            Assert.AreEqual("The label 'L_0' has not been marked", e.Message);
        }

        [Test]
        public void TestLabelHasNotBeenMarked_Switch()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            GroboIL il = new GroboIL(method);
            GroboIL.Label label = il.DefineLabel("L");
            il.Ldc_I4(0);
            il.Switch(label);
            il.Ret();
            InvalidOperationException e = Assert.Throws<InvalidOperationException>(il.Seal);
            Assert.AreEqual("The label 'L_0' has not been marked", e.Message);
        }

        [Test]
        public void TestLabelHasNotBeenUsed()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            using (GroboIL il = new GroboIL(method))
            {
                il.DefineLabel("L");
                il.Ret();
            }
        }

        [Test]
        public void TestLabelHasBeenMarkedTwice()
        {
            DynamicMethod method = new DynamicMethod(Guid.NewGuid().ToString(), typeof(void), Type.EmptyTypes, typeof(string), true);
            GroboIL il = new GroboIL(method);
            GroboIL.Label label = il.DefineLabel("L");
            il.Ldc_I4(0);
            il.Brfalse(label);
            il.Ldc_I4(1);
            il.Pop();
            il.MarkLabel(label);
            il.Ldc_I4(2);
            il.Pop();
            InvalidOperationException e = Assert.Throws<InvalidOperationException>(() => il.MarkLabel(label));
            Assert.AreEqual("The label 'L_0' has already been marked", e.Message);
        }
    }
}