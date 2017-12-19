using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public static bool SignFlag, ZeroFlag, AuxCarryFlag, ParityFlag, CarryFlag;

        public static byte programCounter; // (PC) An ancient Instruction Pointer

        public static InstructionFetcher instructionFecther;


        static CPU()
        {
            instructionFecther = new InstructionFetcher();
        }

 

    }
}