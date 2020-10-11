using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Lab3
{

    class Program
    {

        static void Main(string[] args)
        {
            String s = "";

            if (args.Length == 1)
            {
                s = args[0];
            }
            else
            {
                Console.WriteLine("Skriv ETT argument till programmet!");
                Environment.Exit(0);
            }

            FileStream fs;

            try
            {
                fs = new FileStream(@s, FileMode.Open);
            }
            catch(FileNotFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
         
            
            var fileSize = (int)fs.Length;
            var fileData = new byte[fileSize];

            fs.Read(fileData, 0, fileSize);
            fs.Close();

            if (fileData.Length < 8) 
            {
                Console.WriteLine("\nFile too small. The file needs to be at least 8 bytes in size.");
                Environment.Exit(0);
            }

            byte[] first8Bytes = new byte[8];
            for (int i = 0; i < 8; i++)
            {
                first8Bytes[i] = fileData[i];
            }

            if (IsPNG(first8Bytes))
            {
                Console.WriteLine("\nThis is a PNG-file.");
                Console.WriteLine("The image has the pixel dimensions: " + Read4BytesBEndian(fileData, 16) + " x " + Read4BytesBEndian(fileData, 20) + " pixels.\n");

                Console.WriteLine("Chunk List:");

                int offset = 8;
                while (true)
                {
                    Console.WriteLine(ChunkType(fileData, offset + 4) + " is " + (Read4BytesBEndian(fileData, offset) + 12) + " bytes in size.");
                    if (ChunkType(fileData, offset + 4) == "IEND")
                    {
                        break;
                    } 
                    else
                    {
                        offset += Read4BytesBEndian(fileData, offset) + 12;
                    }
                }

            }
            else if (IsBMP(first8Bytes))
            {
                Console.WriteLine("\nThis is a BMP-file.");
                Console.WriteLine("The image has the pixel dimensions: " + BitConverter.ToInt32(fileData, 18) + " x " + BitConverter.ToInt32(fileData, 22) + " pixels.");
            } 
            else
            {
                Console.WriteLine("\nThis is neither a BMP- nor a PNG-file.");
            }

        }

        static bool IsPNG(byte[] first8Bytes)
        {
            byte[] pngPrefix = new byte[8] { 137, 80, 78, 71, 13, 10, 26, 10 };

            for(int i = 0; i < 8; i++)
            {
                if (pngPrefix[i] != first8Bytes[i])
                {
                    return false;
                }
            }
            return true;

        }

        static bool IsBMP(byte[] first2Bytes)
        {
            byte[] bmpPrefix = new byte[2] { 66, 77 };
            if (bmpPrefix[0] == first2Bytes[0] && bmpPrefix[1] == first2Bytes[1])
            {
                return true;
            } 
            else
            {
                return false;
            }

        }

        static int Read4BytesBEndian (byte[] fileData, int offset)
        {
            var temp = new byte[4];
            Array.Copy(fileData, offset, temp, 0, 4);
            Array.Reverse(temp);
            return (int)BitConverter.ToUInt32(temp, 0);

        }

        static string ChunkType (byte[] fileData, int offset)
        {
            ASCIIEncoding s = new ASCIIEncoding();
            return s.GetString(fileData, offset, 4);

        }

    }
}
