using System;
using System.Collections;

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

        public static int memoryAddressDE;
        public static int memoryAddressHL;
        public static byte[] tempBytesStorage;
        public static byte byteOperation;
        public static BitArray bitArrayOperation;

        public static InstructionFetcher instructionFecther;
        public static string InstructionExecuting;

        static CPU()
        {
            instructionFecther = new InstructionFetcher();
            InstructionExecuting = "";
            memoryAddressDE = 0;
            memoryAddressHL = 0;
            tempBytesStorage = new byte[4];
            byteOperation = 0;
            bitArrayOperation = new BitArray(8, false);
            SignFlag = false;
            ZeroFlag = false;
            AuxCarryFlag = false;
            ParityFlag = false;
            CarryFlag = false;
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
                InstructionExecuting = CPU.instructionFecther.DisassembleInstruction(codes, out int size);

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

                case 0x05: //DCR B "Z, S, P, AC flags affected"
                    byteOperation = 0;
                    byteOperation = (byte)(CPU.registerB - 1); //need to cast because + operator creates int. byte does not have +
                    CPU.ZeroFlag = (byteOperation == 0) ? true : false;
                    CPU.SignFlag = (0x80 == (byteOperation & 0x80)); //0x80 = 128 (10000000) Most Significant bit
                                                                     //  if 8th bit is 1, the & will preserve and the result will be 0x80 
                    bitArrayOperation = new BitArray(new byte[] { byteOperation });
                    int evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;

                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity
                    CPU.AuxCarryFlag = true; //SpaceInvaders does not use it. TODO: Implement in full 8080 emulator
                    break;


                case 0x06: //MVI    B,#${byte2}
                    CPU.registerB = opCodes.Byte2;
                    break;

                case 0x11: //LXI    D,#${byte3}{byte2}
                    CPU.registerE = opCodes.Byte2;
                    CPU.registerD = opCodes.Byte3;
                    break;

                case 0x13: //INX    D
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    memoryAddressDE++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressDE);
                    CPU.registerD = tempBytesStorage[1];
                    CPU.registerE = tempBytesStorage[0];
                    break;

                case 0x1A: //LDAX   D - "A <- (DE)"
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    CPU.registerA = Memory.RAMMemory[memoryAddressDE];

                    break;

                case 0x21: //LXI    H,#${byte3}{byte2}
                    CPU.registerL = opCodes.Byte2;
                    CPU.registerH = opCodes.Byte3;
                    break;

                case 0x23: //INX    H
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    memoryAddressHL++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressHL);
                    CPU.registerH = tempBytesStorage[1];
                    CPU.registerL = tempBytesStorage[0];
                    break;

                case 0x77: //MOV    M,A
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    Memory.RAMMemory[memoryAddressHL] = CPU.registerA;

                    break;

                case 0xC3: //JMP    ${byte3}{byte2}
                    CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                    CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    break;
                case 0xC2: //JNZ    ${byte3}{byte2}
                    // if z=false then jump to address
                    if (CPU.ZeroFlag == false)
                    {
                        CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                        CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    }
                    break;

                case 0x31: //LXI    SP,#${byte3}{byte2}
                    CPU.stackPointer = opCodes.Byte3 << 8;
                    CPU.stackPointer = CPU.stackPointer | opCodes.Byte2;
                    break;

                case 0xCD: //CALL   ${byte3}{byte2}
                    // This is a PUSH to stack, fixed start address in the code, $2400
                    // for clarity , better of use returnaddress, but point is programCounter contains already pc  +2
                    //int returnAddress = CPU.programCounter; // no need +2 because it is incremented already
                    Memory.RAMMemory[CPU.stackPointer - 1] = opCodes.Byte2; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = opCodes.Byte3;
                    CPU.stackPointer = CPU.stackPointer - 2;
                    CPU.programCounter = opCodes.Byte3 << 8; // This is a JMP
                    CPU.programCounter = CPU.programCounter | opCodes.Byte2;
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