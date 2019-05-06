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
        public static int stackSize;

        public static string comment;
        public static int memoryAddressDE;
        public static int memoryAddressHL;
        public static int memoryAddressBC;
        public static int memoryAddressImmediate;
        public static byte[] tempBytesStorage;
        public static byte byteOperation;
        public static ushort uInt16Operation;
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
            stackSize = 0;
            comment = "";
            uInt16Operation = 0;
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
            memoryAddressBC = 0;
            memoryAddressImmediate = 0;
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
                   $"SP:${CPU.stackPointer.ToString("X4")}->({valueTopStack.ToString("X4")}) StackSize({stackSize.ToString("D2")})";
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

                case 0x03: //INX    B
                    instructionText = $"{byte1txt}\t\tINX    B\t\t; Increments BC({CPU.registerB.ToString("X2")}{CPU.registerC.ToString("X2")}) + 1" + "\t\t" + CPU.CPUStatus();
                    memoryAddressBC = 0;
                    memoryAddressBC = CPU.registerB << 8;
                    memoryAddressBC = memoryAddressBC | CPU.registerC;
                    memoryAddressBC++;
                    tempBytesStorage = BitConverter.GetBytes(memoryAddressBC);
                    CPU.registerB = tempBytesStorage[1];
                    CPU.registerC = tempBytesStorage[0];
                    break;

                case 0x05: //DCR    B"Z, S, P, AC flags affected"
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

                case 0x0A: //LDAX   B - "A <- (BC)"
                    instructionText = $"{byte1txt}\t\tLDAX   B\t\t; Copy $BC(${CPU.registerB.ToString("X2")}{CPU.registerC.ToString("X2")}) value()->A(0x{CPU.registerA.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    memoryAddressBC = 0;
                    memoryAddressBC = CPU.registerB << 8;
                    memoryAddressBC = memoryAddressBC | CPU.registerC;
                    CPU.registerA = Memory.RAMMemory[memoryAddressBC];
                    instructionText = instructionText.Replace("value()", $"value(0x{Memory.RAMMemory[memoryAddressBC].ToString("X2")})");
                    break;

                case 0x0D: //DCR    C"Z, S, P, AC flags affected"
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

                case 0x0F: //RRC //TODO: I just copied this instruction. Investigate more how it works and why exists
                    //uint8_t x = state->a;
                    //state->a = ((x & 1) << 7) | (x >> 1);
                    //state->cc.cy = (1 == (x & 1));
                    instructionText = $"{byte1txt}\t\tRRC\t\t\t; Rotate A Right" + "\t\t\t" + CPU.CPUStatus();
                    byteOperation = 0;
                    byteOperation = CPU.registerA;
                    CPU.registerA = (byte)((byteOperation & 1) << 7 | (byteOperation >> 1));
                    CPU.CarryFlag = (1 == (byteOperation & 1));
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
                case 0x32: //STA    ${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tSTA    ${byte3txt}{byte2txt}\t\t; Store A(0x{CPU.registerA.ToString("X2")}) to ${byte3txt}{byte2txt}" + "\t\t" + CPU.CPUStatus();
                    memoryAddressImmediate = 0;
                    memoryAddressImmediate = opCodes.Byte3 << 8;
                    memoryAddressImmediate = memoryAddressImmediate | opCodes.Byte2;
                    Memory.RAMMemory[memoryAddressImmediate] = CPU.registerA;
                    break;

                case 0x36: //MVI    M,#${byte2}   M means memory address of HL in this case!!!
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    instructionText = $"{byte1txt} {byte2txt}\t\tMVI    M,#${byte2txt}\t\t; Move Byte2({byte2txt}) to $ in HL $({memoryAddressHL.ToString("X4")})" + "\t" + CPU.CPUStatus();
                    Memory.RAMMemory[memoryAddressHL] = opCodes.Byte2;
                    break;

                case 0x37: //STC
                    instructionText = $"{byte1txt}\t\tSTC\t\t\t; Carry flag({CPU.CarryFlag.ToString()}) to 1" + "\t\t\t" + CPU.CPUStatus();
                    CPU.CarryFlag = true;
                    break;

                case 0x3A: //LDA    ${byte3}{byte2} 	A <- (adr)

                    memoryAddressImmediate = 0;
                    memoryAddressImmediate = opCodes.Byte3 << 8;
                    memoryAddressImmediate = memoryAddressImmediate | opCodes.Byte2;
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tLDA    ${byte3txt}{byte2txt}\t\t; Load A({CPU.registerA.ToString("X2")}) with value({Memory.RAMMemory[memoryAddressImmediate]}) in ${memoryAddressImmediate.ToString("X4")}" + "\t" + CPU.CPUStatus();
                    CPU.registerA = Memory.RAMMemory[memoryAddressImmediate];
                    break;

                case 0x3D: //DCR    A"Z, S, P, AC flags affected"
                    instructionText = $"{byte1txt}\t\tDCR    A\t\t; Decrement A({CPU.registerA.ToString("X2")}) and update ZSPAC" + "\t" + CPU.CPUStatus();
                    byteOperation = 0;
                    byteOperation = (byte)(CPU.registerA - 1); //need to cast because + operator creates int. byte does not have +
                    CPU.ZeroFlag = (byteOperation == 0) ? true : false; //TODO BUG TRACK, it stops in the P of PLAY
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
                    CPU.registerA = byteOperation;
                    break;

                case 0x3E: //MVI    A,#${byte2}
                    instructionText = $"{byte1txt} {byte2txt}\t\tMVI    A,#${byte2txt}\t\t; Move Byte2(0x{byte2txt}) to A(0x{CPU.registerB.ToString("X2")})" + "\t\t" + CPU.CPUStatus();
                    CPU.registerA = opCodes.Byte2;
                    break;

                case 0x4F: //MOV    C,A
                    instructionText = $"{byte1txt}\t\tMOV    C,A\t\t; Move A(0x{CPU.registerA.ToString("X2")}) to C(0x{CPU.registerC.ToString("X2")})(C Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerC = CPU.registerA;
                    break;

                case 0x43: //MOV    B,E
                    /////////////////////////////////instructionText = $"{byte1txt}\t\tMOV    C,A\t\t; Move A(0x{CPU.registerA.ToString("X2")}) to C(0x{CPU.registerC.ToString("X2")})(C Before)" + "\t" + CPU.CPUStatus();
                    break;


                case 0x56: //"MOV    D,M
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    instructionText = $"{byte1txt}\t\tMOV    D,M\t\t; Move value({Memory.RAMMemory[memoryAddressHL].ToString("X2")})" +
                                      $" in HL(${memoryAddressHL.ToString("X4")}) to D({CPU.registerD.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    CPU.registerD = Memory.RAMMemory[memoryAddressHL];
                    break;

                case 0x57: //MOV    D,A
                    instructionText = $"{byte1txt}\t\tMOV    D,A\t\t; Move A(0x{CPU.registerA.ToString("X2")}) to D(0x{CPU.registerD.ToString("X2")})(D Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerD = CPU.registerA;
                    break;

                case 0x5E: //MOV    E,M
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    instructionText = $"{byte1txt}\t\tMOV    E,M\t\t; Move value({Memory.RAMMemory[memoryAddressHL].ToString("X2")})" +
                                      $" in HL(${memoryAddressHL.ToString("X4")}) to E({CPU.registerE.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    CPU.registerE = Memory.RAMMemory[memoryAddressHL];
                    break;

                case 0x5F: //MOV    E,A
                    instructionText = $"{byte1txt}\t\tMOV    E,A\t\t; Move A(0x{CPU.registerA.ToString("X2")}) to E(0x{CPU.registerE.ToString("X2")})(E Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerE = CPU.registerA;
                    break;

                case 0x66: //"MOV    H,M
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    instructionText = $"{byte1txt}\t\tMOV    H,M\t\t; Move value({Memory.RAMMemory[memoryAddressHL].ToString("X2")})" +
                                      $" in HL(${memoryAddressHL.ToString("X4")}) to H({CPU.registerH.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    CPU.registerH = Memory.RAMMemory[memoryAddressHL];
                    break;

                case 0x67: //MOV    H,A
                    instructionText = $"{byte1txt}\t\tMOV    H,A\t\t; Move A(0x{CPU.registerA.ToString("X2")}) to H(0x{CPU.registerH.ToString("X2")})(H Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerH = CPU.registerA;
                    break;

                case 0x6F: //MOV    L,A
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

                case 0x7A: //MOV    A,D
                    instructionText = $"{byte1txt}\t\tMOV    A,D\t\t; Move D(0x{CPU.registerD.ToString("X2")}) to A(0x{CPU.registerA.ToString("X2")})(A Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerA = CPU.registerD;
                    break;

                case 0x7B: //MOV    A,E
                    instructionText = $"{byte1txt}\t\tMOV    A,E\t\t; Move D(0x{CPU.registerE.ToString("X2")}) to A(0x{CPU.registerA.ToString("X2")})(A Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerA = CPU.registerE;
                    break;

                case 0x7C: //MOV    A,H
                    instructionText = $"{byte1txt}\t\tMOV    A,H\t\t; Move H(0x{CPU.registerH.ToString("X2")}) to A(0x{CPU.registerA.ToString("X2")})(A Before)" + "\t" + CPU.CPUStatus();
                    CPU.registerA = CPU.registerH;
                    break;

                case 0x7E: //MOV    A,M
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    instructionText = $"{byte1txt}\t\tMOV    A,M\t\t; Move value({Memory.RAMMemory[memoryAddressHL].ToString("X2")})" +
                                      $" in HL(${memoryAddressHL.ToString("X4")}) to A({CPU.registerA.ToString("X2")})" + "\t" + CPU.CPUStatus();
                    CPU.registerA = Memory.RAMMemory[memoryAddressHL];
                    break;


                case 0xA7: // ANA    A
                    instructionText = $"";
                    instructionText = $"{byte1txt}\t\tANA    A\t\t; A and A - Update Flags ZSPCYAC" + "\t" + CPU.CPUStatus();
                    CPU.registerA = (byte)(CPU.registerA & CPU.registerA); //AND
                    CPU.CarryFlag = false; //Test dangerous, revert to false
                    CPU.AuxCarryFlag = false;
                    CPU.SignFlag = (0b1000_0000 == (CPU.registerA & 0b1000_0000));

                    bitArrayOperation = new BitArray(new byte[] { CPU.registerA });
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;
                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity

                    break;
                case 0xAF: // XRA    A
                    instructionText = $"{byte1txt}\t\tXOR    A\t\t; A xor A - Update Flags ZSPCYAC" + "\t" + CPU.CPUStatus();
                    CPU.registerA = (byte)(CPU.registerA ^ CPU.registerA); //XOR
                    CPU.CarryFlag = false;
                    CPU.AuxCarryFlag = false;
                    CPU.SignFlag = (0x80 == (CPU.registerA & 0x80));

                    bitArrayOperation = new BitArray(new byte[] { CPU.registerA });
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;
                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity

                    //state->cc.cy = state->cc.ac = 0;
                    //state->cc.z = (state->a == 0);
                    //state->cc.s = (0x80 == (state->a & 0x80));
                    //state->cc.p = parity(state->a, 8);
                    break;

                case 0xC1: //POP    B
                    instructionText = $"POP    B";
                    instructionText = $"{byte1txt}\t\tPOP    B\t\t; Stack (0x{Memory.RAMMemory[CPU.stackPointer + 1].ToString("X2")}{Memory.RAMMemory[CPU.stackPointer].ToString("X2")})" +
                                      $" to BC (0x{CPU.registerB.ToString("X2")}{CPU.registerC.ToString("X2")}). SP+2" + "\t" + CPU.CPUStatus();
                    CPU.registerB = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerC = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;
                    stackSize += 2;
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



                case 0xC5: //PUSH   B - BC move to the stack
                    instructionText = $"{byte1txt}\t\tPUSH   B\t\t; BC(0x{CPU.registerB.ToString("X2")}{CPU.registerC.ToString("X2")}) to stack. Stack -2" + "\t\t" + CPU.CPUStatus();
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerB; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerC;
                    CPU.stackPointer -= 2;
                    stackSize -= 2;
                    break;

                case 0xC6: //ADI    #${byte2}
                    instructionText = $"{byte1txt} {byte2txt}\t\tADI    A,#${byte2txt}\t\t; Add Byte2(0x{byte2txt}) to A(0x{CPU.registerA.ToString("X2")})" + "\t\t" + CPU.CPUStatus();

                    uInt16Operation = 0;

                    uInt16Operation = (ushort)(CPU.registerA + opCodes.Byte2);
                    CPU.ZeroFlag = ((uInt16Operation & 0xFF) == 0);
                    CPU.SignFlag = (0x80 == (uInt16Operation & 0x80));

                    tempBytesStorage = BitConverter.GetBytes(uInt16Operation);
                    bitArrayOperation = new BitArray(new byte[] { tempBytesStorage[0] }); //WTF am I doing lol. I should split it and take the low byte
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;
                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity

                    CPU.CarryFlag = uInt16Operation > 0xFF; //TODO CHECK THIS!!!
                    CPU.registerA = (byte)uInt16Operation;

                    //state->cc.z = ((x & 0xff) == 0);
                    //state->cc.s = (0x80 == (x & 0x80));
                    //state->cc.p = parity((x & 0xff), 8);
                    //state->cc.cy = (x > 0xff);
                    //state->a = (uint8_t)x;
                    //state->pc++;
                    break;

                case 0xC8: //RZ                    
                    // if z=true then jump to address
                    if (CPU.ZeroFlag == true)
                    {
                        CPU.programCounter = 0;
                        programCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                        programCounter = programCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                        instructionText = $"{byte1txt}\t\tRZ\t\t\t;if Z=1({Convert.ToInt32(CPU.ZeroFlag)}) Jump ret SP ${programCounter.ToString("X4")}, SP+2" + "\t" + CPU.CPUStatus();
                        CPU.stackPointer += 2; //return the stack pointer back to original position
                        stackSize += 2;
                    }
                    else
                    {
                        int pseudoProgramCounter = 0;
                        pseudoProgramCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                        pseudoProgramCounter = pseudoProgramCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                        instructionText = $"{byte1txt}\t\tRZ\t\t\t;if Z=1({Convert.ToInt32(CPU.ZeroFlag)}) Jump to ret $ in SP->${pseudoProgramCounter.ToString("X4")},  SP +2" + "\t" + CPU.CPUStatus();
                    }
                    break;

                case 0xC9: //RET 

                    CPU.programCounter = 0;
                    programCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                    programCounter = programCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                    instructionText = $"{byte1txt}\t\tRET\t\t\t; Jump to ret $ in SP->${programCounter.ToString("X4")},  SP +2" + "\t" + CPU.CPUStatus();
                    CPU.stackPointer += 2; //return the stack pointer back to original position
                    stackSize += 2;
                    break;


                case 0xCA: //JZ    ${byte3}{byte2} opposite JNZ, if Z true -> jump to immediate $
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tJZ    ${byte3txt}{byte2txt}\t\t; jump to ${byte3txt}{byte2txt} if ZeroFlag({Convert.ToInt32(CPU.ZeroFlag)}) == 1" + "\t" + CPU.CPUStatus();
                    // if z=true then jump to address
                    if (CPU.ZeroFlag == true)
                    {
                        //This is a jump to the address
                        CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                        CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    }


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
                    stackSize -= 2;

                    //This is the JMP
                    CPU.programCounter = opCodes.Byte3 << 8; // This is a JMP
                    CPU.programCounter = CPU.programCounter | opCodes.Byte2;
                    break;

                case 0xD3: //OUT    #${byte2} // ****************** WHAT TO DO HEERER !!!! !
                    instructionText = $"OUT    #${byte2txt} Out to device, sound???";
                    if (opCodes.Byte2 == 0x06)
                    {
                        instructionText = $"{byte1txt} {byte2txt}\t\tOUT    #${byte2txt}\t\t; Feed the WatchDog with byte2??? read or write to reset?({byte2txt})" + "\t" + CPU.CPUStatus();
                        //CPU.programCounter = 0x2400;
                    }
                    break;

                case 0xD1: //POP    D -- POP stack to DE
                    instructionText = $"{byte1txt}\t\tPOP    D\t\t; Stack (0x{Memory.RAMMemory[CPU.stackPointer + 1].ToString("X2")}{Memory.RAMMemory[CPU.stackPointer].ToString("X2")})" +
                                      $" to DE (0x{CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")}). SP+2" + "\t" + CPU.CPUStatus();
                    CPU.registerD = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerE = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;
                    stackSize += 2;
                    break;

                case 0xD5: //PUSH   D - DE move to the stack //TODO: Finally found a bug here, I was using + 2!!!! check again if the order is correct, and matches with POP
                    instructionText = $"{byte1txt}\t\tPUSH   D\t\t; DE(0x{CPU.registerD.ToString("X2")}{CPU.registerE.ToString("X2")}) to stack. Stack -2" + "\t\t" + CPU.CPUStatus();
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerD; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerE;
                    CPU.stackPointer -= 2;
                    stackSize -= 2;
                    break;


                case 0xD8: //RC - if carry true jump to top of stack address
                    instructionText = $"{byte1txt}\t\tRC\t\t; POP top stack and jumps to it. SP+2" + "\t\t" + CPU.CPUStatus();
                    // if cy=true then jump to address on top of stack
                    if (CPU.CarryFlag == true)
                    {
                        //This is a pop + jump to the address
                        CPU.programCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                        CPU.programCounter = CPU.programCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                        CPU.stackPointer += 2;
                        stackSize += 2;
                    }
                    break;

                case 0xDA: //JC     ${byte3}{byte2} jump if carry true(1) //04*JAN debug here TODO TODO TODO 
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tJC - no jump //TODO***";
                    if (CPU.CarryFlag == true)
                    {
                        instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tJC    ${byte3txt}{byte2txt}\t\t; If Carryflag jump to ${byte3txt}{byte2txt}" + "\t\t\t\t" + CPU.CPUStatus();
                        CPU.programCounter = opCodes.Byte3 << 8; //equal to byte3 + 8 bits padded right
                        CPU.programCounter = CPU.programCounter | opCodes.Byte2; // fill the padded 8 bits right
                    }

                    break;

                case 0xE1: //POP    H -- POP stack to HL
                    instructionText = $"{byte1txt}\t\tPOP    H\t\t; Stack (0x{Memory.RAMMemory[CPU.stackPointer + 1].ToString("X2")}{Memory.RAMMemory[CPU.stackPointer].ToString("X2")})" +
                                      $" to HL (0x{CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")}). SP+2" + "\t" + CPU.CPUStatus();
                    CPU.registerH = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerL = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;
                    stackSize += 2;
                    break;

                case 0xE6: //ANI    #${byte2} //TODO: Chec if we are implemeting this right
                    //state->a = state->a & opcode[1];
                    //LogicFlagsA(state);
                    //state->pc++;
                    instructionText = $"{byte1txt} {byte2txt}\t\tANI    #${byte2txt}\t\t; AND immediate with A" + "\t\t\t" + CPU.CPUStatus(); ;
                    CPU.registerA = (byte)(CPU.registerA & opCodes.Byte2);
                    //state->cc.cy = state->cc.ac = 0;
                    //state->cc.z = (state->a == 0);
                    //state->cc.s = (0x80 == (state->a & 0x80));
                    //state->cc.p = parity(state->a, 8);
                    CPU.AuxCarryFlag = false;
                    CPU.CarryFlag = false;
                    if (CPU.registerA == 0) CPU.ZeroFlag = false;
                    CPU.SignFlag = (0x80 == (CPU.registerA & 0x80));

                    bitArrayOperation = new BitArray(new byte[] { CPU.registerA });
                    evenOddCounter = 0; // TODO: take this to a separate parity method
                    foreach (bool bit in bitArrayOperation)
                    {
                        if (bit == true) evenOddCounter++;
                    }
                    CPU.ParityFlag = (evenOddCounter % 2 == 0) ? true : false; // set if even parity
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
                    stackSize -= 2;
                    break;

                case 0xF1: //POP    PSW (sp-2)->-flags; (sp-1)->A; sp -> sp - 2 ZSPCYAux
                    instructionText = $"{byte1txt}\t\tPOP    PSW\t\t; Restore flags and A *******, SP-2\t" + CPU.CPUStatus();
                    //state->a = state->memory[state->sp + 1];
                    //uint8_t psw = state->memory[state->sp];
                    //state->cc.z = (0x01 == (psw & 0x01));
                    //state->cc.s = (0x02 == (psw & 0x02));
                    //state->cc.p = (0x04 == (psw & 0x04));
                    //state->cc.cy = (0x05 == (psw & 0x08));
                    //state->cc.ac = (0x10 == (psw & 0x10));
                    //state->sp += 2;

                    // This is how the push is implemented
                    //                    Convert.ToByte(CPU.ZeroFlag) |
                    //Convert.ToByte(CPU.SignFlag) << 1 |
                    //Convert.ToByte(CPU.ParityFlag) << 2 |
                    //Convert.ToByte(CPU.CarryFlag) << 3 |
                    //Convert.ToByte(CPU.AuxCarryFlag) << 4);
                    CPU.registerA = Memory.RAMMemory[stackPointer + 1];
                    byteOperation = 0;
                    byteOperation = Memory.RAMMemory[stackPointer];
                    CPU.ZeroFlag = ((byteOperation & 0b0000_0001) > 0);
                    CPU.SignFlag = ((byteOperation & 0b0000_0010) > 0);
                    CPU.ParityFlag = ((byteOperation & 0b0000_0100) > 0);
                    CPU.CarryFlag = ((byteOperation & 0b0000_1000) > 0);
                    CPU.AuxCarryFlag = ((byteOperation & 0b0001_0000) > 0);
                    stackPointer += 2;

                    break;

                //TODO: This broke the welcome screen, probabl because there is a POP somewhere next
                case 0xF5: //PUSH   PSW -  (sp-2)<-flags; (sp-1)<-A; sp <- sp - 2 ZSPCYAux                    
                    byte psw = (byte)(
                        Convert.ToByte(CPU.ZeroFlag) |
                        Convert.ToByte(CPU.SignFlag) << 1 |
                        Convert.ToByte(CPU.ParityFlag) << 2 |
                        Convert.ToByte(CPU.CarryFlag) << 3 |
                        Convert.ToByte(CPU.AuxCarryFlag) << 4);
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerA; // is this the correct order? who knows. Yes it is correct according to PDF specification
                    Memory.RAMMemory[CPU.stackPointer - 2] = psw;
                    instructionText = $"{byte1txt}\t\tPUSH   PSW\t\t; Move flags(0x{psw.ToString("X2")})A(0x{CPU.registerA.ToString("X2")})->stack. SP-2\t" + CPU.CPUStatus();
                    CPU.stackPointer -= 2;
                    stackSize -= 2;
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
                    instructionText = $"******* NOT IMPLEMENTED : {byte1txt}";
                    Debug.WriteLine($"******* NOT IMPLEMENTED : {byte1txt}");
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

                switch (instructionLine.ToString("X4"))
                {
                    case "1A32":
                        comment = "-BlockCopyROM->RAM";
                        break;

                    case "1956":
                        comment = "-DrawStatus";
                        break;

                    case "1A5C":
                        comment = "--ClearScreen";
                        break;

                    case "191A":
                        comment = "--DrawScoreHead";
                        break;

                    case "08F3":
                        comment = "---PrintMessage";
                        break;



                    case "08FF":
                        comment = "----DrawChar";
                        if (instructionText.Contains("A:0x26"))
                        {
                            comment = comment + ("          SPACE");
                        }
                        else if (instructionText.Contains("A:0x12"))
                        {
                            comment = comment + ("           S");
                        }
                        else if (instructionText.Contains("A:0x02"))
                        {
                            comment = comment + ("           C");
                        }
                        else if (instructionText.Contains("A:0x0E"))
                        {
                            comment = comment + ("           O");
                        }
                        else if (instructionText.Contains("A:0x11"))
                        {
                            comment = comment + ("           R");
                        }
                        else if (instructionText.Contains("A:0x04"))
                        {
                            comment = comment + ("           E");
                        }
                        else if (instructionText.Contains("A:0x24"))
                        {
                            comment = comment + ("           <");
                        }
                        else if (instructionText.Contains("A:0x25"))
                        {
                            comment = comment + ("           >");
                        }
                        else if (instructionText.Contains("A:0x1B"))
                        {
                            comment = comment + ("           1");
                        }
                        else if (instructionText.Contains("A:0x1C"))
                        {
                            comment = comment + ("           2");
                        }
                        else if (instructionText.Contains("A:0x28"))
                        {
                            comment = comment + ("           *ASTERISK");
                        }
                        else if (instructionText.Contains("A:0x0B"))
                        {
                            comment = comment + ("           L");
                        }
                        else if (instructionText.Contains("A:0x1D"))
                        {
                            comment = comment + ("           3");
                        }
                        else if (instructionText.Contains("A:0x1A"))
                        {
                            comment = comment + ("           0");
                        }
                        else if (instructionText.Contains("A:0x18"))
                        {
                            comment = comment + ("           Y");
                        }
                        else if (instructionText.Contains("A:0x27"))
                        {
                            comment = comment + ("           =");
                        }
                        else if (instructionText.Contains("A:0x07"))
                        {
                            comment = comment + ("           H");
                        }
                        else if (instructionText.Contains("A:0x08"))
                        {
                            comment = comment + ("           I");
                        }
                        else if (instructionText.Contains("A:0x0F"))
                        {
                            comment = comment + ("           P");
                        }
                        else if (instructionText.Contains("A:0x0E"))
                        {
                            comment = comment + ("           O");
                        }
                        else if (instructionText.Contains("A:0x0D"))
                        {
                            comment = comment + ("           N");
                        }
                        else if (instructionText.Contains("A:0x13"))
                        {
                            comment = comment + ("           T");
                        }
                        else if (instructionText.Contains("A:0x03"))
                        {
                            comment = comment + ("           D");
                        }
                        else if (instructionText.Contains("A:0x00"))
                        {
                            comment = comment + ("           A");
                        }
                        else
                        {
                            comment = comment + ("           **NOTFOUND");
                            int a = 34; //brakpoit dummy.. sigh...
                        }
                        break;

                    case "1439":
                        comment = "-----DrawSimpSprite";
                        break;

                    case "1815":
                        comment = "-----DrawAdvancedTable -SCORE ADVANCE TABLE";
                        break;

                    case "143C":
                        comment = "-----DrawSimpSprite - Next +1";
                        break;


                    case "1925":
                        comment = "--Draw Player 1 Score";
                        break;

                    case "181D":
                        comment = "--Draw SCORE ADVANCE TABLE";
                        break;

                    case "192B":
                        comment = "--Draw Player 2 Score";
                        break;

                    case "1820":
                        comment = "--Draw SCORE ADVANCE TABLE OUT";
                        break;

                    case "1856":
                        comment = "--Read Pri Struct";
                        break;

                    case "0B24":
                        comment = "--Animate";
                        break;
                    case "0a80":
                        comment = "--Animate with ISR";
                        break;
                    case "183D":
                        comment = "-----DrawAdvancedTable -SCORE ADVANCE TABLE midroutine";
                        break;



                    case "1931":
                        comment = "---Draw Score";
                        break;

                    case "09AD":
                        comment = "----Print4Digits";
                        break;

                    case "09B2":
                        comment = "-----DrawHexByte";
                        break;

                    case "1844":
                        comment = "-----Draw 16-bit sprite";
                        break;

                    case "1828":
                        comment = "-----Do All table";
                        break;

                    case "09C5":
                        comment = "-----DrawHexByte - Short jump";
                        break;

                    case "1950":
                        comment = "--PrintHiScore";
                        break;


                    case "193C":
                        comment = "--PrintCreditLabel";
                        break;

                    case "1947":
                        comment = "--DrawNumCredits";
                        break;
                    case "0AEA":
                        comment = " After initialitation --- splash screens";
                        break;
                    case "0B14":
                        comment = " After initialitation --- splash screens midroutine";
                        break;
                    case "0AB1":
                        comment = " -One Second Delay";
                        break;
                    case "0AD7":
                        comment = " -- Wait on delay";
                        break;
                    case "0A93":
                        comment = " - PrintMessageDel";
                        break;
                    case "0ACF":
                        comment = " - Print Center Screen - SPACE INVADERS";
                        break;
                    case "0AAA":
                        comment = " Out from call";
                        break;
                    case "0B17":
                        comment = " Do splash animation";
                        break;
                    case "0B1E":
                        comment = " Animate small alien";
                        break;




                    case "0AA2": // hack to decrease the counter ISRdelay in address: 20C0
                        Memory.RAMMemory[0x20C0] = (byte)(Memory.RAMMemory[0x20C0] - 1);
                        comment = "Hack for wait seconds ISRDelay";
                        break;


                    default:
                        //comment = "";
                        break;
                }
                sb.AppendLine("C: " + CPU.cyclesCounter.ToString("D7") + " $" +
                        instructionLine.ToString("X4") + " " +
                        instructionText + " " + comment);
                linesToAppendCounter++;
            }

            //line counter to file:
            //if (cyclesCounter % 1000 == 0)
            //{
            //    sb.AppendLine(cyclesCounter.ToString() + "****************************************************");
            //    linesToAppendCounter++;
            //}
            int writeEverynLines = 500; //Normal value is 500
            if (linesToAppendCounter > writeEverynLines)
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