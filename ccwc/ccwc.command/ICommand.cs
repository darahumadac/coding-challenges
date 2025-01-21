public interface ICommand
{
    ResultCode Execute();

    string GetData();
    string GetResult();
    void AppendResult(string result);
}


public enum ResultCode
{
    SUCCESS = 0,
    FAILED = 1,
    WARNING = 2
}