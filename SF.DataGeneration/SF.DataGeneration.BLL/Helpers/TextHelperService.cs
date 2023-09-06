using System.Text;

namespace SF.DataGeneration.BLL.Helpers
{
    //public interface ITextHelperService
    //{
    //    string CleanTextExtractedFromPdf(string text);

    //}

    public static class TextHelperService
    {
        public static string CleanTextExtractedFromPdf(string text)
        {
            text = text.Replace("\r\n", "</br>");
            text = text.Replace("\n", "</br>");
            return text;
        }

        public static string GenerateRandomString(string inputString)
        {
            const string alphanumericChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            const string alphabeticChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            const string numericChars = "0123456789";

            bool containsAlphabets = inputString.Any(char.IsLetter);
            bool containsNumbers = inputString.Any(char.IsDigit);

            if (containsAlphabets && !containsNumbers)
            {
                return GenerateRandomStringOfType(alphabeticChars, inputString.Length);
            }
            else if (!containsAlphabets && containsNumbers)
            {
                return GenerateRandomStringOfType(numericChars, inputString.Length);
            }
            else
            {
                return GenerateRandomStringOfType(alphanumericChars, inputString.Length);
            }
        }

        private static string GenerateRandomStringOfType(string chars, int length)
        {
            var random = new Random();
            var sb = new StringBuilder(length);

            for (int i = 0; i < length; i++)
            {
                int index = random.Next(0, chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }

        public static string GenerateRandomColorCode()
        {
            var _random = new Random();
            byte[] colorBytes = new byte[4];
            _random.NextBytes(colorBytes);

            string colorCode = "#" + BitConverter.ToString(colorBytes).Replace("-", "").Substring(0, 8);
            return colorCode;
        }
    }
}
