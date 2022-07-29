using GrEmit.InstructionParameters;

using System;

namespace GrEmit.StackMutators
{
    internal class LdelemaStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            Type elementType = ((TypeILInstructionParameter)parameter).Type;
            CheckNotEmpty(il, stack, () => "In order to perform the 'ldelema' instruction an index must be put onto the evaluation stack");
            CheckCanBeAssigned(il, typeof(int), stack.Pop());
            CheckNotEmpty(il, stack, () => "In order to perform the 'ldelema' instruction an array must be put onto the evaluation stack");
            ESType esType = stack.Pop();
            Type array = esType.ToType();
            if (!array.IsArray && array != typeof(Array))
            {
                throw new InvalidOperationException($"An array expected to perform the 'ldelema' instruction but was '{esType}'");
            }

            stack.Push(elementType.MakeByRefType());
        }
    }
}