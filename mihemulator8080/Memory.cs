using System;
using System.IO;
using System.Text.RegularExpressions;

namespace mihemulator8080
{
    public static class Memory
    {
        static public Byte[] RAMMemory = new byte[65536];

        public static void RAM2File()
        {
            //0000  MOV    D,A
            //15A1
            //AE5F

            string outputPath = @"..\..\..\..\Misc\OutputFiles\RAM.8080asm";
            int memoryAddress = 0;

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (StreamWriter file = new StreamWriter(outputPath))
            {
                foreach (var instruction in CPU.instructionFecther.AssemblyLines)
                {
                    string address = memoryAddress.ToString("X4");
                    memoryAddress += instruction.Item2;
                    file.WriteLine("0x" + address + "\t" + instruction.Item1);
                }
            }
        }

        public static void RAM2FileHTML()
        {
            string outputPath = @"..\..\..\..\Misc\OutputFiles\RAM.html";
            int memoryAddress = 0;

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            using (StreamWriter file = new StreamWriter(outputPath))
            {
                string header = @"
                <head>
                    <style TYPE=""text/css"">
                        * {
                            font-family: consolas;
                            font-size: 97%;
                            }
                            p { margin: 0; }
                    </style>
                </head>
                <p style=""font-size:105%;""><b> Space Invaders. Assembly lines: " +
                CPU.instructionFecther.AssemblyLines.Count + " | Size(Bytes): " +
                CPU.instructionFecther.Bytes.Count + "</b><br/></p>";

                file.WriteLine(header);

                foreach (var instruction in CPU.instructionFecther.AssemblyLines)
                {
                    string address = memoryAddress.ToString("X4");
                    string[] instructionSplit = Regex.Split(instruction.Item1, @"\s+");
                    string action = instructionSplit[0];
                    string argument;
                    if (instructionSplit.Length > 1)
                    {
                        argument = instructionSplit[1];
                    }
                    else argument = "";
                    string indent = "";
                    for (int i = action.Length; i < 7; i++)
                    {
                        indent += "&nbsp;";
                    }

                    string htmlLine = "";
                    htmlLine += @"<p id= """;

                    bool isJumpInstruction = instruction.Item1.Contains("JMP");
                    if (isJumpInstruction)
                    {
                        string targetAddres = instruction.Item1.Split('$')[1];
                        htmlLine += address + @"""><a href=""#" + targetAddres + @""">0x" + address + "&nbsp;" + action + indent + argument + " </a><br/></p>"; //if it is a jump include href
                    }
                    else
                    {
                        htmlLine += address + @""">0x" + address + "&nbsp;" + action + indent + argument + "<br/></p>"; //if it is a jump include href
                    }

                    file.WriteLine(htmlLine);

                    memoryAddress += instruction.Item2;
                }
            }
        }
    }
}