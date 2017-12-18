using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            using (StreamReader file = File.OpenText(pathByteCode))
            {
                JsonSerializer serializer = new JsonSerializer();
                bytesROM = (BytesROM)serializer.Deserialize(file, typeof(BytesROM));
            }
            for (byte position = 0; position < bytesROM.Bytes.Length; position++)
            {
                
                string currentByte = BitConverter.ToString(bytesROM.Bytes, position, 1);
                string byteOffset1 = BitConverter.ToString(bytesROM.Bytes, position + 1, 1); //out of bound at the endfix
                string byteOffset2 = BitConverter.ToString(bytesROM.Bytes, position + 2, 1);


                switch (currentByte)
                {
                    // case "01": assemblyLines.Add($""); break;
                    // Opcode	Instruction	size	flags	function
                    case "00": assemblyLines.Add($"NOP"); break; 
                    case "01": assemblyLines.Add($"LXI    B,#${byteOffset2}{byteOffset1}"); break;
                    case "02": assemblyLines.Add($"STAX    B"); break;
                    case "03": assemblyLines.Add($"INX    B"); break;
                    case "0F": assemblyLines.Add($"RRC"); break;

                    case "21": assemblyLines.Add($"LXI    H,#${byteOffset2}{byteOffset1}"); break;

                    case "32": assemblyLines.Add($"STA    ${byteOffset2}{byteOffset1}"); break;
                    case "3E": assemblyLines.Add($"MVI    A,#${byteOffset1}"); break; // is byte2 or byte3?
                    case "35": assemblyLines.Add($"DCR    M"); break;






                    case "C3": assemblyLines.Add($"JMP    ${byteOffset2}{byteOffset1}"); break;
                    case "C5": assemblyLines.Add($"PUSH    B"); break;
                    case "CD": assemblyLines.Add($"CALL    ${byteOffset2}{byteOffset1}"); break;

                    case "D4": assemblyLines.Add($"CNC    ${byteOffset2}{byteOffset1}"); break;
                    case "D5": assemblyLines.Add($"PUSH    D"); break;
                    case "DA": assemblyLines.Add($"IN    #${byteOffset1}"); break;
                    case "DB": assemblyLines.Add($"IN    #${byteOffset1}"); break;

                    case "E5": assemblyLines.Add($"PUSH    H"); break;

                    case "F5": assemblyLines.Add($"PUSH    PSW"); break;

                    case "tuyj":
                        Console.WriteLine(5);
                        break;

                    default:
                        break;
                }
            }
            return "";
        }
    }
}