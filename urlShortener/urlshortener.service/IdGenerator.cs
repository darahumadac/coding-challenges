namespace urlshortener.service;
public static class IdGenerator
{
    private static long _lastTs = 0;
    private static uint _sequenceNumber = 0;
    private static object _sequenceLock = new object();

    public static readonly DateTime Epoch = new DateTime(2025, 1, 1);

    private static Func<DateTime> _getDateTimeNow = () => DateTime.UtcNow;
    public static Func<DateTime> GetDateTimeNow
    {
        get
        {
            return _getDateTimeNow;
        }

        set
        {
            _getDateTimeNow = value;
        }
    }


    public static long GenerateId()
    {
        long currentUnixTs = (long)(GetDateTimeNow() - Epoch).TotalMilliseconds;
        uint machineId = 1;
        //for mini project implementation:
        //create 64-bit id using timestamp (42-bits), machine id (5-bits), sequence number
        long id = (currentUnixTs << 22) | (machineId << 17) | getNextSequenceNumber(currentUnixTs);
        // Console.WriteLine($"{Convert.ToString(currentUnixTs, 2)}, {Convert.ToString(machineId, 2)}, {Convert.ToString(_sequenceNumber)}");
        // Console.WriteLine($"{Convert.ToString(id, 2)}");
        return id;
    }

    private static uint getNextSequenceNumber(long currentUnixTs)
    {
        lock (_sequenceLock)
        {
            if (currentUnixTs != _lastTs)
            {
                _sequenceNumber = 0;
                _lastTs = currentUnixTs;
            }

            return _sequenceNumber++;
        }

    }
}