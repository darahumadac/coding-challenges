public interface ICommand
{
    ResultCode Execute();

    string GetData();
    string GetResult();
    void AppendResult(string result);
}


public enum ResultCode
{
    SUCCESS,
    FAILED,
    WARNING,
    INVALID_ARGS
}