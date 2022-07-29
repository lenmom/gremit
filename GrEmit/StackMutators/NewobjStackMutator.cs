using GrEmit.InstructionParameters;
using GrEmit.Utils;

namespace GrEmit.StackMutators
{
    internal class NewobjStackMutator : StackMutator
    {
        public override void Mutate(GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            System.Reflection.ConstructorInfo constructor = ((ConstructorILInstructionParameter)parameter).Constructor;
            System.Type[] parameterTypes = ReflectionExtensions.GetParameterTypes(constructor);
            for (int i = parameterTypes.Length - 1; i >= 0; --i)
            {
                CheckNotEmpty(il, stack, () => $"Expected exactly {parameterTypes.Length} parameters to call the constructor '{Formatter.Format(constructor)}'");
                CheckCanBeAssigned(il, parameterTypes[i], stack.Pop());
            }
            stack.Push(constructor.ReflectedType);
        }
    }
}