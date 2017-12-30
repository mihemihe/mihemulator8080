using System;
using System.Collections;
using System.IO;

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
        public static int returnAddress;
        public static int evenOddCounter;

        public static InstructionFetcher instructionFecther;
        public static string InstructionExecuting;

        public static bool fileDebug = true;
        public static string instructionText;
        public static string debugFilePath = @"..\..\..\..\Misc\OutputFiles\ExecutionSecuence.txt";

        static CPU()
        {
            instructionFecther = new InstructionFetcher();
            InstructionExecuting = "";
            memoryAddressDE = 0;
            memoryAddressHL = 0;
            tempBytesStorage = new byte[4];
            byteOperation = 0;
            bitArrayOperation = new BitArray(8, false);
            returnAddress = 0;
            evenOddCounter = 0;
            SignFlag = false;
            ZeroFlag = false;
            AuxCarryFlag = false;
            ParityFlag = false;
            CarryFlag = false;
            instructionText = "";
            if (File.Exists(debugFilePath))
            {
                File.Delete(debugFilePath);
            }
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
            instructionText = "";
            switch (opCodes.Byte1)
            {
                case 0x00: //NOP, do nothing
                    instructionText = $"NOP";
                    break;

                case 0x01: //LXI    B,#${byte3}{byte2}
                    CPU.registerC = opCodes.Byte2;
                    CPU.registerB = opCodes.Byte3;
                    instructionText = $"LXI    B,#${(opCodes.Byte3.ToString("X2"))}{opCodes.Byte2.ToString("X2")}";
                    break;

                case 0x05: //DCR B "Z, S, P, AC flags affected"
                    instructionText = $"DCR    B - B: {CPU.registerB.ToString("X2")}";
                    byteOperation = 0;
                    byteOperation = (byte)(CPU.registerB - 1); //need to cast because + operator creates int. byte does not have +
                    CPU.ZeroFlag = (byteOperation == 0) ? true : false;
                    CPU.SignFlag = (0x80 == (byteOperation & 0x80)); //0x80 = 128 (10000000) Most Significant bit
                                                                     //  if 8th bit is 1, the & will preserve and the result will be 0x80 
                    bitArrayOperation = new BitArray(new byte[] { byteOperation });
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;

                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity
                    CPU.AuxCarryFlag = true; //SpaceInvaders does not use it. TODO: Implement in full 8080 emulator
                    CPU.registerB = byteOperation;                    
                    break;


                case 0x06: //MVI    B,#${byte2}
                    instructionText = $"MVI    B,#${opCodes.Byte2.ToString("X2")}";
                    CPU.registerB = opCodes.Byte2;
                    break;

                case 0x11: //LXI    D,#${byte3}{byte2}
                    instructionText = $"LXI    D,#${opCodes.Byte3.ToString("X2")}{opCodes.Byte2.ToString("X2")}";
                    CPU.registerE = opCodes.Byte2;
                    CPU.registerD = opCodes.Byte3;
                    break;

                case 0x13: //INX    D
                    instructionText = $"INX    D";
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    memoryAddressDE++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressDE);
                    CPU.registerD = tempBytesStorage[1];
                    CPU.registerE = tempBytesStorage[0];
                    break;

                case 0x1A: //LDAX   D - "A <- (DE)"
                    instructionText = "LDAX   D";
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    CPU.registerA = Memory.RAMMemory[memoryAddressDE];

                    break;

                case 0x21: //LXI    H,#${byte3}{byte2}
                    instructionText = $"LXI    H,#${opCodes.Byte3.ToString("X2")}{opCodes.Byte2.ToString("X2")}";
                    CPU.registerL = opCodes.Byte2;
                    CPU.registerH = opCodes.Byte3;
                    break;

                case 0x23: //INX    H
                    instructionText = $"INX    H";
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    memoryAddressHL++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressHL);
                    CPU.registerH = tempBytesStorage[1];
                    CPU.registerL = tempBytesStorage[0];
                    break;

                case 0x77: //MOV    M,A
                    instructionText = $"MOV    M,A";
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    Memory.RAMMemory[memoryAddressHL] = CPU.registerA;

                    break;

                case 0xC3: //JMP    ${byte3}{byte2}
                    instructionText = $"JMP    ${opCodes.Byte3.ToString("X2")}{opCodes.Byte2.ToString("X2")}";
                    CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                    CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    break;

                case 0xC2: //JNZ    ${byte3}{byte2}
                    instructionText = $"JNZ    ${opCodes.Byte3.ToString("X2")}{opCodes.Byte2.ToString("X2")}";
                    // if z=false then jump to address
                    if (CPU.ZeroFlag == false)
                    {
                        CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                        CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    }
                    break;

                case 0x31: //LXI    SP,#${byte3}{byte2}
                    instructionText = $"LXI    SP,#${opCodes.Byte3.ToString("X2")}{opCodes.Byte2.ToString("X2")}";
                    CPU.stackPointer = opCodes.Byte3 << 8;
                    CPU.stackPointer = CPU.stackPointer | opCodes.Byte2;
                    break;

                case 0xC9: //RET
                    instructionText = $"RET";
                    CPU.programCounter = 0;
                    programCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                    programCounter = programCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                    CPU.stackPointer += 2; //return the stack pointer back to original position
                    break;

                case 0xCD: //CALL   ${byte3}{byte2}
                    instructionText = $"CALL   ${opCodes.Byte3.ToString("X2")}{opCodes.Byte2.ToString("X2")}";
                    // This is a PUSH to stack, fixed start address in the code, $2400
                    // for clarity , better of use returnaddress, but point is programCounter contains already pc  +2
                    returnAddress = CPU.programCounter;
                    tempBytesStorage = BitConverter.GetBytes(returnAddress);
                    //This is the PUSH of programCounter + 2 in the stack
                    Memory.RAMMemory[CPU.stackPointer - 1] = tempBytesStorage[1]; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = tempBytesStorage[0];
                    CPU.stackPointer = CPU.stackPointer - 2;

                    //This is the JMP
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

            if (fileDebug == true)
            {
                using (StreamWriter sw = File.AppendText(debugFilePath))
                {
                    sw.WriteLine(instructionText);

                }
            }
            //Debug.Write(nextInstruction.Byte1 + "\n");
            return 0;
        }
    }
}