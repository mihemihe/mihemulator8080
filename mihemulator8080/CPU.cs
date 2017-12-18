using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace mihemulator8080
{
    public static class CPU
    {
        public static byte registerA, registerB, registerC, registerD, registreE;

        public static byte programCounter;

        public static String Assemble(string asmCode)
        {
            return "";
        }

        public static String Disassemble(string pathByteCode)
        {
            BytesROM bytesROM;
            string assemblyCode;
            List<string> assemblyLines = new List<string>();
            List<string> noOPCode = new List<string>();

            using (StreamReader file = File.OpenText(pathByteCode))
            {
                JsonSerializer serializer = new JsonSerializer();
                bytesROM = (BytesROM)serializer.Deserialize(file, typeof(BytesROM));
            }

            int codeLengthInBytes = bytesROM.Bytes.Length;
            for (int position = 0; position < codeLengthInBytes; position++)
            {
                string byte2 = "";
                string byte3 = "";
                
                string currentByte = BitConverter.ToString(bytesROM.Bytes, position, 1);

                if (position < codeLengthInBytes - 1)
                {
                     byte2 = BitConverter.ToString(bytesROM.Bytes, position + 1, 1); //out of bound at the endfix

                    if (position < codeLengthInBytes - 2)
                    { 
                         byte3 = BitConverter.ToString(bytesROM.Bytes, position + 2, 1);
                    }
                }


                switch (currentByte)
                {
                    // case "01": assemblyLines.Add($""); break;
                    // Opcode	Instruction	size	flags	function
                    case "00": assemblyLines.Add($"NOP"); break;
                    case "01": assemblyLines.Add($"LXI    B,#${byte3}{byte2}"); position++; position++; break;
                    case "02": assemblyLines.Add($"STAX   B"); break;
                    case "03": assemblyLines.Add($"INX    B"); break;
                    case "04": assemblyLines.Add($"INR    B"); break;
                    case "05": assemblyLines.Add($"DCR    B"); break;
                    case "06": assemblyLines.Add($"MVI    B,#${byte2}"); position++; break;
                    case "07": assemblyLines.Add($"RLC"); break;
                    case "0D": assemblyLines.Add($"DCR    C"); break;
                    case "0E": assemblyLines.Add($"MVI    C,#${byte2}"); position++; break;
                    case "0F": assemblyLines.Add($"RRC"); break;

                    case "11": assemblyLines.Add($"LXI    D,#${byte3}{byte2}"); position++; position++; break;
                    case "13": assemblyLines.Add($"INX    D"); break;
                    case "14": assemblyLines.Add($"INR    D"); break;
                    case "15": assemblyLines.Add($"DCR    D"); break;
                    case "16": assemblyLines.Add($"MIV    D,#${byte2}"); position++; break;
                    case "19": assemblyLines.Add($"DAD    D"); break;
                    case "1A": assemblyLines.Add($"LDAX   #${byte2}"); position++; break;

                    case "20": assemblyLines.Add($"RIM"); break;
                    case "21": assemblyLines.Add($"LXI    H,#${byte3}{byte2}"); position++; position++; break;
                    case "22": assemblyLines.Add($"SHLD   ${byte3}{byte2}"); position++; position++; break;
                    case "23": assemblyLines.Add($"INX    H"); break;
                    case "26": assemblyLines.Add($"MVI    H,#${byte2}"); position++; break;
                    case "27": assemblyLines.Add($"DAA"); break;
                    case "29": assemblyLines.Add($"DAD    H"); break;
                    case "2A": assemblyLines.Add($"LHLD   ${byte3}{byte2}"); position++; position++; break;
                    case "2B": assemblyLines.Add($"DCX    H"); break;
                    case "2C": assemblyLines.Add($"INR    L"); break;
                    case "2E": assemblyLines.Add($"MVI    L,#${byte2}"); position++; break;

                    case "31": assemblyLines.Add($"LXI    SP,#${byte3}{byte2}"); position++; position++; break;
                    case "32": assemblyLines.Add($"STA    ${byte3}{byte2}"); position++; position++; break;
                    case "34": assemblyLines.Add($"INR    M"); break;
                    case "35": assemblyLines.Add($"DCR    M"); break;
                    case "36": assemblyLines.Add($"MVI    M,#${byte2}"); position++; break;
                    case "37": assemblyLines.Add($"STC"); break;
                    case "3A": assemblyLines.Add($"LDA    ${byte3}{byte2}"); position++; position++; break;
                    case "3C": assemblyLines.Add($"INR    A"); break;
                    case "3D": assemblyLines.Add($"DCR    A"); break;
                    case "3E": assemblyLines.Add($"MVI    A,#${byte2}"); position++; break; // is byte2 or byte3?


                    case "46": assemblyLines.Add($"MOV    B,M"); break;
                    case "47": assemblyLines.Add($"MOV    B,A"); break;
                    case "4E": assemblyLines.Add($"MOV    C,M"); break;
                    case "4F": assemblyLines.Add($"MOV    C,A"); break;

                    case "50": assemblyLines.Add($"MOV    D,B"); break;
                    case "51": assemblyLines.Add($"MOV    D,C"); break;
                    case "52": assemblyLines.Add($"MOV    D,D"); break;
                    case "53": assemblyLines.Add($"MOV    D,E"); break;
                    case "54": assemblyLines.Add($"MOV    D,H"); break;
                    case "55": assemblyLines.Add($"MOV    D,L"); break;
                    case "56": assemblyLines.Add($"MOV    D,M"); break;


                    case "5D": assemblyLines.Add($"MOV    E,L"); break;
                    case "5E": assemblyLines.Add($"MOV    E,M"); break;
                    case "5F": assemblyLines.Add($"MOV    E,A"); break;

                    case "61": assemblyLines.Add($"MOV    H,C"); break;
                    case "62": assemblyLines.Add($"MOV    H,D"); break;
                    case "63": assemblyLines.Add($"MOV    H,E"); break;
                    case "64": assemblyLines.Add($"MOV    H,H"); break;
                    case "65": assemblyLines.Add($"MOV    H,L"); break;
                    case "66": assemblyLines.Add($"MOV    H,M"); break;
                    case "67": assemblyLines.Add($"MOV    H,A"); break;
                    case "68": assemblyLines.Add($"MOV    L,B"); break;
                    case "69": assemblyLines.Add($"MOV    L,C"); break;
                    case "6A": assemblyLines.Add($"MOV    L,D"); break;
                    case "6B": assemblyLines.Add($"MOV    L,E"); break;
                    case "6C": assemblyLines.Add($"MOV    L,H"); break;
                    case "6D": assemblyLines.Add($"MOV    L,L"); break;
                    case "6E": assemblyLines.Add($"MOV    L,M"); break;
                    case "6F": assemblyLines.Add($"MOV    L,A"); break;

                    case "70": assemblyLines.Add($"MOV    M,B"); break;
                    case "71": assemblyLines.Add($"MOV    M,C"); break;
                    case "72": assemblyLines.Add($"MOV    M,D"); break;
                    case "73": assemblyLines.Add($"MOV    M,E"); break;
                    case "74": assemblyLines.Add($"MOV    M,H"); break;
                    case "75": assemblyLines.Add($"MOV    M,L"); break;
                    case "76": assemblyLines.Add($"HLT"); break;
                    case "77": assemblyLines.Add($"MOV    M,A"); break;
                    case "78": assemblyLines.Add($"MOV    A,B"); break;
                    case "79": assemblyLines.Add($"MOV    A,C"); break;
                    case "7A": assemblyLines.Add($"MOV    A,D"); break;
                    case "7B": assemblyLines.Add($"MOV    A,E"); break;
                    case "7C": assemblyLines.Add($"MOV    A,H"); break;
                    case "7D": assemblyLines.Add($"MOV    A,L"); break;
                    case "7E": assemblyLines.Add($"MOV    A,M"); break;
                    case "7F": assemblyLines.Add($"MOV    A,A"); break;

                    case "80": assemblyLines.Add($"ADD    B"); break;
                    case "82": assemblyLines.Add($"ADD    D"); break;
                    case "85": assemblyLines.Add($"ADD    L"); break;
                    case "86": assemblyLines.Add($"ADD    M"); break;
                    case "87": assemblyLines.Add($"ADD    A"); break;
                    case "88": assemblyLines.Add($"ADC    B"); break;
                    case "89": assemblyLines.Add($"ADC    C"); break;
                    case "8A": assemblyLines.Add($"ADC    D"); break;
                    case "8B": assemblyLines.Add($"ADC    E"); break;
                    case "8C": assemblyLines.Add($"ADC    H"); break;
                    case "8D": assemblyLines.Add($"ADC    L"); break;
                    case "8E": assemblyLines.Add($"ADC    M"); break;
                    case "8F": assemblyLines.Add($"ADC    A"); break;

                    case "97": assemblyLines.Add($"DUB    A"); break;

                    case "B0": assemblyLines.Add($"ORA    B"); break;
                    case "B4": assemblyLines.Add($"ORA    H"); break;
                    case "B8": assemblyLines.Add($"CMP    B"); break;
                    case "BE": assemblyLines.Add($"CMP    M"); break;


                    case "A0": assemblyLines.Add($"ANA    B"); break;
                    case "A7": assemblyLines.Add($"ANA    A"); break;
                    case "AF": assemblyLines.Add($"XRA    A"); break;

                    case "C0": assemblyLines.Add($"RNZ"); break;
                    case "C1": assemblyLines.Add($"POP    B"); break;
                    case "C2": assemblyLines.Add($"JNZ    ${byte3}{byte2}"); position++; position++; break;
                    case "C3": assemblyLines.Add($"JMP    ${byte3}{byte2}"); position++; position++; break;
                    case "C4": assemblyLines.Add($"CNZ    ${byte3}{byte2}"); position++; position++; break;
                    case "C5": assemblyLines.Add($"PUSH   B"); break;
                    case "C6": assemblyLines.Add($"ADI    #${byte2}"); position++; break;
                    case "C8": assemblyLines.Add($"RZ"); break;
                    case "C9": assemblyLines.Add($"RET");  break;
                    case "CA": assemblyLines.Add($"JZ     ${byte3}{byte2}"); position++; position++; break;
                    case "CC": assemblyLines.Add($"CZ     ${byte3}{byte2}"); position++; position++; break;
                    case "CD": assemblyLines.Add($"CALL   ${byte3}{byte2}"); position++; position++; break;

                    case "D0": assemblyLines.Add($"RNC"); break;
                    case "D1": assemblyLines.Add($"POP    D"); break;
                    case "D3": assemblyLines.Add($"OUT    #${byte2}"); position++; break;
                    case "D2": assemblyLines.Add($"JNZ    ${byte3}{byte2}"); position++; position++; break;
                    case "D4": assemblyLines.Add($"CNC    ${byte3}{byte2}"); position++; position++; break;
                    case "D5": assemblyLines.Add($"PUSH   D"); break;
                    case "D6": assemblyLines.Add($"SUI    #${byte2}"); position++; break;
                    case "DA": assemblyLines.Add($"JC     ${byte3}{byte2}"); position++; position++; break;
                    case "DB": assemblyLines.Add($"IN     #${byte2}"); position++; break;
                    case "DE": assemblyLines.Add($"SBI    #${byte2}"); position++; break;


                    case "E1": assemblyLines.Add($"POP    H"); break;
                    case "E3": assemblyLines.Add($"XTHL"); break;
                    case "E5": assemblyLines.Add($"PUSH   H"); break;
                    case "E6": assemblyLines.Add($"ANI    #${byte2}"); position++; break;
                    case "E9": assemblyLines.Add($"PCHL"); break;
                    case "EB": assemblyLines.Add($"XCHG"); break;

                    case "F1": assemblyLines.Add($"POP    PSW"); break;
                    case "F5": assemblyLines.Add($"PUSH   PSW"); break;
                    case "F6": assemblyLines.Add($"ORI    #${byte2}"); position++; break;
                    case "FA": assemblyLines.Add($"JM     ${byte3}{byte2}"); position++; position++; break;
                    case "FB": assemblyLines.Add($"EI"); break;
                    case "FE": assemblyLines.Add($"CPI    #${byte2}"); position++; break;



                    default: noOPCode.Add($"{currentByte}******OPTCODE NOT FOUND *******");
                        break;
                }
                
            }

            foreach (string line in assemblyLines)
            {
                Debug.WriteLine(line);
            }


            return "";
        }
    }
}