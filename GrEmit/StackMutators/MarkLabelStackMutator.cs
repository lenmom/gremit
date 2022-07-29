using GrEmit.InstructionParameters;

namespace GrEmit.StackMutators
{
    internal class MarkLabelStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            GroboIL.Label label = ((LabelILInstructionParameter)parameter).Label;
            if (stack != null)
            {
                SaveOrCheck(il, stack, label);
            }

            if (il.labelStacks.TryGetValue(label, out ESType[] labelStack))
            {
                stack = new EvaluationStack(labelStack);
            }
        }
    }
}