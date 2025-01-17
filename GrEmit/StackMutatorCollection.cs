using GrEmit.StackMutators;

using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace GrEmit
{
    internal static class StackMutatorCollection
    {
        public static void Mutate(OpCode opCode, GroboIL il, ILInstructionParameter parameter, ref EvaluationStack stack)
        {
            StackMutator stackMutator;
            if (opCode.Size == 0)
            {
                stackMutator = markLabelStackMutator;
            }
            else if (!stackMutators.TryGetValue(opCode, out stackMutator))
            {
                throw new NotSupportedException("OpCode '" + opCode + "' is not supported");
            }

            stackMutator.Mutate(il, parameter, ref stack);
        }

        private static readonly Dictionary<OpCode, StackMutator> stackMutators = new Dictionary<OpCode, StackMutator>
            {
                {OpCodes.Nop, new NopStackMutator()},
                {OpCodes.Break, new NopStackMutator()},
                {OpCodes.Pop, new PopStackMutator()},
                {OpCodes.Dup, new DupStackMutator()},
                {OpCodes.Ldc_I4, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_0, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_1, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_2, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_3, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_4, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_5, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_6, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_7, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_8, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_M1, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I4_S, new Ldc_I4StackMutator()},
                {OpCodes.Ldc_I8, new Ldc_I8StackMutator()},
                {OpCodes.Ldc_R4, new Ldc_R4StackMutator()},
                {OpCodes.Ldc_R8, new Ldc_R8StackMutator()},
                {OpCodes.Ldstr, new LdstrStackMutator()},
                {OpCodes.Ret, new RetStackMutator()},
                {OpCodes.Call, new CallStackMutator(false)},
                {OpCodes.Callvirt, new CallStackMutator(true)},
                {OpCodes.Calli, new CalliStackMutator()},
                {OpCodes.Jmp, new JmpStackMutator()},
                {OpCodes.Leave, new BrStackMutator()},
                {OpCodes.Br, new BrStackMutator()},
                {OpCodes.Br_S, new BrStackMutator()},
                {OpCodes.Brfalse, new BrfalseStackMutator()},
                {OpCodes.Brfalse_S, new BrfalseStackMutator()},
                {OpCodes.Brtrue, new BrtrueStackMutator()},
                {OpCodes.Brtrue_S, new BrtrueStackMutator()},
                {OpCodes.Beq, new BranchEqualityStackMutator(OpCodes.Beq)},
                {OpCodes.Beq_S, new BranchEqualityStackMutator(OpCodes.Beq_S)},
                {OpCodes.Bne_Un, new BranchEqualityStackMutator(OpCodes.Bne_Un)},
                {OpCodes.Bne_Un_S, new BranchEqualityStackMutator(OpCodes.Bne_Un_S)},
                {OpCodes.Ble, new BranchComparisonStackMutator(OpCodes.Ble)},
                {OpCodes.Ble_S, new BranchComparisonStackMutator(OpCodes.Ble_S)},
                {OpCodes.Ble_Un, new BranchComparisonStackMutator(OpCodes.Ble_Un)},
                {OpCodes.Ble_Un_S, new BranchComparisonStackMutator(OpCodes.Ble_Un_S)},
                {OpCodes.Blt, new BranchComparisonStackMutator(OpCodes.Blt)},
                {OpCodes.Blt_S, new BranchComparisonStackMutator(OpCodes.Blt_S)},
                {OpCodes.Blt_Un, new BranchComparisonStackMutator(OpCodes.Blt_Un)},
                {OpCodes.Blt_Un_S, new BranchComparisonStackMutator(OpCodes.Blt_Un_S)},
                {OpCodes.Bge, new BranchComparisonStackMutator(OpCodes.Bge)},
                {OpCodes.Bge_S, new BranchComparisonStackMutator(OpCodes.Bge_S)},
                {OpCodes.Bge_Un, new BranchComparisonStackMutator(OpCodes.Bge_Un)},
                {OpCodes.Bge_Un_S, new BranchComparisonStackMutator(OpCodes.Bge_Un_S)},
                {OpCodes.Bgt, new BranchComparisonStackMutator(OpCodes.Bgt)},
                {OpCodes.Bgt_S, new BranchComparisonStackMutator(OpCodes.Bgt_S)},
                {OpCodes.Bgt_Un, new BranchComparisonStackMutator(OpCodes.Bgt_Un)},
                {OpCodes.Bgt_Un_S, new BranchComparisonStackMutator(OpCodes.Bgt_Un_S)},
                {OpCodes.Ceq, new ArithmeticEqualityStackMutator(OpCodes.Ceq)},
                {OpCodes.Cgt, new ArithmeticComparisonStackMutator(OpCodes.Cgt)},
                {OpCodes.Cgt_Un, new Cgt_UnStackMutator()},
                {OpCodes.Clt, new ArithmeticComparisonStackMutator(OpCodes.Clt)},
                {OpCodes.Clt_Un, new ArithmeticComparisonStackMutator(OpCodes.Clt_Un)},
                {OpCodes.Switch, new SwitchStackMutator()},
                {OpCodes.Add, new Add_UnStackMutator(OpCodes.Add)},
                {OpCodes.Add_Ovf, new IntegerOpStackMutator(OpCodes.Add_Ovf)},
                {OpCodes.Add_Ovf_Un, new Add_Ovf_UnStackMutator(OpCodes.Add_Ovf_Un)},
                {OpCodes.Sub, new Sub_UnStackMutator(OpCodes.Sub)},
                {OpCodes.Sub_Ovf, new IntegerOpStackMutator(OpCodes.Sub_Ovf)},
                {OpCodes.Sub_Ovf_Un, new Sub_Ovf_UnStackMutator(OpCodes.Sub_Ovf_Un)},
                {OpCodes.Mul, new NumericOpStackMutator(OpCodes.Mul)},
                {OpCodes.Mul_Ovf, new IntegerOpStackMutator(OpCodes.Mul_Ovf)},
                {OpCodes.Mul_Ovf_Un, new IntegerOpStackMutator(OpCodes.Mul_Ovf_Un)},
                {OpCodes.Div, new NumericOpStackMutator(OpCodes.Div)},
                {OpCodes.Div_Un, new IntegerOpStackMutator(OpCodes.Div_Un)},
                {OpCodes.Rem, new NumericOpStackMutator(OpCodes.Rem)},
                {OpCodes.Rem_Un, new IntegerOpStackMutator(OpCodes.Rem_Un)},
                {OpCodes.And, new IntegerOpStackMutator(OpCodes.And)},
                {OpCodes.Or, new IntegerOpStackMutator(OpCodes.Or)},
                {OpCodes.Xor, new IntegerOpStackMutator(OpCodes.Xor)},
                {OpCodes.Shl, new ShiftOpStackMutator(OpCodes.Shl)},
                {OpCodes.Shr, new ShiftOpStackMutator(OpCodes.Shr)},
                {OpCodes.Shr_Un, new ShiftOpStackMutator(OpCodes.Shr_Un)},
                {OpCodes.Neg, new NegStackMutator()},
                {OpCodes.Not, new NotStackMutator()},
                {OpCodes.Conv_I1, new ConvI1StackMutator(OpCodes.Conv_I1)},
                {OpCodes.Conv_I2, new ConvI2StackMutator(OpCodes.Conv_I2)},
                {OpCodes.Conv_I4, new ConvI4StackMutator(OpCodes.Conv_I4)},
                {OpCodes.Conv_I8, new ConvI8StackMutator(OpCodes.Conv_I8)},
                {OpCodes.Conv_U1, new ConvU1StackMutator(OpCodes.Conv_U1)},
                {OpCodes.Conv_U2, new ConvU2StackMutator(OpCodes.Conv_U2)},
                {OpCodes.Conv_U4, new ConvU4StackMutator(OpCodes.Conv_U4)},
                {OpCodes.Conv_U8, new ConvU8StackMutator(OpCodes.Conv_U8)},
                {OpCodes.Conv_I, new ConvIStackMutator()},
                {OpCodes.Conv_U, new ConvUStackMutator()},
                {OpCodes.Conv_R4, new ConvR4StackMutator(OpCodes.Conv_R4)},
                {OpCodes.Conv_R8, new ConvR8StackMutator(OpCodes.Conv_R8)},
                {OpCodes.Conv_R_Un, new ConvR4StackMutator(OpCodes.Conv_R4)},
                {OpCodes.Conv_Ovf_I1, new ConvI1StackMutator(OpCodes.Conv_Ovf_I1)},
                {OpCodes.Conv_Ovf_I1_Un, new ConvI1StackMutator(OpCodes.Conv_Ovf_I1_Un)},
                {OpCodes.Conv_Ovf_I2, new ConvI2StackMutator(OpCodes.Conv_Ovf_I2)},
                {OpCodes.Conv_Ovf_I2_Un, new ConvI2StackMutator(OpCodes.Conv_Ovf_I2_Un)},
                {OpCodes.Conv_Ovf_I4, new ConvI4StackMutator(OpCodes.Conv_Ovf_I4)},
                {OpCodes.Conv_Ovf_I4_Un, new ConvI4StackMutator(OpCodes.Conv_Ovf_I4_Un)},
                {OpCodes.Conv_Ovf_I8, new ConvI8StackMutator(OpCodes.Conv_Ovf_I8)},
                {OpCodes.Conv_Ovf_I8_Un, new ConvI8StackMutator(OpCodes.Conv_Ovf_I8_Un)},
                {OpCodes.Conv_Ovf_U1, new ConvU1StackMutator(OpCodes.Conv_Ovf_U1)},
                {OpCodes.Conv_Ovf_U1_Un, new ConvU1StackMutator(OpCodes.Conv_Ovf_U1_Un)},
                {OpCodes.Conv_Ovf_U2, new ConvU2StackMutator(OpCodes.Conv_Ovf_U2)},
                {OpCodes.Conv_Ovf_U2_Un, new ConvU2StackMutator(OpCodes.Conv_Ovf_U2_Un)},
                {OpCodes.Conv_Ovf_U4, new ConvU4StackMutator(OpCodes.Conv_Ovf_U4)},
                {OpCodes.Conv_Ovf_U4_Un, new ConvU4StackMutator(OpCodes.Conv_Ovf_U4_Un)},
                {OpCodes.Conv_Ovf_U8, new ConvU8StackMutator(OpCodes.Conv_Ovf_U8)},
                {OpCodes.Conv_Ovf_U8_Un, new ConvU8StackMutator(OpCodes.Conv_Ovf_U8_Un)},
                {OpCodes.Ldnull, new LdnullStackMutator()},
                {OpCodes.Ldloc, new LdlocStackMutator()},
                {OpCodes.Ldloc_S, new LdlocStackMutator()},
                {OpCodes.Ldloca, new LdlocaStackMutator()},
                {OpCodes.Ldloca_S, new LdlocaStackMutator()},
                {OpCodes.Stloc, new StlocStackMutator()},
                {OpCodes.Stloc_S, new StlocStackMutator()},
                {OpCodes.Ldarg_0, new Ldarg_0StackMutator()},
                {OpCodes.Ldarg_1, new Ldarg_1StackMutator()},
                {OpCodes.Ldarg_2, new Ldarg_2StackMutator()},
                {OpCodes.Ldarg_3, new Ldarg_3StackMutator()},
                {OpCodes.Ldarg_S, new Ldarg_SStackMutator()},
                {OpCodes.Ldarg, new LdargStackMutator()},
                {OpCodes.Ldarga_S, new Ldarga_SStackMutator()},
                {OpCodes.Ldarga, new LdargaStackMutator()},
                {OpCodes.Starg, new StargStackMutator()},
                {OpCodes.Starg_S, new Starg_SStackMutator()},
                {OpCodes.Arglist, new ArglistStackMutator()},
                {OpCodes.Ldfld, new LdfldStackMutator()},
                {OpCodes.Ldsfld, new LdfldStackMutator()},
                {OpCodes.Ldflda, new LdfldaStackMutator()},
                {OpCodes.Ldsflda, new LdfldaStackMutator()},
                {OpCodes.Stfld, new StfldStackMutator()},
                {OpCodes.Stsfld, new StfldStackMutator()},
                {OpCodes.Ldind_I1, new LdindStackMutator()},
                {OpCodes.Ldind_I2, new LdindStackMutator()},
                {OpCodes.Ldind_I4, new LdindStackMutator()},
                {OpCodes.Ldind_I8, new LdindStackMutator()},
                {OpCodes.Ldind_R4, new LdindStackMutator()},
                {OpCodes.Ldind_R8, new LdindStackMutator()},
                {OpCodes.Ldind_Ref, new LdindStackMutator()},
                {OpCodes.Ldind_U1, new LdindStackMutator()},
                {OpCodes.Ldind_U2, new LdindStackMutator()},
                {OpCodes.Ldind_U4, new LdindStackMutator()},
                {OpCodes.Stind_I1, new StindStackMutator()},
                {OpCodes.Stind_I2, new StindStackMutator()},
                {OpCodes.Stind_I4, new StindStackMutator()},
                {OpCodes.Stind_I8, new StindStackMutator()},
                {OpCodes.Stind_R4, new StindStackMutator()},
                {OpCodes.Stind_R8, new StindStackMutator()},
                {OpCodes.Stind_Ref, new StindStackMutator()},
                {OpCodes.Ldobj, new LdindStackMutator()},
                {OpCodes.Stobj, new StindStackMutator()},
                {OpCodes.Ldftn, new LdftnStackMutator()},
                {OpCodes.Ldvirtftn, new LdvirtftnStackMutator()},
                {OpCodes.Ldtoken, new LdtokenStackMutator()},
                {OpCodes.Castclass, new CastclassStackMutator()},
                {OpCodes.Isinst, new IsinstStackMutator()},
                {OpCodes.Unbox_Any, new Unbox_AnyStackMutator()},
                {OpCodes.Box, new BoxStackMutator()},
                {OpCodes.Newobj, new NewobjStackMutator()},
                {OpCodes.Initobj, new InitobjStackMutator()},
                {OpCodes.Cpobj, new CpobjStackMutator()},
                {OpCodes.Cpblk, new CpblkStackMutator()},
                {OpCodes.Initblk, new InitblkStackMutator()},
                {OpCodes.Newarr, new NewarrStackMutator()},
                {OpCodes.Throw, new ThrowStackMutator()},
                {OpCodes.Rethrow, new RethrowStackMutator()},
                {OpCodes.Ldlen, new LdlenStackMutator()},
                {OpCodes.Ldelema, new LdelemaStackMutator()},
                {OpCodes.Ldelem, new LdelemStackMutator()},
                {OpCodes.Ldelem_I, new LdelemStackMutator()},
                {OpCodes.Ldelem_I1, new LdelemStackMutator()},
                {OpCodes.Ldelem_I2, new LdelemStackMutator()},
                {OpCodes.Ldelem_I4, new LdelemStackMutator()},
                {OpCodes.Ldelem_I8, new LdelemStackMutator()},
                {OpCodes.Ldelem_R4, new LdelemStackMutator()},
                {OpCodes.Ldelem_R8, new LdelemStackMutator()},
                {OpCodes.Ldelem_Ref, new LdelemStackMutator()},
                {OpCodes.Ldelem_U1, new LdelemStackMutator()},
                {OpCodes.Ldelem_U2, new LdelemStackMutator()},
                {OpCodes.Ldelem_U4, new LdelemStackMutator()},
                {OpCodes.Stelem, new StelemStackMutator()},
                {OpCodes.Stelem_I, new StelemStackMutator()},
                {OpCodes.Stelem_I1, new StelemStackMutator()},
                {OpCodes.Stelem_I2, new StelemStackMutator()},
                {OpCodes.Stelem_I4, new StelemStackMutator()},
                {OpCodes.Stelem_I8, new StelemStackMutator()},
                {OpCodes.Stelem_R4, new StelemStackMutator()},
                {OpCodes.Stelem_R8, new StelemStackMutator()},
                {OpCodes.Stelem_Ref, new StelemStackMutator()},
                {OpCodes.Ckfinite, new CkfiniteStackMutator()},
                {OpCodes.Localloc, new LocallocStackMutator()},
            };

        private static readonly MarkLabelStackMutator markLabelStackMutator = new MarkLabelStackMutator();
    }
}