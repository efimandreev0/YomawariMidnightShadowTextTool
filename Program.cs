using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace YomawariText
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args[0].Contains(".txt"))
            {
                Rebuild(args[0], args[1]);
            }
            else
            {
                Extract(args[0]);
            }
        }
        public static void Extract(string dat)
        {
            var reader = new BinaryReader(File.OpenRead(dat));
            int count = reader.ReadInt32();
            string[] strings = new string[count];
            int[] offsets = new int[count];
            int[] length = new int[count];
            int[] ID = new int[count];
            int[] isSystem = new int[count];
            for (int i = 0; i < count; i++)
            {
                ID[i] = reader.ReadInt32();
                length[i] = reader.ReadInt32();
                offsets[i] = reader.ReadInt32() + 4;
                isSystem[i] = reader.ReadInt32();
                if (isSystem[i] != 1)
                {
                    var pos = reader.BaseStream.Position;
                    reader.BaseStream.Position = offsets[i];
                    strings[i] = "SYSTEM|" + Encoding.UTF8.GetString(reader.ReadBytes(length[i])).Replace("\n", "<lf>").Replace("\r", "<br>");
                    reader.BaseStream.Position = pos;
                }
                else
                {
                    var pos = reader.BaseStream.Position;
                    reader.BaseStream.Position = offsets[i];
                    strings[i] = "DIALOGUE|" + Encoding.UTF8.GetString(reader.ReadBytes(length[i])).Replace("\n", "<lf>").Replace("\r", "<br>");
                    reader.BaseStream.Position = pos;
                }
            }
            File.WriteAllLines(Path.GetFileNameWithoutExtension(dat) + ".txt", strings);
        }
        public static void Rebuild(string text, string dat)
        {
            string[] strings = File.ReadAllLines(text);
            int[] offsets = new int[strings.Length];
            int[] length = new int[strings.Length];
            int[] isSystem = new int[strings.Length];
            var writer = new BinaryWriter(File.OpenWrite(dat));
            writer.Write(strings.Length);
            writer.BaseStream.Position = (strings.Length * 16) + 5;
            for (int i = 0; i < strings.Length; i++)
            {
                offsets[i] = (int)writer.BaseStream.Position - 3;
                if (strings[i].Contains("SYSTEM|"))
                {
                    isSystem[i] = 0;
                    strings[i] = strings[i].Substring(7);
                    length[i] = strings[i].Length;
                    writer.Write(Encoding.UTF8.GetBytes(strings[i]));
                    writer.Write(new byte[2]);
                }
                if (strings[i].Contains("DIALOGUE|"))
                {
                    isSystem[i] = 1;
                    strings[i] = strings[i].Substring(9);
                    length[i] = strings[i].Length;
                    writer.Write(Encoding.UTF8.GetBytes(strings[i]));
                    writer.Write(new byte[2]);
                }
            }
            writer.BaseStream.Position = 4;
            for (int i = 0; i < strings.Length; i++)
            {
                writer.BaseStream.Position += 4;
                writer.Write(length[i]);
                writer.Write(offsets[i]);
                writer.Write(isSystem[i]);
            }
        }
    }
}
