using ccwc.command;
using Moq;

namespace ccwc.command.tests;

[TestFixture]
public class CCWCCmdTests
{
    private Mock<IReader> _mockReader;
    private const string INVALID_FILE = "invalid_file.txt";
    private const string VALID_FILE = "test.txt";


    [OneTimeSetUp]
    public void SetupMocks()
    {
        //mock IReader using Moq
        _mockReader = new Mock<IReader>();
        _mockReader.Setup(r => r.ReadToEnd(VALID_FILE)).Returns("the quick\nbrown fox");
        _mockReader.Setup(r => r.ReadToEnd(INVALID_FILE)).Throws<FileNotFoundException>();
    }

    [Test]
    [TestCase("Filename is expected.", ResultCode.INVALID_ARGS, "", TestName = "Return invalid args when filename is empty")]
    [TestCase("", ResultCode.FAILED, INVALID_FILE, TestName = "Return invalid args when filename is invalid")]

    public void Execute_ShouldReturnUnsuccessfulResultCodes_WhenFilenameIsNotValid(string expectedErrorMessage, ResultCode expectedResultCode, params string[] args)
    {
        //arrange
        using var errWriter = new StringWriter();
        Console.SetError(errWriter);

        //act
        CCWCCmd cmd = new CCWCCmd(args, _mockReader.Object);
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
    [TestCase("Invalid arguments. Prefix all flags with '-'", "x", VALID_FILE, TestName = "Incorrect single flag format should return INVALID_ARGS with error message")]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "x", "y", VALID_FILE, TestName = "Incorrect single flag format should return INVALID_ARGS with error message")]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "xy", VALID_FILE, TestName = "Incorrect single flag format in single arg should return INVALID_ARGS with error message")]
    [TestCase("Invalid arguments. Prefix all flags with '-'", "x", "-l", VALID_FILE, TestName = "Valid single flag with incorrect single flag format should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x'", "-x", VALID_FILE, TestName = "Invalid single flag should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x'", "-x", "-x", VALID_FILE, TestName = "Duplicate invalid flag should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x', 'y'", "-x", "-y", VALID_FILE, TestName = "Invalid multiple flags should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'x', 'y'", "-xy", VALID_FILE, TestName = "Invalid multiple flags in single arg should return INVALID_ARGS with error message")]
    [TestCase("Unknown flag(s): 'y', 'x'", "-yx", "-l", "-x", VALID_FILE, TestName = "Valid flag with duplicate invalid flags should return INVALID_ARGS with error message")]
    public void Execute_ShouldReturnResultCodeInvalidArgs_AndLogToConsole_WhenInvalidFlagArgs(string expectedErrorMessage, params string[] args)
    {
        //arrange
        using var errWriter = new StringWriter();
        Console.SetError(errWriter);

        //act
        CCWCCmd ccwc = new CCWCCmd(args, _mockReader.Object);
        ResultCode code = ccwc.Execute();

        //assert
        Assert.That(code, Is.EqualTo(ResultCode.INVALID_ARGS));
        Assert.That(errWriter.ToString().Trim(), Is.EqualTo(expectedErrorMessage));
    }

    [Test]
    [TestCase($"1 4 22 {VALID_FILE}", VALID_FILE, TestName = "Output all flags in order of l,w,c when no flags are provided")]
    [TestCase($"1 4 22 {VALID_FILE}", "-l", "-w", "-c", VALID_FILE, TestName = "Output all flags in order of l,w,c when all flags are provided in multiple flags")]
    [TestCase($"1 4 22 {VALID_FILE}", "-lwc", VALID_FILE, TestName = "Output all flags in order of l,w,c when all flags are provided in single flag")]
    [TestCase($"1 4 22 {VALID_FILE}", "-w", "-cl", VALID_FILE, TestName = "Output all flags in order of l,w,c when all flags are provided in multiple flags in different formats")]
    [TestCase($"1 {VALID_FILE}", "-l", VALID_FILE, TestName = "Output only line count result for selected flag l")]
    [TestCase($"4 {VALID_FILE}", "-w", VALID_FILE, TestName = "Output only word count result for selected flag w")]
    [TestCase($"22 {VALID_FILE}", "-c", VALID_FILE, TestName = "Output only byte count result for selected flag c")]
    [TestCase($"1 22 {VALID_FILE}", "-c", "-l", VALID_FILE, TestName = "Output only selected flags in order l,w,c")]
    public void Execute_ShouldReturnSuccess_AndOutputResult_WhenFlagsAreValid(string expectedResult, params string[] args)
    {
        //arrange
        using var outputWriter = new StringWriter();
        Console.SetOut(outputWriter);

        //act
        CCWCCmd ccwc = new CCWCCmd(args, _mockReader.Object);
        ResultCode result = ccwc.Execute();

        //assert
        Assert.That(result, Is.EqualTo(ResultCode.SUCCESS));
        Assert.That(outputWriter.ToString().Trim(), Is.EqualTo(expectedResult));
    }
}