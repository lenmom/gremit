using GrEmit.InstructionParameters;

using System;

namespace GrEmit.StackMutators
{
    internal class StelemStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            Type elementType = ((TypeILInstructionParameter)parameter).Type;
            CheckNotEmpty(il, stack, () => "A value must be put onto the evaluation stack in order to perform the 'stelem' instruction");
            CheckCanBeAssigned(il, elementType, stack.Pop());
            CheckNotEmpty(il, stack, () => "In order to perform the 'stelem' instruction an index must be put onto the evaluation stack");
            CheckCanBeAssigned(il, typeof(int), stack.Pop());
            CheckNotEmpty(il, stack, () => "In order to perform the 'stelem' instruction an array must be put onto the evaluation stack");
            ESType esType = stack.Pop();
            Type array = esType.ToType();
            if (!array.IsArray && array != typeof(Array))
            {
                ThrowError(il, $"An array expected to perform the 'stelem' instruction but was '{esType}'");
            }
        }
    }
}