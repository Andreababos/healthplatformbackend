using System;
using System.Linq;

namespace RS.NetDiet.Therapist.Api.Infrastructure
{
    public static class PasswordGenerator
    {
        private const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        private const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string digits = "0123456789";
        private const string nonAlphanumeric = "`~!@#$%^&*()-_=+[{]}\\|;:'\",<.>/?";
        private static Random random = new Random(DateTime.Now.Millisecond);

        public static string Generate(int length = 8, bool useLowerCase = true, bool useUpperCase = true, bool useDigits = true, bool useNonAlphanumeric = true)
        {
            var password = "";
            var charsToUse = "";
            if (useLowerCase) { charsToUse += lowerCase; password += lowerCase[random.Next(26)]; }
            if (useUpperCase) { charsToUse += upperCase; password += upperCase[random.Next(26)]; }
            if (useDigits) { charsToUse += digits; password += digits[random.Next(10)]; }
            if (useNonAlphanumeric) { charsToUse += nonAlphanumeric; password += nonAlphanumeric[random.Next(32)]; }

            if (password.Length == 0)
            {
                throw new Exception("Select at least one type of characters to use for password generation");
            }

            if (password.Length >= length)
            {
                throw new Exception(string.Format("Lenght must be at least {0}", password.Length));
            }

            return password + new string(Enumerable.Repeat(charsToUse, length - password.Length).Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}