using System.Text;

// Implementation based on C# driver implementation from https://github.com/yojimbo87/ArangoDB-NET 
// with some bug fixes and extensions to support asynchronous operations 
namespace Maverick.Client.ArangoDb.ExternalLibraries.fastJSON;

internal static class Formatter
{
    public static string Indent = "   ";

    public static void AppendIndent(StringBuilder sb, int count)
    {
        for (; count > 0; --count)
        {
            sb.Append(Indent);
        }
    }

    public static string PrettyPrint(string input)
    {
        var output = new StringBuilder();
        var depth = 0;
        int len = input.Length;
        char[] chars = input.ToCharArray();

        for (var i = 0; i < len; ++i)
        {
            char ch = chars[i];

            if (ch == '\"') // found string span
            {
                var str = true;

                while (str)
                {
                    output.Append(ch);
                    ch = chars[++i];

                    if (ch == '\\')
                    {
                        output.Append(ch);
                        ch = chars[++i];
                    }
                    else if (ch == '\"')
                    {
                        str = false;
                    }
                }
            }

            switch (ch)
            {
                case '{':
                case '[':
                    output.Append(ch);
                    output.AppendLine();
                    AppendIndent(output, ++depth);

                    break;
                case '}':
                case ']':
                    output.AppendLine();
                    AppendIndent(output, --depth);
                    output.Append(ch);

                    break;
                case ',':
                    output.Append(ch);
                    output.AppendLine();
                    AppendIndent(output, depth);

                    break;
                case ':':
                    output.Append(" : ");

                    break;
                default:
                    if (!char.IsWhiteSpace(ch))
                    {
                        output.Append(ch);
                    }

                    break;
            }
        }

        return output.ToString();
    }
}
