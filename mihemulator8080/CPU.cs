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
                string byteOffset1 = BitConverter.ToString(bytesROM.Bytes, position + 1, 1);
                string byteOffset2 = BitConverter.ToString(bytesROM.Bytes, position + 2, 1);


                switch (currentByte)
                {
                    // template: case "C1": assemblyLines.Add(""); break;
                    // Opcode	Instruction	size	flags	function
                    case "00": assemblyLines.Add("NOP"); break; // 0x00	NOP	1
                    case "01": assemblyLines.Add("LXI    B,"); break;
                    case "C3": assemblyLines.Add($"JMP    ${byteOffset2}{byteOffset1}"); break; // 0x00	NOP	1


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