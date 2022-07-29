using GrEmit.InstructionParameters;
using GrEmit.Utils;

namespace GrEmit.StackMutators
{
    internal class LdindStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            System.Type type = ((TypeILInstructionParameter)parameter).Type;
            CheckNotEmpty(il, stack, () => "In order to perform the 'ldind' instruction load an address onto the evaluation stack");
            ESType esType = stack.Pop();
            CheckIsAPointer(il, esType);
            System.Type pointer = esType.ToType();
            if (pointer.IsByRef)
            {
                System.Type elementType = pointer.GetElementType();
                if (elementType.IsValueType)
                {
                    CheckCanBeAssigned(il, type.MakeByRefType(), pointer);
                }
                else
                {
                    CheckCanBeAssigned(il, type, elementType);
                }
            }
            else if (pointer.IsPointer)
            {
                System.Type elementType = pointer.GetElementType();
                if (elementType.IsValueType)
                {
                    CheckCanBeAssigned(il, type.MakePointerType(), pointer);
                }
                else
                {
                    CheckCanBeAssigned(il, type, elementType);
                }
            }
            else if (!type.IsPrimitive && type != typeof(object))
            {
                ThrowError(il, $"Unable to load an instance of type '{Formatter.Format(type)}' from a pointer of type '{Formatter.Format(pointer)}' indirectly");
            }

            stack.Push(type);
        }
    }
}