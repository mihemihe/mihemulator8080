using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace mihemulator8080
{
    public enum SourceFileFormat
    {
        BINARY,
        JSON_HEX,
        JSON_SIMPLE,
        JSON_DECIMAL,
        TEXT_HEX,
        TEXT_SIMPLE,
        TEXT_DECIMAL
    }

    public class InstructionFetcher
    {
        public List<string> AssemblyLines { get; set; }
        private string SourceCode { get; set; }
        private SourceFileFormat SourceCodeFormat { get; set; }
        private byte[] Bytes { get; set; }
        private int iterator { get; set; }

        public InstructionFetcher()
        {
            iterator = -1;
        }

        public string FetchNext()
        {
            if (iterator >= AssemblyLines.Count)
            {
                return "EOF";
            }
            else
            {
                iterator++;
                return AssemblyLines[iterator];
            }
        }

        public int LoadSourceFile(string pathByteCode, SourceFileFormat format)
        {
            SourceCodeFormat = format;
            if (format == SourceFileFormat.JSON_HEX)
            {
                using (StreamReader file = File.OpenText(pathByteCode))
                {
                    
                    JsonSerializer serializer = new JsonSerializer();
                    Bytes = (byte[])serializer.Deserialize(file, typeof(byte[]));
                }
            }
            //TODO: add other formats from the enum

            return 0;
        }

        private void ParseFile()
        {
            int size = 0;
            int codeLengthInBytes = Bytes.Length;

            for (int position = 0; position < codeLengthInBytes;)
            {
                byte currentByte = Bytes[position];
                byte byte2 = 0x00;
                byte byte3 = 0x00;

                if (position < codeLengthInBytes - 1)
                {
                    byte2 = Bytes[position + 1]; //out of bound at the endfix

                    if (position < codeLengthInBytes - 2)
                    {
                        byte3 = Bytes[position + 2];
                    }
                }

                AssemblyLines.Add(DisassembleInstruction(new InstructionOpcodes(currentByte, byte2, byte3), out size));

                position = position + size;
            }

            //        List<string> noOPCode = new List<string>();
        }

        private readonly struct InstructionOpcodes
        {
            public byte Byte1 { get; }
            public byte Byte2 { get; }
            public byte Byte3 { get; }

            public InstructionOpcodes(byte opcode1, byte opcode2, byte opcode3)
            {
                Byte1 = opcode1;
                Byte2 = opcode2;
                Byte3 = opcode3;
            }
        }

        private static string DisassembleInstruction(in InstructionOpcodes instruction, out int size)
        {
            size = 1;
            string byte3 = BitConverter.ToString(new byte[] { instruction.Byte3 });
            string byte2 = BitConverter.ToString(new byte[] { instruction.Byte2 });

            switch (instruction.Byte1)
            {
                // case "01": assemblyLines.Add($""); break;
                // Opcode	Instruction	size	flags	function
                case 0x00: return $"NOP";
                case 0x01: size = 3; return $"LXI    B,#${byte3}{byte2}";
                case 0x02: return $"STAX   B";
                case 0x03: return $"INX    B";
                case 0x04: return $"INR    B";
                case 0x05: return $"DCR    B";
                case 0x06: size = 2; return $"MVI    B,#${byte2}";
                case 0x07: return $"RLC";
                case 0x0D: return $"DCR    C";
                case 0x0E: size = 2; return $"MVI    C,#${byte2}";
                case 0x0F: return $"RRC";

                case 0x11: size = 3; return $"LXI    D,#${byte3}{byte2}";
                case 0x13: return $"INX    D";
                case 0x14: return $"INR    D";
                case 0x15: return $"DCR    D";
                case 0x16: size = 2; return $"MIV    D,#${byte2}";
                case 0x19: return $"DAD    D";
                case 0x1A: size = 2; return $"LDAX   #${byte2}";

                case 0x20: return $"RIM";
                case 0x21: size = 3; return $"LXI    H,#${byte3}{byte2}";
                case 0x22: size = 3; return $"SHLD   ${byte3}{byte2}";
                case 0x23: return $"INX    H";
                case 0x26: size = 2; return $"MVI    H,#${byte2}";
                case 0x27: return $"DAA";
                case 0x29: return $"DAD    H";
                case 0x2A: size = 3; return $"LHLD   ${byte3}{byte2}";
                case 0x2B: return $"DCX    H";
                case 0x2C: return $"INR    L";
                case 0x2E: size = 2; return $"MVI    L,#${byte2}";

                case 0x31: size = 3; return $"LXI    SP,#${byte3}{byte2}";
                case 0x32: size = 3; return $"STA    ${byte3}{byte2}";
                case 0x34: return $"INR    M";
                case 0x35: return $"DCR    M";
                case 0x36: size = 2; return $"MVI    M,#${byte2}";
                case 0x37: return $"STC";
                case 0x3A: size = 3; return $"LDA    ${byte3}{byte2}";
                case 0x3C: return $"INR    A";
                case 0x3D: return $"DCR    A";
                case 0x3E: size = 2; return $"MVI    A,#${byte2}"; // is byte2 or byte3?

                case 0x46: return $"MOV    B,M";
                case 0x47: return $"MOV    B,A";
                case 0x4E: return $"MOV    C,M";
                case 0x4F: return $"MOV    C,A";

                case 0x50: return $"MOV    D,B";
                case 0x51: return $"MOV    D,C";
                case 0x52: return $"MOV    D,D";
                case 0x53: return $"MOV    D,E";
                case 0x54: return $"MOV    D,H";
                case 0x55: return $"MOV    D,L";
                case 0x56: return $"MOV    D,M";

                case 0x5D: return $"MOV    E,L";
                case 0x5E: return $"MOV    E,M";
                case 0x5F: return $"MOV    E,A";

                case 0x61: return $"MOV    H,C";
                case 0x62: return $"MOV    H,D";
                case 0x63: return $"MOV    H,E";
                case 0x64: return $"MOV    H,H";
                case 0x65: return $"MOV    H,L";
                case 0x66: return $"MOV    H,M";
                case 0x67: return $"MOV    H,A";
                case 0x68: return $"MOV    L,B";
                case 0x69: return $"MOV    L,C";
                case 0x6A: return $"MOV    L,D";
                case 0x6B: return $"MOV    L,E";
                case 0x6C: return $"MOV    L,H";
                case 0x6D: return $"MOV    L,L";
                case 0x6E: return $"MOV    L,M";
                case 0x6F: return $"MOV    L,A";

                case 0x70: return $"MOV    M,B";
                case 0x71: return $"MOV    M,C";
                case 0x72: return $"MOV    M,D";
                case 0x73: return $"MOV    M,E";
                case 0x74: return $"MOV    M,H";
                case 0x75: return $"MOV    M,L";
                case 0x76: return $"HLT";
                case 0x77: return $"MOV    M,A";
                case 0x78: return $"MOV    A,B";
                case 0x79: return $"MOV    A,C";
                case 0x7A: return $"MOV    A,D";
                case 0x7B: return $"MOV    A,E";
                case 0x7C: return $"MOV    A,H";
                case 0x7D: return $"MOV    A,L";
                case 0x7E: return $"MOV    A,M";
                case 0x7F: return $"MOV    A,A";

                case 0x80: return $"ADD    B";
                case 0x82: return $"ADD    D";
                case 0x85: return $"ADD    L";
                case 0x86: return $"ADD    M";
                case 0x87: return $"ADD    A";
                case 0x88: return $"ADC    B";
                case 0x89: return $"ADC    C";
                case 0x8A: return $"ADC    D";
                case 0x8B: return $"ADC    E";
                case 0x8C: return $"ADC    H";
                case 0x8D: return $"ADC    L";
                case 0x8E: return $"ADC    M";
                case 0x8F: return $"ADC    A";

                case 0x97: return $"DUB    A";

                case 0xB0: return $"ORA    B";
                case 0xB4: return $"ORA    H";
                case 0xB8: return $"CMP    B";
                case 0xBE: return $"CMP    M";

                case 0xA0: return $"ANA    B";
                case 0xA7: return $"ANA    A";
                case 0xAF: return $"XRA    A";

                case 0xC0: return $"RNZ";
                case 0xC1: return $"POP    B";
                case 0xC2: size = 3; return $"JNZ    ${byte3}{byte2}";
                case 0xC3: size = 3; return $"JMP    ${byte3}{byte2}";
                case 0xC4: size = 3; return $"CNZ    ${byte3}{byte2}";
                case 0xC5: return $"PUSH   B";
                case 0xC6: size = 2; return $"ADI    #${byte2}";
                case 0xC8: return $"RZ";
                case 0xC9: return $"RET";
                case 0xCA: size = 3; return $"JZ     ${byte3}{byte2}";
                case 0xCC: size = 3; return $"CZ     ${byte3}{byte2}";
                case 0xCD: size = 3; return $"CALL   ${byte3}{byte2}";

                case 0xD0: return $"RNC";
                case 0xD1: return $"POP    D";
                case 0xD3: size = 2; return $"OUT    #${byte2}";
                case 0xD2: size = 3; return $"JNZ    ${byte3}{byte2}";
                case 0xD4: size = 3; return $"CNC    ${byte3}{byte2}";
                case 0xD5: return $"PUSH   D";
                case 0xD6: size = 2; return $"SUI    #${byte2}";
                case 0xDA: size = 3; return $"JC     ${byte3}{byte2}";
                case 0xDB: size = 2; return $"IN     #${byte2}";
                case 0xDE: size = 2; return $"SBI    #${byte2}";

                case 0xE1: return $"POP    H";
                case 0xE3: return $"XTHL";
                case 0xE5: return $"PUSH   H";
                case 0xE6: size = 2; return $"ANI    #${byte2}";
                case 0xE9: return $"PCHL";
                case 0xEB: return $"XCHG";

                case 0xF1: return $"POP    PSW";
                case 0xF5: return $"PUSH   PSW";
                case 0xF6: size = 2; return $"ORI    #${byte2}";
                case 0xFA: size = 3; return $"JM     ${byte3}{byte2}";
                case 0xFB: return $"EI";
                case 0xFE: size = 2; return $"CPI    #${byte2}";

                default:
                    return BitConverter.ToString(new byte[] { instruction.Byte1 }) + " not found ************************";
            }
        }
    }
}