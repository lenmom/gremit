using GrEmit.InstructionParameters;

namespace GrEmit.StackMutators
{
    internal class LdlocaStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            GroboIL.Local local = ((LocalILInstructionParameter)parameter).Local;
            stack.Push(local.Type.MakeByRefType());
        }
    }
}