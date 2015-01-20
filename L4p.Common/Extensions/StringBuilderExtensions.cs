using System.Text;

namespace L4p.Common.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder StartNewLine(this StringBuilder sb)
        {
            int len = sb.Length;

            if (len == 0)
                return sb;

            char lastChar = sb[len - 1];

            if (lastChar == '\n')
                return sb;

            if (lastChar == '\r')
                return sb;

            sb.AppendLine();

            return sb;
        }
    }
}