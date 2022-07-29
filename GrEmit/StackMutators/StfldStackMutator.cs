using GrEmit.InstructionParameters;
using GrEmit.Utils;

namespace GrEmit.StackMutators
{
    internal class StfldStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            System.Reflection.FieldInfo field = ((FieldILInstructionParameter)parameter).Field;
            CheckNotEmpty(il, stack, () => $"In order to store the field '{Formatter.Format(field)}' a value must be put onto the evaluation stack");
            CheckCanBeAssigned(il, field.FieldType, stack.Pop());
            if (!field.IsStatic)
            {
                System.Type declaringType = field.DeclaringType;
                CheckNotEmpty(il, stack, () => $"In order to store the field '{Formatter.Format(field)}' an instance must be put onto the evaluation stack");

                System.Type instance = stack.Pop().ToType();
                if (instance != null)
                {
                    if (instance.IsValueType)
                    {
                        ThrowError(il, $"In order to store the field '{Formatter.Format(field)}' of a value type '{Formatter.Format(instance)}' load an instance by ref");
                    }
                    else if (!instance.IsByRef)
                    {
                        CheckCanBeAssigned(il, declaringType, instance);
                    }
                    else
                    {
                        System.Type elementType = instance.GetElementType();
                        if (elementType.IsValueType)
                        {
                            if (declaringType != elementType)
                            {
                                ThrowError(il, $"Cannot store the field '{Formatter.Format(field)}' to an instance of type '{Formatter.Format(elementType)}'");
                            }
                        }
                        else
                        {
                            ThrowError(il, $"Cannot store the field '{Formatter.Format(field)}' to an instance of type '{Formatter.Format(instance)}'");
                        }
                    }
                }
            }
        }
    }
}