using GrEmit.Utils;

using System.Reflection;

namespace GrEmit.InstructionParameters
{
    internal class MethodILInstructionParameter : ILInstructionParameter
    {
        public MethodILInstructionParameter(MethodInfo method)
        {
            Method = method;
        }

        public override string Format()
        {
            return Formatter.Format(Method);
        }

        public MethodInfo Method { get; private set; }
    }
}