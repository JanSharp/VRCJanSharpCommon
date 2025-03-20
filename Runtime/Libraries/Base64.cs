using System;

namespace JanSharp
{
    public static class Base64
    {
        public static string Encode(byte[] data, Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            return Convert.ToBase64String(data, options);
        }

        public static byte[] Decode(string data)
        {
            return Convert.FromBase64String(data);
        }

        ///<summary>Returns false instead of throwing exceptions in case of invalid data.
        ///Otherwise see https://learn.microsoft.com/en-us/dotnet/api/system.convert.frombase64string?view=net-8.0#system-convert-frombase64string(system-string)</summary>
        public static bool TryDecode(string data, out byte[] result)
        {
            result = null;
            int count = 0;
            bool startedPadding = false;
            int paddingCount = 0;
            foreach (char c in data)
            {
                if ('A' <= c && c <= 'Z'
                    || 'a' <= c && c <= 'z'
                    || '0' <= c && c <= '9'
                    || c == '+'
                    || c == '/')
                {
                    if (startedPadding)
                        return false;
                    count++;
                }
                else if (c == '=')
                {
                    count++;
                    startedPadding = true;
                    paddingCount++;
                    if (paddingCount > 2)
                        return false;
                }
                else if (!char.IsWhiteSpace(c))
                    return false;
            }
            if ((count % 4) != 0)
                return false;

            result = Convert.FromBase64String(data);
            return true;
        }
    }
}
