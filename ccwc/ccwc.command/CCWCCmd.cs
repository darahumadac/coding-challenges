namespace ccwc.command;

using FlagOrder = int;

public partial class CCWCCmd : ICommand
{

    private string _filename;
    private IReader _reader;
    private string _data;
    private List<string> _results;
    private string[] _flagsArgs;
    private readonly Dictionary<char, KeyValuePair<FlagOrder, IFlag>> _flagsMap = new(){
        {'l', new (0, new LineCountFlag())},
        {'w', new(1, new WordCountFlag())},
        {'c', new(2, new CharBytesCountFlag())},
    };

    public CCWCCmd(string[] args, IReader reader)
    {
        _flagsArgs = args.Length > 1 ? args[0..(args.Length - 1)] : [];
        _filename = args.Length > 0 ? args.Last() : string.Empty;

        _reader = reader;
        _data = string.Empty;
        _results = new List<string>();
    }

    //flags
    public ResultCode Execute()
    {
        if (_filename == string.Empty)
        {
            Console.Error.WriteLine("Filename is expected.");
            return ResultCode.INVALID_ARGS;
        }

        try
        {
            _data = _reader.ReadToEnd(_filename);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return ResultCode.FAILED;
        }

        IFlag[] cmdFlags = _flagsMap.Values.OrderBy(f => f.Key).Select(f => f.Value).ToArray();
        if (_flagsArgs.Length > 0 && !TryParseFlags(_flagsArgs, out cmdFlags))
        {
            return ResultCode.INVALID_ARGS;
        }

        foreach (IFlag flag in cmdFlags)
        {
            flag.Execute(this);
        }
        Console.WriteLine(GetResult());
        return ResultCode.SUCCESS;
    }

    private bool TryParseFlags(string[] flagsArgs, out IFlag[] flags)
    {
        flags = [];

        if (!flagsArgs.All(f => f.StartsWith("-")))
        {
            Console.Error.WriteLine("Invalid arguments. Prefix all flags with '-'");
            return false;
        }

        //validate that all chars are valid
        HashSet<char> flagsSet = [.. string.Join("", flagsArgs).Replace("-", "").ToCharArray()];
        var invalidFlags = flagsSet.Where(f => !_flagsMap.ContainsKey(f)).Select(f => $"'{f}'").ToArray();
        if (invalidFlags.Length > 0)
        {
            Console.Error.WriteLine($"Unknown flag(s): {string.Join(", ", invalidFlags)}");
            return false;
        }

        //create IFlag from flags
        flags = flagsSet.Select(f => _flagsMap[f]).OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
        return true;
    }


    public string GetData()
    {
        return _data;
    }

    public string GetResult()
    {
        return string.Join(" ", [.. _results, _filename]);
    }

    public void AppendResult(string result)
    {
        _results.Add(result);
    }
}

