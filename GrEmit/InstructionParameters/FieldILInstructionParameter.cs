using GrEmit.Utils;

using System.Reflection;

namespace GrEmit.InstructionParameters
{
    internal class FieldILInstructionParameter : ILInstructionParameter
    {
        public FieldILInstructionParameter(FieldInfo field)
        {
            Field = field;
        }

        public override string Format()
        {
            return Formatter.Format(Field);
        }

        public FieldInfo Field { get; private set; }
    }
}