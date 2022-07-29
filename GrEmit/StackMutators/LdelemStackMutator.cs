using GrEmit.InstructionParameters;

using System;

namespace GrEmit.StackMutators
{
    internal class LdelemStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            Type elementType = ((TypeILInstructionParameter)parameter).Type;
            CheckNotEmpty(il, stack, () => "In order to perform the 'ldelem' instruction an index must be put onto the evaluation stack");
            CheckCanBeAssigned(il, typeof(int), stack.Pop());
            CheckNotEmpty(il, stack, () => "In order to perform the 'ldelem' instruction an array must be put onto the evaluation stack");
            ESType esType = stack.Pop();
            Type array = esType.ToType();
            if (!array.IsArray && array != typeof(Array))
            {
                throw new InvalidOperationException($"An array expected to perform the 'ldelem' instruction but was '{esType}'");
            }

            CheckCanBeAssigned(il, elementType, array == typeof(Array) ? typeof(object) : array.GetElementType());
            stack.Push(elementType);
        }
    }
}