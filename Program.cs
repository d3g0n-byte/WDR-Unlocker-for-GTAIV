// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using Converter.Utils;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip.Compression;
using System;
using static ICSharpCode.SharpZipLib.Zip.ExtendedUnixData;

namespace General
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1) return;
			for (int a = 0; a < args.Length; a++)
			{
				string fileName = args[a];
				using (EndianBinaryReader br = new EndianBinaryReader(File.OpenRead(fileName)))
				{
					uint magic = br.ReadUInt32();
					int version = br.ReadInt32();
					if (version != 110 || magic != 88298322) continue;
					int flags = br.ReadInt32();
					uint CPUSize = ((uint)flags & 0x7FF) << (((flags >> 11) & 0xF) + 8); //расчет размера System Memory Segment
					uint GPUSize = (((uint)flags >> 15) & 0x7FF) << (((flags >> 26) & 0xF) + 8); //расчет размера Graphics Memory Segment

					int dLen = (int)(GPUSize + CPUSize);
					byte[] origBuffer = br.ReadBytes(Convert.ToInt32(br.BaseStream.Length - br.BaseStream.Position));
					byte[] decompBuffer = DecompressDeflate(origBuffer, dLen, noHeader: false);

					WDRUnlocker.WDR.Unlock(ref decompBuffer);

					byte[] array = Compress(decompBuffer, 9, noHeader: false);
					br.Close();
					//File.Delete(fileName);
					BinaryWriter dataOut = new BinaryWriter(new FileStream(fileName, FileMode.Create));
					uint val = 88298322u;
					int resVersion = 110;
					dataOut.Write(val);
					dataOut.Write(resVersion);
					dataOut.Write(flags);
					dataOut.Write(array);
					dataOut.Close();


				}

			}
		}

		public static byte[] DecompressDeflate(byte[] data, int decompSize, bool noHeader = true)
		{
			byte[] array = new byte[decompSize];
			Inflater inflater = new Inflater(noHeader);
			inflater.SetInput(data);
			inflater.Inflate(array);
			return array;
		}
		public static byte[] Compress(byte[] input, int level, bool noHeader = true)
		{
			byte[] array = new byte[input.Length];
			Deflater deflater = new Deflater(level, noHeader);
			byte[] array2;
			try
			{
				deflater.SetInput(input, 0, input.Length);
				deflater.Finish();
				array2 = new byte[deflater.Deflate(array)];
			}
			catch (Exception ex)
			{
				throw ex;
			}
			Array.Copy(array, 0, array2, 0, array2.Length);
			return array2;
		}

	}
}