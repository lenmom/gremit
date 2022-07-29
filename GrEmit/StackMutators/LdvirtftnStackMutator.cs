using GrEmit.InstructionParameters;

using System;

namespace GrEmit.StackMutators
{
    internal class LdvirtftnStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            System.Reflection.MethodInfo method = ((MethodILInstructionParameter)parameter).Method;
            CheckNotEmpty(il, stack, () => "Ldvirtftn requires an instance to be loaded onto evaluation stack");
            ESType instance = stack.Pop();
            Type declaringType = method.DeclaringType;
            CheckCanBeAssigned(il, declaringType, instance);
            stack.Push(typeof(IntPtr));
        }
    }
}