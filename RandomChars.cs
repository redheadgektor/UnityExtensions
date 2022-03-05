using System;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomChars
{
    public enum Type
    {
        UpperLower,
        Lower,
        Hex
    }

    public const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    public const string lower_chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    public const string hex_chars = "abcdef0123456789";
    static char[] stringChars = new char[64];

    public static string RandomString(int length, Type type = Type.UpperLower)
    {
        if(length > 0)
        {
            length %= 96;
        }
        if(stringChars.Length != length)
        {
            stringChars = new char[length];
        }
        for (int i = 0; i < stringChars.Length; i++)
        {
            if (type == Type.UpperLower)
            {
                stringChars[i] = chars[Random.Range(0, chars.Length)];
            }else if(type == Type.Lower)
            {
                stringChars[i] = lower_chars[Random.Range(0, lower_chars.Length)];
            }
            else
            {
                stringChars[i] = hex_chars[Random.Range(0, hex_chars.Length)];
            }
        }

        return new string(stringChars);
    }

    static byte[] randomBytes = new byte[64];

    public static string RandomStringUTF8(int length)
    {
        if(randomBytes.Length != length)
        {
            Array.Resize(ref randomBytes, length);
        }
        new System.Random().NextBytes(randomBytes);

        return Encoding.UTF8.GetString(randomBytes);
    }

    public static string RandomStringASCII(int length)
    {
        if (randomBytes.Length != length)
        {
            Array.Resize(ref randomBytes, length);
        }
        new System.Random().NextBytes(randomBytes);

        return Encoding.ASCII.GetString(randomBytes);
    }

    public static string RandomStringUnicode(int length)
    {
        if (randomBytes.Length != length)
        {
            Array.Resize(ref randomBytes, length);
        }
        new System.Random().NextBytes(randomBytes);

        return Encoding.Unicode.GetString(randomBytes);
    }
}
