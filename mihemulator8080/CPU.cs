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
                   $"SP:{CPU.stackPointer.ToString("X4")}";
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
                    CPU.registerC = opCodes.Byte2;
                    CPU.registerB = opCodes.Byte3;
                    instructionText = $"LXI    B,#${(byte3txt)}{byte2txt}\t; {(byte3txt)}{byte2txt} to BC" + "\t\t" + CPU.CPUStatus(); 
                    break;

                case 0x05: //DCR B "Z, S, P, AC flags affected"
                    instructionText = $"{byte1txt}\t\tDCR    B\t\t; Decrement B({CPU.registerB.ToString("X2")}) and update ZSPAC" + "\t\t" +CPU.CPUStatus();
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
                    instructionText = $"DAD    B";
                    HL = 0;
                    HL = (uint)((CPU.registerH << 8) | CPU.registerL);
                    uint BC = (uint)((CPU.registerB << 8) | CPU.registerC);
                    uint DADBResult = HL + BC;
                    CPU.registerH = (byte)((DADBResult & 0xFF00) >> 8);
                    CPU.registerL = (byte)(DADBResult & 0xFF);
                    CPU.CarryFlag = ((DADBResult & 0xFFFF0000) != 0);
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
                    instructionText = $"MVI    C,#${byte2txt}";
                    CPU.registerC = opCodes.Byte2;
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

                case 0x19: //DAD    D //double add, sums HL + DE, in their byte positions and compare. CY flag
                    instructionText = $"DAD    D";
                    HL = 0;
                    HL = (uint)((CPU.registerH << 8) | CPU.registerL);
                    uint DE = (uint)((CPU.registerD << 8) | CPU.registerE);
                    uint DADDResult = HL + DE;
                    CPU.registerH = (byte)((DADDResult & 0xFF00) >> 8);
                    CPU.registerL = (byte)(DADDResult & 0xFF);
                    CPU.CarryFlag = ((DADDResult & 0xFFFF0000) != 0);
                    break;

                case 0x1A: //LDAX   D - "A <- (DE)"
                    instructionText = "LDAX   D";
                    memoryAddressDE = 0;
                    memoryAddressDE = CPU.registerD << 8;
                    memoryAddressDE = memoryAddressDE | CPU.registerE;
                    CPU.registerA = Memory.RAMMemory[memoryAddressDE];

                    break;

                case 0x21: //LXI    H,#${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tLXI    H,#${byte3txt}{byte2txt}\t\t; Load 0x{byte3txt}{byte2txt} on HL" + "\t\t\t" + CPU.CPUStatus();
                    CPU.registerH = opCodes.Byte3;
                    CPU.registerL = opCodes.Byte2;
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

                case 0x26: //MVI    H,#${byte2}
                    instructionText = $"MVI    H,#${byte2txt}";
                    CPU.registerH = opCodes.Byte2;
                    break;

                case 0x29: //DAD    H
                    instructionText = $"DAD    H"; //double add, doubles L doubles H, sums them and compare. CY flag
                    HL = 0;
                    HL = (uint)(CPU.registerH << 8 | CPU.registerL);
                    DADHResult = HL + HL;
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
                    instructionText = $"MVI    M,#${byte2txt}";
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    Memory.RAMMemory[memoryAddressHL] = opCodes.Byte2;
                    break;

                case 0x6f: //MOV    L,A
                    instructionText = $"MOV    L,A";
                    CPU.registerL = CPU.registerA;
                    break;

                //case 0x6f: //
                //    instructionText = $"";
                //    break;

                case 0x77: //MOV    M,A
                    instructionText = $"MOV    M,A";
                    memoryAddressHL = 0;
                    memoryAddressHL = CPU.registerH << 8;
                    memoryAddressHL = memoryAddressHL | CPU.registerL;
                    Memory.RAMMemory[memoryAddressHL] = CPU.registerA;
                    break;

                case 0x7c: //MOV    A,H
                    instructionText = $"MOV    A,H --- HL: ${CPU.registerH.ToString("X2")}{CPU.registerL.ToString("X2")}";
                    CPU.registerA = CPU.registerH;
                    break;

                case 0xC1: //POP    B
                    instructionText = $"POP    B";
                    CPU.registerB = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerC = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;

                    break;

                case 0xC2: //JNZ    ${byte3}{byte2}
                    instructionText = $"JNZ    ${byte3txt}{byte2txt}";
                    // if z=false then jump to address
                    if (CPU.ZeroFlag == false)
                    {
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
                    instructionText = $"RET";
                    CPU.programCounter = 0;
                    programCounter = Memory.RAMMemory[CPU.stackPointer + 1] << 8; //why +1 and not -1? explained next commentary
                    programCounter = programCounter | Memory.RAMMemory[CPU.stackPointer]; //+1 to go up in the stack (grows downwards)
                    CPU.stackPointer += 2; //return the stack pointer back to original position
                    break;

                case 0xC5: //PUSH   B - BC move to the stack
                    instructionText = $"PUSH   B";
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerB; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerC;
                    CPU.stackPointer += 2;
                    break;

                case 0xCD: //CALL   ${byte3}{byte2}
                    instructionText = $"{byte1txt} {byte2txt} {byte3txt}\tCALL   ${byte3txt}{byte2txt}\t\t; Jump->${byte3txt}{byte2txt}, ret ${CPU.programCounter.ToString("X2")}->stack, SP -2"
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

                case 0xD5: //PUSH   D - DE move to the stack
                    instructionText = $"PUSH   D";
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerD; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerE;
                    CPU.stackPointer += 2;
                    break;

                case 0xE1: //POP    H -- POP stack to HL
                    instructionText = $"POP    H";
                    CPU.registerH = Memory.RAMMemory[CPU.stackPointer + 1];
                    CPU.registerL = Memory.RAMMemory[CPU.stackPointer];
                    CPU.stackPointer += 2;
                    break;

                case 0xEB: //XCHG - Swaps HL by DE, in this particular order
                    instructionText = $"XCHG";
                    tempHL = new byte[] { CPU.registerH, CPU.registerL };
                    CPU.registerH = CPU.registerD;
                    CPU.registerL = CPU.registerE;
                    CPU.registerD = tempHL[0];
                    CPU.registerE = tempHL[1];
                    break;

                case 0xE5: //PUSH   H - HL move to the stack
                    instructionText = $"PUSH   H";
                    Memory.RAMMemory[CPU.stackPointer - 1] = CPU.registerH; // is this the correct order? who knows
                    Memory.RAMMemory[CPU.stackPointer - 2] = CPU.registerL;
                    CPU.stackPointer += 2;
                    break;

                case 0xFE: //CPI    #${byte2} //compare immediate with accumulator A, substracting
                    //	Z, S, P, CY, AC
                    instructionText = $"CPI    #${byte2txt}"; //comments in 0x05 of this operations
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