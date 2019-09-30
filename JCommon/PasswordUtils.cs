using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace JExtensions
{
    public static class PasswordUtils
    {
        public static int MIN_PLAYER_NAME_LENTH = 4;
        public static int MAX_PLAYER_NAME_LENTH = 14;
        private static readonly string[] LIMIT_CHAR = new string[] { "%", ",", "*", "^", "#", "$", "&", ":", "_", "[", "]", "|" };

        public static bool ValidLogin(this string str)
        {
            if (ValidStrLength(str, MIN_PLAYER_NAME_LENTH, MAX_PLAYER_NAME_LENTH))
            {
                return ValidNamePattern(str);
            }
            return false;
        }

        public static bool ValidPassword(this string str)
        {
            if (ValidStrLength(str, MIN_PLAYER_NAME_LENTH, MAX_PLAYER_NAME_LENTH))
            {
                return ValidNamePattern(str);
            }
            return false;
        }

        public static bool ComparePasswords(string Encrypted, string Decrypted)
        { 
            return Decrypt(Encrypted) == Decrypted;
        }

        private static bool ValidNamePattern(string name)
        {
            if (IsBlank(name))
            {
                return false;
            }

            foreach (string element in LIMIT_CHAR)
            {
                if (name.Contains(element))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsBlank(this string name)
        {
            return string.IsNullOrEmpty(name);
        }

        public static bool IsAlphaNumericNoSpace(this string data)
        {
            return Regex.IsMatch(data, @"^[a-zA-Z0-9]+$");
        }

        public static bool IsAlphaNumericWithSpaceSpace(this string data)
        {
            return Regex.IsMatch(data, @"^[a-zA-Z0-9 ]+$");
        }

        public static bool IsAllowedUsername(this string data)
        {
            return Regex.IsMatch(data, @"^[a-zA-Z0-9]+$") && ValidLenth(data.Length, 4, 14);
        }

        public static bool IsAllowedPassWord(this string data)
        {
            return Regex.IsMatch(data, @"^[a-zA-Z0-9]+$") && !ValidLenth(data.Length, 4, 14);
        }

        private static bool ValidLenth(int length, int min, int max)
        {
            return length >= min && length <= max;
        }

        private static int ToEncode(string element, Encoding encoding)
        {
            return encoding.GetBytes(element).Count();
        }

        private static int ToDefaultEncoding(string element)
        {
            return Encoding.UTF8.GetBytes(element).Count();
        }

        private static bool ValidStrLength(string element, int minLength, int maxLength)
        {
            try
            {
                return ValidLenth(ToEncode(element, Encoding.UTF8), minLength, maxLength);
            }
            catch
            {
            }
            try
            {
                return ValidLenth(ToEncode(element, Encoding.ASCII), minLength, maxLength);
            }
            catch
            {
            }
            try
            {
                return ValidLenth(ToEncode(element, Encoding.Default), minLength, maxLength);
            }
            catch
            {
            }
            return ValidLenth(ToDefaultEncoding(element), minLength, maxLength);
        }

        private static readonly byte[] key = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private static readonly byte[] iv = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };

        public static string Crypt(this string text)
        {
            using (SymmetricAlgorithm algorithm = DES.Create())
            {
                ICryptoTransform transform = algorithm.CreateEncryptor(key, iv);
                byte[] inputbuffer = Encoding.Unicode.GetBytes(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Convert.ToBase64String(outputBuffer);
            }
        }

        public static string Decrypt(this string text)
        {
            using (SymmetricAlgorithm algorithm = DES.Create())
            {
                ICryptoTransform transform = algorithm.CreateDecryptor(key, iv);
                byte[] inputbuffer = Convert.FromBase64String(text);
                byte[] outputBuffer = transform.TransformFinalBlock(inputbuffer, 0, inputbuffer.Length);
                return Encoding.Unicode.GetString(outputBuffer);
            }
        }

    }
}
