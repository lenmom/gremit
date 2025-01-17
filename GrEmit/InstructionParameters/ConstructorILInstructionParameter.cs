using GrEmit.Utils;

using System.Reflection;

namespace GrEmit.InstructionParameters
{
    internal class ConstructorILInstructionParameter : ILInstructionParameter
    {
        public ConstructorILInstructionParameter(ConstructorInfo constructor)
        {
            Constructor = constructor;
        }

        public override string Format()
        {
            return Formatter.Format(Constructor);
        }

        public ConstructorInfo Constructor { get; private set; }
    }
}