public interface IReader
{
    string ReadToEnd(string path);
}

public class FileReader : IReader
{
    public FileReader() { }

    public string ReadToEnd(string path)
    {
        string data = string.Empty;

        using (StreamReader sr = File.OpenText(path))
        {
            data = sr.ReadToEnd();
        }
        return data;
    }
}