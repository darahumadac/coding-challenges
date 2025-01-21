using ccwc.command;

namespace ccwc.command.tests;

[TestFixture]
public class CCWCCmdTests
{

    [Test]
    [TestCase("Filename is expected.", ResultCode.INVALID_ARGS, "", TestName = "Return invalid args when filename is empty")]
    [TestCase("", ResultCode.FAILED, "test.txt", TestName = "Return invalid args when filename is invalid")]

    public void Execute_ShouldReturnUnsuccessfulResultCodes_WhenFilenameIsNotValid(string expectedErrorMessage, ResultCode expectedResultCode, params string[] args)
    {
        //arrange
        using var errWriter = new StringWriter();
        Console.SetError(errWriter);

        //act
        CCWCCmd cmd = new CCWCCmd(args);
        ResultCode result = cmd.Execute();

        //assert
        Assert.That(result, Is.EqualTo(expectedResultCode));
        string errMsg = errWriter.ToString().Trim();

        if (expectedResultCode == ResultCode.FAILED)
        {
            Assert.That(errMsg, Is.Not.Empty);
        }
        else
        {
            Assert.That(errMsg, Is.EqualTo(expectedErrorMessage));
        }
    }

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
    public void Execute_ShouldReturnResultCodeInvalidArgs_AndLogToConsole_WhenInvalidFlagArgs(string expectedErrorMessage, params string[] args)
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

    [Test]
    [TestCase("1 4 22 test.txt", "test.txt", TestName = "Output all flags in order of l,w,c when no flags are provided")]
    [TestCase("1 4 22 test.txt", "-l", "-w", "-c", "test.txt", TestName = "Output all flags in order of l,w,c when all flags are provided in multiple flags")]
    [TestCase("1 4 22 test.txt", "-lwc", "test.txt", TestName = "Output all flags in order of l,w,c when all flags are provided in single flag")]
    [TestCase("1 4 22 test.txt", "-w", "-cl", "test.txt", TestName = "Output all flags in order of l,w,c when all flags are provided in multiple flags in different formats")]
    [TestCase("1 test.txt", "-l", "test.txt", TestName = "Output only line count result for selected flag l")]
    [TestCase("4 test.txt", "-w", "test.txt", TestName = "Output only word count result for selected flag w")]
    [TestCase("22 test.txt", "-c", "test.txt", TestName = "Output only byte count result for selected flag c")]
    [TestCase("1 22 test.txt", "-c", "-l", "test.txt", TestName = "Output only selected flags in order l,w,c")]
    public void Execute_ShouldReturnSuccess_AndOutputResult_WhenFlagsAreValid(string expectedResult, params string[] args)
    {
        //arrange
        using var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);

        //act
        CCWCCmd ccwc = new CCWCCmd(args, "the quick\nbrown fox");
        ResultCode result = ccwc.Execute();

        //assert
        Assert.That(result, Is.EqualTo(ResultCode.SUCCESS));
        Assert.That(outputWriter.ToString().Trim(), Is.EqualTo(expectedResult));
    }

    //TODO: Add test for testing file reading
}