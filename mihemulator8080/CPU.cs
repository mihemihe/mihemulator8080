using System.Diagnostics;

namespace mihemulator8080
{
    public static class CPU
    {
        // CPU registers A,B,C,D,E - 8 bits each
        public static byte registerA, registerB, registerC, registerD, registerE, registerH, registerL;

        //private Flag register(F) bits:

        //7	6	5	4	3	2	1	0
        //S Z	0	A	0	P	1	C

        //S - Sign Flag
        //Z - Zero Flag
        //0 - Not used, always zero
        //A - also called AC, Auxiliary Carry Flag
        //0 - Not used, always zero
        //P - Parity Flag
        //1 - Not used, always one
        //C - Carry Flag

        //bitarray better?

        public static bool SignFlag, ZeroFlag, AuxCarryFlag, ParityFlag, CarryFlag;

        public static int programCounter; // (PC) An ancient Instruction Pointer

        public static InstructionFetcher instructionFecther;

        static CPU()
        {
            instructionFecther = new InstructionFetcher();
        }

        public static InstructionOpcodes GetNextInstruction()
        {
            // read the program counter
            // read next instruction frm membory, offset rogram counter


            if (programCounter < Memory.TextSectionSize)
            {
                InstructionOpcodes codes = new InstructionOpcodes(
            Memory.RAMMemory[programCounter],
            Memory.RAMMemory[programCounter + 1],
            Memory.RAMMemory[programCounter + 2]);
                CPU.instructionFecther.DisassembleInstruction(codes, out int size);
                programCounter += size;
                return codes;
            }
            else return new InstructionOpcodes(5, 5, 5);
        }

        public static int ExecuteInstruction(InstructionOpcodes opCodes)
        {
            return 0;
        }

        public static int Cycle()
        {
            InstructionOpcodes nextInstruction = CPU.GetNextInstruction();
            int cycleResult = ExecuteInstruction(nextInstruction);
            //Debug.Write(nextInstruction.Byte1 + "\n");
            return 0;

        }
    }
}