using ccwc.command;

namespace ccwc.command.tests;

[TestFixture]
public class CCWCCmdTests
{

    [Test]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "x", "test.txt", TestName = "Incorrect single flag format should return INVALID_ARGS with error message")]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "x", "y", "test.txt", TestName = "Incorrect single flag format should return INVALID_ARGS with error message")]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "xy", "test.txt", TestName = "Incorrect single flag format in single arg should return INVALID_ARGS with error message")]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "x", "-l", "test.txt", TestName = "Valid single flag with incorrect single flag format should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x'", "-x", "test.txt", TestName = "Invalid single flag should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x'", "-x", "-x", "test.txt", TestName = "Duplicate invalid flag should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x', 'y'", "-x", "-y", "test.txt", TestName = "Invalid multiple flags should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x', 'y'", "-xy", "test.txt", TestName = "Invalid multiple flags in single arg should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'y', 'x'", "-yx", "-l", "-x", "test.txt", TestName = "Valid flag with duplicate invalid flags should return INVALID_ARGS with error message")]
    public void Execute_ShouldReturnResultCodeFailed_AndLogToConsole_WhenInvalidFlagArgs(string expectedErrorMessage, params string[] args)
    {
        //arrange
        using var errWriter = new StringWriter();
        Console.SetError(errWriter);

        //act
        CCWCCmd ccwc = new CCWCCmd(args, "the quick brown fox jumped over the lazy dog");
        ResultCode code = ccwc.Execute();

        //assert
        Assert.That(code, Is.EqualTo(ResultCode.INVALID_ARGS));
        Assert.That(errWriter.ToString().Trim(), Is.EqualTo(expectedErrorMessage));
    }
}