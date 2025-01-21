using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.VisualBasic;

namespace ccwc.command;

public partial class CCWCCmd : ICommand
{

    private string _filename;
    private string _data;
    private List<string> _results;
    private string[] _flagsArgs;
    private TextReader? _reader;



    public CCWCCmd(string[] args)
    {
        _flagsArgs = args.Length > 1 ? args[0..(args.Length - 1)] : [];
        _filename = args.Length > 0 ? args.Last() : string.Empty;

        _reader = null;
        _data = string.Empty;
        _results = new List<string>();
    }

    public CCWCCmd(string[] args, TextReader reader) : this(args)
    {
        _reader = reader;
    }

    //flags
    public ResultCode Execute()
    {
        try
        {
            SetData(_reader ?? File.OpenText(_filename));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
            return ResultCode.FAILED;
        }


        IFlag[] cmdFlags = flagsMap.Values.OrderBy(f => f.Key).Select(f => f.Value).ToArray();
        if (_flagsArgs.Length > 0 && !TryParseFlags(_flagsArgs, out cmdFlags))
        {
            return ResultCode.FAILED;
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
        var invalidFlags = flagsSet.Where(f => !flagsMap.ContainsKey(f)).Select(f => $"'{f}'").ToArray();
        if (invalidFlags.Length > 0)
        {
            Console.Error.WriteLine($"Unknown flag(s): {string.Join(", ", invalidFlags)}");
            return false;
        }

        //create IFlag from flags
        flags = flagsSet.Select(f => flagsMap[f]).OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value).ToArray();
        return true;
    }

    private void SetData(TextReader reader)
    {
        using (reader)
        {
            _data = reader.ReadToEnd();
        }
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

