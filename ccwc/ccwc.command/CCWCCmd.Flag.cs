using System.Text;
using System.Text.RegularExpressions;

namespace ccwc.command;


public partial class CCWCCmd
{
    private Dictionary<char, KeyValuePair<int, IFlag>> flagsMap = new(){
        {'l', new (0, new LineCountFlag())},
        {'w', new(1, new WordCountFlag())},
        {'c', new(2, new CharBytesCountFlag())},
    };

    private class LineCountFlag : IFlag
    {
        public void Execute(ICommand command)
        {
            command.AppendResult(Regex.Matches(command.GetData(), @"\n").Count.ToString());
        }
    }
    private class WordCountFlag : IFlag
    {
        public void Execute(ICommand command)
        {
            command.AppendResult(Regex.Matches(command.GetData(), @"\S+").Count.ToString());
        }
    }

    private class CharBytesCountFlag : IFlag
    {
        public void Execute(ICommand command)
        {
            Encoding utf8 = Encoding.UTF8;
            /*
            - To achieve same byte count with wc -c in linux, must make sure to add the BOM (Byte Order Mark).
            - A byte order mark (BOM) is a sequence of bytes that indicates the encoding and byte order of a text file. 
                It's a Unicode character that's usually placed at the beginning of a file.
            - The BOM tells software how to interpret the bytes in a file.
            - C# GetByteCount does not count the byte order mark because the BOM is not part of the encoded characters.
                However, Unix/Linux counts the BOM as part of the byte count. So, we need to get the BOM length and add it
                to GetByteCount result.
            */
            int bomByteCount = utf8.GetPreamble().Length;
            command.AppendResult((utf8.GetByteCount(command.GetData()) + bomByteCount).ToString());
        }
    }
}

