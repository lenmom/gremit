using GrEmit.InstructionParameters;

using System.Linq;

namespace GrEmit.StackMutators
{
    internal class BrtrueStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            GroboIL.Label label = ((LabelILInstructionParameter)parameter).Label;
            CheckNotEmpty(il, stack, () => "The 'brtrue' instruction requires one argument but none is supplied");
            ESType value = stack.Pop();
            CheckNotStruct(il, value);

            ESType[] newStack = stack.Reverse().ToArray();
            for (int i = 0; i < newStack.Length; ++i)
            {
                if (ReferenceEquals(newStack[i], value))
                {
                    newStack[i] = ESType.Zero;
                }
            }

            SaveOrCheck(il, stack, label);

            stack = new EvaluationStack(newStack);
        }
    }
}