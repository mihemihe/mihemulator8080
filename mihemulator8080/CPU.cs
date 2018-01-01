using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;

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

        public static int cyclesCounter;

        public static uint HL;
        public static uint DADHResult;
        public static byte[] tempHL;
        public static int instructionSize;
        public static int instructionLine;
        public static Stopwatch stopWatch;
        public static long elapsedTimeMs;
        public static int cyclesPerSecond;
        public static string CPS;
        public static StringBuilder sb;
        public static int linesToAppendCounter;

        static CPU()
        {
            linesToAppendCounter = 0;
            sb = new StringBuilder();
            programCounter = 0x00;
            CPS = "";
            cyclesPerSecond = 0;
            elapsedTimeMs = 0;
            stopWatch = new Stopwatch();
            stopWatch.Start();
            instructionLine = 0;
            instructionSize = 0;
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
            cyclesCounter = 0;
            HL = 0;
            DADHResult = 0;
            tempHL = new byte[2] { 0x00, 0x00 };
            if (File.Exists(debugFilePath))
            {
                File.Delete(debugFilePath);
            }
        }

        public static string CPUStatus()
        {
            int valueTopStack = 0;
            valueTopStack = Memory.RAMMemory[CPU.stackPointer + 1] << 8;
            valueTopStack = valueTopStack | Memory.RAMMemory[CPU.stackPointer];
            return $"A:0x{CPU.registerA.ToString("X2")} " +
                   $"B:0x{CPU.registerB.ToString("X2")} " +
                   $"C:0x{CPU.registerC.ToString("X2")} " +
                   $"DE:0x{CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")} " +
                   $"HL:0x{CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")} -- " +
                   $"S:{Convert.ToInt32(CPU.SignFlag)} " +
                   $"Z:{Convert.ToInt32(CPU.ZeroFlag)} " +
                   $"C:{Convert.ToInt32(CPU.CarryFlag)} " +
                   $"P:{Convert.ToInt32(CPU.ParityFlag)} " +
                   $"Aux:{Convert.ToInt32(CPU.AuxCarryFlag)} " +
                   $"SP:${CPU.stackPointer.ToString("X4")}->({valueTopStack.ToString("X4")})";
            //TODO: Add stack size too, it will be helpful to debug
        }

        public static InstructionOpcodes GetNextInstruction()
        {
            // read the program counter
            // read next instruction frm membory, offset rogram counter
            instructionLine = CPU.programCounter;
            if (programCounter < Memory.TextSectionSize)
            {
                InstructionOpcodes codes = new InstructionOpcodes(
            Memory.RAMMemory[programCounter],
            Memory.RAMMemory[programCounter + 1],
            Memory.RAMMemory[programCounter + 2]);
                InstructionExecuting = CPU.instructionFecther.DisassembleInstruction(codes, out instructionSize);

                programCounter += instructionSize;
                return codes;
            }
            else return new InstructionOpcodes(5, 5, 5);
        }

        public static int ExecuteInstruction(InstructionOpcodes opCodes)
        {
            instructionText = "";
            string byte1txt = opCodes.Byte1.ToString("X2");
            string byte2txt = opCodes.Byte2.ToString("X2");
            string byte3txt = opCodes.Byte3.ToString("X2");

            switch (opCodes.Byte1)
            {
                case 0x00: //NOP, do nothing
                    instructionText = $"{byte1txt}\t\tNOP\t\t\t; No operation" + "\t\t\t\t" + CPU.CPUStatus();
                    break;

                case 0x01: //LXI    B,#${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tLXI    B,#${(byte3txt)}{byte2txt}\t\t; {(byte3txt)}{byte2txt} to BC" + "\t\t\t\t" + CPU.CPUStatus();
                    CPU.registerC = opCodes.Byte2;
                    CPU.registerB = opCodes.Byte3;                    
                    break;

                case 0x05: //DCR B "Z, S, P, AC flags affected"
                    instructionText = $"{byte1txt}\t\tDCR    B\t\t; Decrement B({CPU.registerB.ToString("X2")}) and update ZSPAC" + "\t" + CPU.CPUStatus();
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
                    instructionText = $"{byte1txt} {byte2txt}\t\tMVI    B,#${byte2txt}\t\t; Move Byte2(0x{byte2txt}) to B(0x{CPU.registerB.ToString("X2")})" + "\t\t" + CPU.CPUStatus();
                    CPU.registerB = opCodes.Byte2;
                    break;

                case 0x09: //DAD    B //double add, sums HL + BC, in their byte positions and compare. CY flag
                    HL = 0;
                    HL = (uint)((CPU.registerH << 8) | CPU.registerL);
                    uint BC = (uint)((CPU.registerB << 8) | CPU.registerC);
                    uint DADBResult = HL + BC;
                    instructionText = $"{byte1txt}\t\tDAD    B\t\t; HL(0x{HL.ToString("X4")})+DE(0x{BC.ToString("X4")})=(0x{DADBResult.ToString("X4")})->HL CY" + "\t" + CPU.CPUStatus();
                    CPU.registerH = (byte)((DADBResult & 0xFF00) >> 8);
                    CPU.registerL = (byte)(DADBResult & 0xFF);
                    CPU.CarryFlag = ((DADBResult & 0xFFFF0000) != 0);
                    break;



                case 0x0D: //DCR C "Z, S, P, AC flags affected"
                    instructionText = $"{byte1txt}\t\tDCR    C\t\t; Decrement C({CPU.registerC.ToString("X2")}) and update ZSPAC" + "\t" + CPU.CPUStatus();
                    byteOperation = 0;
                    byteOperation = (byte)(CPU.registerC - 1); //need to cast because + operator creates int. byte does not have +
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
                    CPU.registerC = byteOperation;
                    break;

                case 0x11: //LXI    D,#${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tLXI    D,#${byte3txt}{byte2txt}\t\t; Load 0x{byte3txt}{byte2txt} on DE" + "\t\t\t" + CPU.CPUStatus();
                    CPU.registerD = opCodes.Byte3;
                    CPU.registerE = opCodes.Byte2;
                    break;

                case 0x14: //INR    D
                    //Z S P Aux
                    instructionText = $"INR    D";
                    CPU.registerD = (byte)(CPU.registerD + 1);
                    CPU.ZeroFlag = (CPU.registerD == 0);
                    CPU.SignFlag = (0x80 == (CPU.registerD & 0x80));
                    bitArrayOperation = new BitArray(new byte[] { CPU.registerD });
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;
                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity
                    CPU.AuxCarryFlag = true; //SpaceInvaders does not use it. TODO: Implement in full 8080 emulator

                    break;

                case 0x0E: //MVI    C,#${byte2}
                    instructionText = $"{byte1txt} {byte2txt}\t\tMVI    C,#${byte2txt}\t\t; Move Byte2(0x{byte2txt}) to C(0x{CPU.registerC.ToString("X2")})" + "\t\t" + CPU.CPUStatus();
                    CPU.registerC = opCodes.Byte2;
                    break;

                case 0x13: //INX    D
                    instructionText = $"{byte1txt}\t\tINX    D\t\t; Increments DE({CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")}) + 1" + "\t\t" + CPU.CPUStatus();
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    memoryAddressDE++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressDE);
                    CPU.registerD = tempBytesStorage[1];
                    CPU.registerE = tempBytesStorage[0];
                    break;

                case 0x19: //DAD    D //double add, sums HL + DE, in their byte positions and compare. CY flag
                    HL = 0;
                    HL = (uint)((CPU.registerH << 8) | CPU.registerL);
                    uint DE = (uint)((CPU.registerD << 8) | CPU.registerE);
                    uint DADDResult = HL + DE;
                    instructionText = $"{byte1txt}\t\tDAD    D\t\t; HL(0x{HL.ToString("X4")})+DE(0x{DE.ToString("X4")})=(0x{DADDResult.ToString("X4")})->HL CY" + "\t" + CPU.CPUStatus();
                    CPU.registerH = (byte)((DADDResult & 0xFF00) >> 8);
                    CPU.registerL = (byte)(DADDResult & 0xFF);
                    CPU.CarryFlag = ((DADDResult & 0xFFFF0000) != 0);
                    break;

                case 0x1A: //LDAX   D - "A <- (DE)"
                    instructionText = $"{byte1txt}\t\tLDAX   D\t\t; Copy $DE(${CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")}) value()->A(0x{CPU.registerA.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    CPU.registerA = Memory.RAMMemory[memoryAddressDE];
                    instructionText = instructionText.Replace("value()", $"value(0x{Memory.RAMMemory[memoryAddressDE].ToString("X2")})");

                    break;

                case 0x21: //LXI    H,#${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tLXI    H,#${byte3txt}{byte2txt}\t\t; Load 0x{byte3txt}{byte2txt} on HL" + "\t\t\t" + CPU.CPUStatus();
                    CPU.registerH = opCodes.Byte3;
                    CPU.registerL = opCodes.Byte2;
                    break;

                case 0x23: //INX    H
                    instructionText = $"{byte1txt}\t\tINX    H\t\t; Increments HL({CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")}) + 1" + "\t\t" + CPU.CPUStatus();
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    memoryAddressHL++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressHL);
                    CPU.registerH = tempBytesStorage[1];
                    CPU.registerL = tempBytesStorage[0];
                    break;

                case 0x26: //MVI    H,#${byte2}
                    instructionText = $"{byte1txt} {byte2txt}\t\tMVI    H,#${byte2txt}\t\t; Move Byte2(0x{byte2txt}) to H(0x{CPU.registerH.ToString("X2")})" + "\t\t" + CPU.CPUStatus();
                    CPU.registerH = opCodes.Byte2;
                    break;

                case 0x29: //DAD    H
                    HL = 0;
                    HL = (uint)(CPU.registerH << 8 | CPU.registerL);
                    DADHResult = HL + HL;
                    instructionText = $"{byte1txt}\t\tDAD    H\t\t; HL(0x{HL.ToString("X4")})+HL(0x{HL.ToString("X4")})=(0x{DADHResult.ToString("X4")})->HL CY" + "\t" + CPU.CPUStatus(); //double add, doubles L doubles H, sums them and compare. CY flag
                    CPU.registerH = (byte)((DADHResult & 0xFF00) >> 8);
                    CPU.registerL = (byte)(DADHResult & 0xFF);
                    CPU.CarryFlag = ((DADHResult & 0xFFFF0000) != 0);
                    break;

                case 0x31: //LXI    SP,#${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tLXI    SP,#${byte3txt}{byte2txt}\t; Load 0x{byte3txt}{byte2txt} on Stack Pointer" + "\t\t" + CPU.CPUStatus();
                    CPU.stackPointer = opCodes.Byte3 << 8;
                    CPU.stackPointer = CPU.stackPointer | opCodes.Byte2;
                    break;

                case 0x36: //MVI    M,#${byte2}   M means memory address of HL in this case!!!
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    instructionText = $"{byte1txt} {byte2txt}\t\tMVI    M,#${byte2txt}\t\t; Move Byte2({byte2txt}) to $ in HL $({memoryAddressHL.ToString("X4")})" + "\t" + CPU.CPUStatus();
                    Memory.RAMMemory[memoryAddressHL] = opCodes.Byte2;
                    break;

                case 0x6f: //MOV    L,A
                    instructionText = $"{byte1txt}\t\tMOV    L,A\t\t; Move A(0x{CPU.registerA.ToString("X2")}) to L(0x{CPU.registerL.ToString("X2")})(L Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerL = CPU.registerA;
                    break;

                //case 0x6f: //
                //    instructionText = $"";
                //    break;

                case 0x77: //MOV    M,A
                    instructionText = $"{byte1txt}\t\tMOV    M,A\t\t; Move A({CPU.registerA.ToString("X2")}) to address in $HL(${CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    Memory.RAMMemory[memoryAddressHL] = CPU.registerA;
                    break;

                case 0x7c: //MOV    A,H
                    instructionText = $"{byte1txt}\t\tMOV    A,H\t\t; Move H(0x{CPU.registerH.ToString("X2")}) to A(0x{CPU.registerA.ToString("X2")})(A Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerA = CPU.registerH;
                    break;

                case 0xC1: //POP    B
                    instructionText = $"POP    B";
                    instructionText = $"{byte1txt}\t\tPOP    B\t\t; Stack (0x{Memory.RAMMemory[CPU.stackPointer + 1].ToString("X2")}{Memory.RAMMemory[CPU.stackPointer].ToString("X2")})" +
                                      $" to BC (0x{CPU.registerB.ToString("X2")}{CPU.registerC.ToString("X2")}). SP+2" + "\t" + CPU.CPUStatus();
                    CPU.registerB = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerC = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;

                    break;

                case 0xC2: //JNZ    ${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tJNZ    ${byte3txt}{byte2txt}\t\t; jump to ${byte3txt}{byte2txt} if ZeroFlag({Convert.ToInt32(CPU.ZeroFlag)}) == 0" + "\t" + CPU.CPUStatus();
                    // if z=false then jump to address
                    if (CPU.ZeroFlag == false)
                    {
                        //This is a jump to the address
                        CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                        CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    }
                    break;

                case 0xC3: //JMP    ${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tJMP    ${byte3txt}{byte2txt}\t\t; jump to ${byte3txt}{byte2txt}" + "\t\t\t\t" + CPU.CPUStatus();
                    CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                    CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    break;

                case 0xC9: //RET

                    CPU.programCounter = 0;
                    programCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                    programCounter = programCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                    instructionText = $"{byte1txt}\t\tRET\t\t\t; Jump to ret $ in SP->${programCounter.ToString("X4")},  SP +2" + "\t" + CPU.CPUStatus();
                    CPU.stackPointer += 2; //return the stack pointer back to original position
                    break;

                case 0xC5: //PUSH   B - BC move to the stack
                    instructionText = $"{byte1txt}\t\tPUSH   B\t\t; BC(0x{CPU.registerB.ToString("X2")}{CPU.registerC.ToString("X2")}) to stack. Stack -2" + "\t\t" + CPU.CPUStatus();
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerB; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerC;
                    CPU.stackPointer -= 2;
                    break;

                case 0xCD: //CALL   ${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tCALL   ${byte3txt}{byte2txt}\t\t; Jump->${byte3txt}{byte2txt}, ret ${CPU.programCounter.ToString("X4")}->stack, SP -2"
                        + "\t" + CPU.CPUStatus(); ;
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

                case 0xD3: //OUT    #${byte2}
                    instructionText = $"OUT    #${byte2txt} Out to device, sound???";
                    break;

                case 0xD1: //POP    D -- POP stack to DE
                    instructionText = $"{byte1txt}\t\tPOP    D\t\t; Stack (0x{Memory.RAMMemory[CPU.stackPointer + 1].ToString("X2")}{Memory.RAMMemory[CPU.stackPointer].ToString("X2")})" +
                                      $" to DE (0x{CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")}). SP+2" + "\t" + CPU.CPUStatus();
                    CPU.registerD = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerE = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;
                    break;

                case 0xD5: //PUSH   D - DE move to the stack //TODO: Finally found a bug here, I was using + 2!!!! check again if the order is correct, and matches with POP
                    instructionText = $"{byte1txt}\t\tPUSH   D\t\t; DE(0x{CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")}) to stack. Stack -2" + "\t\t" + CPU.CPUStatus();
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerD; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerE;
                    CPU.stackPointer -= 2;
                    break;

                case 0xE1: //POP    H -- POP stack to HL
                    instructionText = $"{byte1txt}\t\tPOP    H\t\t; Stack (0x{Memory.RAMMemory[CPU.stackPointer + 1].ToString("X2")}{Memory.RAMMemory[CPU.stackPointer].ToString("X2")})" +
                                      $" to HL (0x{CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")}). SP+2" + "\t" + CPU.CPUStatus();
                    CPU.registerH = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerL = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;
                    break;

                case 0xEB: //XCHG - Swaps HL by DE, in this particular order
                    instructionText = $"{byte1txt}\t\tXCHG\t\t\t; Swap HL({CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")}) and DE({CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")})" + "\t\t" + CPU.CPUStatus();
                    tempHL = new byte[] { CPU.registerH, CPU.registerL };
                    CPU.registerH = CPU.registerD;
                    CPU.registerL = CPU.registerE;
                    CPU.registerD = tempHL[0];
                    CPU.registerE = tempHL[1];
                    break;

                case 0xE5: //PUSH   H - HL move to the stack
                    instructionText = $"{byte1txt}\t\tPUSH   H\t\t; HL(0x{ CPU.registerH.ToString("X2")}{ CPU.registerL.ToString("X2")}) to stack. Stack -2" + "\t\t" + CPU.CPUStatus();
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerH; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerL;
                    CPU.stackPointer -= 2;
                    break;

                case 0xFE: //CPI    #${byte2} //compare immediate with accumulator A, substracting
                    //	Z, S, P, CY, AC
                    instructionText = $"{byte1txt} {byte2txt}\t\tCPI    #${byte2txt}\t\t; Compare A(0x{CPU.registerA.ToString("X2")})-Byte2(0x{byte2txt}). Flags" + "\t" + CPU.CPUStatus();
                    byteOperation = 0;
                    byteOperation = (byte)(CPU.registerA - opCodes.Byte2);
                    CPU.ZeroFlag = (byteOperation == 0) ? true : false;
                    CPU.SignFlag = (0x80 == (byteOperation & 0x80));
                    bitArrayOperation = new BitArray(new byte[] { byteOperation });
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;
                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity
                    CPU.AuxCarryFlag = true; //SpaceInvaders does not use it. TODO: Implement in full 8080 emulator
                    //This is the CY operation
                    CPU.CarryFlag = (opCodes.Byte2 > CPU.registerA) ? true : false;
                    break;

                default:
                    break;
            }

            return 0;
        }

        //TODO: Implement a table for parity, instead of that annoying loop

        public static int Cycle()
        {
            InstructionOpcodes nextInstruction = CPU.GetNextInstruction();
            int cycleResult = ExecuteInstruction(nextInstruction);
            elapsedTimeMs += stopWatch.ElapsedMilliseconds;
            stopWatch.Restart();

            if (elapsedTimeMs > 1000)
            {
                CPS = cyclesPerSecond.ToString();
                elapsedTimeMs = elapsedTimeMs - 1000;
                CPU.cyclesPerSecond = 0;
            }

            if (fileDebug == true && cyclesCounter > -1)
            {
                sb.AppendLine("C: " + CPU.cyclesCounter.ToString("D7") + " $" +
                        instructionLine.ToString("X4") + " " +
                        instructionText);
                linesToAppendCounter++;
            }

            //line counter to file:
            //if (cyclesCounter % 1000 == 0)
            //{
            //    sb.AppendLine(cyclesCounter.ToString() + "****************************************************");
            //    linesToAppendCounter++;
            //}

            if (linesToAppendCounter > 500)
            {
                using (StreamWriter sw = File.AppendText(debugFilePath))
                {
                    sw.Write(sb.ToString());
                    sb.Clear();
                }
                linesToAppendCounter = 0;
                //Debug.Write(nextInstruction.Byte1 + "\n");
            }
            return 0;
        }
    }
}