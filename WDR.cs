using Converter.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace WDRUnlocker
{
	internal class WDR
	{
		public static byte[] WriteToByteArray(ref byte[] array, uint pos, uint value)
		{
			byte[] array2 = BitConverter.GetBytes(value);
			for (int a = 0; a < array2.Length; a++)
			{
				array[pos + a] = array2[a];
			}
			//pos += (uint)array2.Length;
			return array;
		}
		public struct Collection
		{
			public uint m_pList;
			public ushort m_wCount;
			public ushort m_wSize;
		}

		public static void Unlock(ref byte[] wdr)
		{
			byte[] newWDR = new byte[wdr.Length];
			Buffer.BlockCopy(wdr, 0, newWDR, 0, wdr.Length);
			MemoryStream ms = new MemoryStream(wdr);
			EndianBinaryReader br = new EndianBinaryReader(ms);
			//if (endian) br.Endianness = Endian.BigEndian;// 0 - lit, 1 - big
			br.Position = 0x40;
			uint[] pModelCollection = new uint[4];
			for (int a = 0; a < 4; a++) pModelCollection[a] = br.ReadOffset();
			br.Position = 0x60;
			int[] count = new int[4];
			for (int a = 0; a < 4; a++) count[a] = br.ReadInt32();
			for (int a = 0; a < 4; a++)
			{
				if (count[a] < 1) continue;
				br.Position = pModelCollection[a];
				Collection cModelCollection = br.ReadCollections();
				br.Position = cModelCollection.m_pList;
				uint[] pModel = new uint[cModelCollection.m_wCount];
				for (int b = 0; b < cModelCollection.m_wCount; b++)pModel[b] = br.ReadOffset();
				for (int b = 0; b < cModelCollection.m_wCount; b++)
				{
					br.Position = pModel[b];
					br.Position +=4;
					Collection cGeometry = br.ReadCollections();
					br.Position = cGeometry.m_pList;
					uint[] pGeometry = new uint[cGeometry.m_wCount];
					for (int c = 0; c < cGeometry.m_wCount; c++) pGeometry[c] = br.ReadOffset();
					for (int c = 0; c < cGeometry.m_wCount; c++)
					{
						br.Position = pGeometry[c];
						br.Position += 0x2c;
						uint tmp1 = br.ReadUInt32();
						WriteToByteArray(ref newWDR, (uint)br.Position, tmp1 / 3);
					}
				}
			}
			ms.Close();
			br.Close();
			Buffer.BlockCopy(newWDR, 0, wdr, 0, wdr.Length);

		}
	}
}
