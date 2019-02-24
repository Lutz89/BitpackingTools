﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using emotitron.Compression;

public class BenchmarkTests : MonoBehaviour
{
	public const int BYTE_CNT = 40;
	public const int LOOP = 10000;
	public static byte[] buffer = new byte[BYTE_CNT];

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Test()
	{
		Debug.Log("Testing <b>" + BYTE_CNT * LOOP + "</b> Byte Read/Writes");

		ByteForByteWrite();
		BitpackBytesEven();
		BitpackBytesUnEven();
		ByteForByteWrite();
		BitpackBytesEven();
		BitpackBytesUnEven();

		BitstreamTest();
		BitstreamIndirectTest();

		Debug.Log("--------");
		//ResetBitstreamTest();
		//ResetBitstreamTest();

	}

	private static Bitstream bs = new Bitstream((ulong)222, (ulong)222, (ulong)222, (ulong)222, (uint)222);

	//public static void ResetBitstreamTest()
	//{
	//	Debug.Log("bs: " + bs[0] + " " + bs[1] + bs[2] + " " + bs[3] + " " + bs[4]);

	//	int count = 100000;
	//	var watch = System.Diagnostics.Stopwatch.StartNew();

	//	Debug.Log("bs len: " + bs.WritePtr);


	//	for (int i = 0; i < count; ++i)
	//	{
	//		int pos = 0;
	//		bs.ReadOut(buffer, ref pos);
	//	}

	//	watch.Stop();
	//	Debug.Log("Reset <b>old</b> =" + watch.ElapsedMilliseconds + " ms");
	//	Debug.Log(buffer[0] + " " + buffer[1] + buffer[2] + " " + buffer[3] + " " + buffer[4] + " " + buffer[5] + " " + buffer[6] + " " + buffer[7] + " " + buffer[8] + " " + buffer[9]
	//		+ " " + buffer[10] + " " + buffer[11] + " " + buffer[12] + " " + buffer[13]);

	//	var watch2 = System.Diagnostics.Stopwatch.StartNew();

	//	for (int i = 0; i < count; ++i)
	//	{
	//		int pos2 = 0;
	//		bs.ReadOutNew(buffer, ref pos2);
	//	}

	//	watch2.Stop();

	//	Debug.Log("Reset <b>new</b> =" + watch2.ElapsedMilliseconds + " ms");
	//	Debug.Log(buffer[0] + " " + buffer[1] + buffer[2] + " " + buffer[3] + " " + buffer[4] + " " + buffer[5] + " " + buffer[6] + " " + buffer[7] + " " + buffer[8] + " " + buffer[9]
	//		+ " " + buffer[10] + " " + buffer[11] + " " + buffer[12] + " " + buffer[13]);
	//}


	public static void ByteForByteWrite()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			BasicWriter.Reset();
			for (int i = 0; i < BYTE_CNT; ++i)
				BasicWriter.BasicWrite(buffer, 255);

			BasicWriter.Reset();
			for (int i = 0; i < BYTE_CNT; ++i)
			{
				byte b = BasicWriter.BasicRead(buffer);
			}
		}

		watch.Stop();

		Debug.Log("Byte For Byte: time=" + watch.ElapsedMilliseconds + " ms");
	}

	public static void BitpackBytesEven()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{
			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			int bitpos = 0;
			for (int i = 0; i < BYTE_CNT; ++i)
				buffer.Write(255, ref bitpos, 8);

			bitpos = 0;
			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				byte b = buffer.Read(ref bitpos, 8);
			}
		}
		
		watch.Stop();

		Debug.Log("Even Bitpack byte: time=" + watch.ElapsedMilliseconds + " ms");
	}


	public static void BitstreamTest()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{

			bs.Reset();

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			bs.WriteBool(true);

			for (int i = 0; i < 40 - 1; ++i)
				bs.Write(255, 8);

			bool ob = bs.ReadBool();

			for (int i = 0; i < 40 - 1; ++i)
			{
				byte b = (byte)bs.Read(8);
			}
		}

		watch.Stop();

		Debug.Log("Unsafe Bitstream: time=" + watch.ElapsedMilliseconds + " ms");
	}

	public static void BitstreamIndirectTest()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();

		for (int loop = 0; loop < LOOP; ++loop)
		{

			bs.Reset();
			
			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			bs.WriteBool(true);

			for (int i = 0; i < 40 - 1; ++i)
				bs.WriteByte(255);

			bool ob = bs.ReadBool();

			for (int i = 0; i < 40 - 1; ++i)
			{
				byte b = bs.ReadByte();
			}
		}

		watch.Stop();

		Debug.Log("Unsafe Bitstream w/ Indirect Calls: time=" + watch.ElapsedMilliseconds + " ms");
	}


	public static void BitpackBytesUnEven()
	{
		var watch = System.Diagnostics.Stopwatch.StartNew();
		
		for (int loop = 0; loop < LOOP; ++loop)
		{
			int bitpos = 0;

			/// First 1 bit write is to ensure all following byte writes don't align with a single byte in the byte[], 
			/// forcing worst case split across two byte[] indexs
			buffer.Write(1, ref bitpos, 1);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
				buffer.Write(255, ref bitpos, 8);

			bitpos = 0;
			byte ob = buffer.Read(ref bitpos, 1);

			for (int i = 0; i < BYTE_CNT - 1; ++i)
			{
				byte b = buffer.Read(ref bitpos, 8);
			}
		}

		watch.Stop();

		Debug.Log("Uneven Bitpack byte: time=" + watch.ElapsedMilliseconds + " ms");
	}

	static float interval = 0;
	// Update is called once per frame
	void Update()
	{
		interval += Time.deltaTime;
		if (interval > 3)
		{
			Test();
			interval = 0;
		}
	}
}

/// <summary>
/// Simulate a VERY basic byte writer. This is just to make the test more fair than inlining byte writes, as this would never be inline.
/// </summary>
public class BasicWriter
{
	public static int pos;

	public static void Reset()
	{
		pos = 0;
	}
	public static byte[] BasicWrite(byte[] buffer, byte value)
	{
		//UnityEngine.Profiling.Profiler.BeginSample("Basic Write");

		buffer[pos] = value;
		pos++;

		//UnityEngine.Profiling.Profiler.EndSample();
		return buffer;

	}

	public static byte BasicRead(byte[] buffer)
	{
		//UnityEngine.Profiling.Profiler.BeginSample("Basic Write");

		byte b = buffer[pos];
		pos++;
		return b;

		//UnityEngine.Profiling.Profiler.EndSample();

	}

}
