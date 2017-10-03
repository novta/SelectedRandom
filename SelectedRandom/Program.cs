using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace HmacSample
{
	class MainClass
	{
		public static string ByteArrayToString(byte[] ba)
		{
			StringBuilder hex = new StringBuilder(ba.Length * 2);
			foreach (byte b in ba)
				hex.AppendFormat("{0:x2}", b);
			return hex.ToString();
			//string hex1 = BitConverter.ToString(ba);
			//return hex1.Replace("-", "");
		}
		public static string GetGroup(string hash)
		{
			return hash.Substring(0, 5);
		}

		private static SHA1 sha1;
		private static string targetPrefix;
		private static StreamWriter file;
		private static KeyGenerator keyGen = new KeyGenerator();
		public static void Main(string[] args)
		{
			string key;
			Dictionary<string, List<string>> mapa = new Dictionary<string, List<string>>(1024);
			HashSet<string> keyCollection = new HashSet<string>();

			// SHA1(0001:identity-domain.com:magic-string) = SHA1(unique-id:magic-string)
			string leftSide = "0001:identity-domain.com:magic-string";
			sha1 = new SHA1Managed();
			byte[] targetPrefixBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(leftSide.ToCharArray()));
			string fullLeftSide = ByteArrayToString(targetPrefixBytes);
			targetPrefix = GetGroup(fullLeftSide);

			using (file = new StreamWriter(@".\AllLinesSweep.txt", false))
			{
				file.WriteLine(DateTime.Now);
				long tries = 0;
				int i = 0;
				int j = 0;
				DateTime prevTime = DateTime.Now;

				for (int len = 16; len < 32; ++len)
				{
					file.WriteLine($"Len: {len}");
					tries = 0;
					i = 0;
					j = 0;
					while ((i < 10000) && (j < 10000))
					{
						key = keyGen.GetRandomKey(len);
						if (!keyCollection.Contains(key))
						{
							CheckFitting(key, ref i, ref prevTime);
						}
						else
						{
							j++;
						}
						tries++;
						Console.Write($"\rLen: {len}, iteration: {i}, duplicate: {j}, tries {tries}");
					}
					Console.WriteLine($"Len: {len}, found: {i}, RAND douplicates {j}, tries {tries}");
					keyCollection.Clear();
				}
			}
			Console.ReadKey();
		}

		private static void CheckFitting(string key, ref int i, ref DateTime prevTime)
		{
			byte[] result;
			string sresult;
			string hashMagic;
			string value;
			string group;
			hashMagic = String.Format($"{key}:magic-string");
			result = sha1.ComputeHash(Encoding.ASCII.GetBytes(hashMagic.ToCharArray()));
			sresult = ByteArrayToString(result);
			group = GetGroup(sresult);
			if (targetPrefix.Equals(group))
			{
				DateTime time = DateTime.Now;
				TimeSpan duration = time - prevTime;
				value = String.Format($"Key: {key}, SHA1: {sresult}, Time: {time}, Duration: {duration}");
				file.WriteLine(value);
				prevTime = time;
				i++;
			}
		}
	}
}