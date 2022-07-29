using GrEmit.InstructionParameters;

namespace GrEmit.StackMutators
{
    internal class LdlocStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            GroboIL.Local local = ((LocalILInstructionParameter)parameter).Local;
            stack.Push(local.Type);
        }
    }
}