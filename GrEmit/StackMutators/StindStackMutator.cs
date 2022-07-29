using GrEmit.InstructionParameters;

namespace GrEmit.StackMutators
{
    internal class StindStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            System.Type type = ((TypeILInstructionParameter)parameter).Type;
            CheckNotEmpty(il, stack, () => "A value must be put onto the evaluation stack in order to perform the 'stind' instruction");
            CheckCanBeAssigned(il, type, stack.Pop());
            CheckNotEmpty(il, stack, () => "In order to perform the 'stind' instruction an address must be put onto the evaluation stack");
            ESType esType = stack.Pop();
            CheckIsAPointer(il, esType);
            System.Type pointer = esType.ToType();
            if (pointer.IsByRef || pointer.IsPointer)
            {
                System.Type elementType = pointer.GetElementType();
                if (elementType.IsValueType)
                {
                    CheckCanBeAssigned(il, pointer, type.MakeByRefType());
                }
                else
                {
                    CheckCanBeAssigned(il, elementType, type);
                }
            }
            else if (pointer.IsPointer)
            {
                System.Type elementType = pointer.GetElementType();
                if (elementType.IsValueType)
                {
                    CheckCanBeAssigned(il, pointer, type.MakePointerType());
                }
                else
                {
                    CheckCanBeAssigned(il, elementType, type);
                }
            }
        }
    }
}