using Moq;

public class IdGeneratorTest
{

    //TODO: Update this and add more tests
    [Test]
    public void GenerateId_ShouldReturnId_WithTimestampMachineIdSequenceNo()
    {
        var mockNow = new Mock<Func<DateTime>>();
        mockNow.Setup(f => f()).Returns(new DateTime(2025, 1, 27));
        IdGenerator.GetDateTimeNow = mockNow.Object;

        long id = IdGenerator.GenerateId();
    }

}