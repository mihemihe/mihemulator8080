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

        // I think this should be uint. If they go over 27......... value they will overflow
        // However only 65535 values are required (2-16) to map all memory
        // if all memory is used it will fail !
        public static int programCounter; // (PC) An ancient Instruction Pointer
        public static int stackPointer; // (SP) Stack Pointer

        public static InstructionFetcher instructionFecther;
        public static string InstructionExecuting;

        static CPU()
        {
            instructionFecther = new InstructionFetcher();
            InstructionExecuting = "";
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
                InstructionExecuting =  CPU.instructionFecther.DisassembleInstruction(codes, out int size);
                
                programCounter += size;
                return codes;
            }
            else return new InstructionOpcodes(5, 5, 5);
        }

        public static int ExecuteInstruction(InstructionOpcodes opCodes)
        {


            switch (opCodes.Byte1)
            {

                case 0x00: //NOP, do nothing
                    break;
                case 0x01: //LXI    B,#${byte3}{byte2}
                    CPU.registerC = opCodes.Byte2;
                    CPU.registerB = opCodes.Byte3;
                    break;
                case 0x05: //DCR B ************************************
                    break;
                case 0xC3: //JMP    ${byte3}{byte2}
                    CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                    CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right 
                    break;
                case 0x31: //LXI    SP,#${byte3}{byte2}
                    break;
                default:
                    break;
            }


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