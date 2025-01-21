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
    private char[] flagOrder = ['l', 'w', 'c'];

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
            command.AppendResult(Regex.Matches(command.GetData(), @"\w+").Count.ToString());
        }
    }

    private class CharBytesCountFlag : IFlag
    {
        public void Execute(ICommand command)
        {
            command.AppendResult(Encoding.UTF8.GetByteCount(command.GetData()).ToString());
        }
    }
}

