using System.Text;

namespace crt_cpp_app
{
    internal static class StringBuilderEx
    {
        public static string END_LINE = "\n";

        public static StringBuilder EmplaceLine(this StringBuilder sb, string line)
        {
            sb.Append(line);
            sb.Append(END_LINE);
            return sb;
        }

        public static StringBuilder EmplaceLine(this StringBuilder sb)
        {
            sb.Append(END_LINE);
            return sb;
        }
    }
}