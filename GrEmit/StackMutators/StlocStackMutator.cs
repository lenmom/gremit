using GrEmit.InstructionParameters;

namespace GrEmit.StackMutators
{
    internal class StlocStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            GroboIL.Local local = ((LocalILInstructionParameter)parameter).Local;
            CheckNotEmpty(il, stack, () => "A value must be put onto the evaluation stack in order to perform the 'stloc' instruction");
            ESType peek = stack.Pop();
            CheckCanBeAssigned(il, local.Type, peek);
        }
    }
}