using System.Numerics;
using Add2Number;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Add2Number.Tests
{
    public class MyBigNumberTests
    {
        private readonly ITestOutputHelper _output;

        public MyBigNumberTests(ITestOutputHelper output)
        {
            _output = output;
        }
        [Theory]
        // Example straight from the requirement document.
        [InlineData("1234", "897", "2131")]
        // Same length, no carry at all.
        [InlineData("123", "456", "579")]
        // Single digit with carry.
        [InlineData("5", "7", "12")]
        // stn1 shorter than stn2.
        [InlineData("9", "999", "1008")]
        // stn1 longer than stn2.
        [InlineData("999", "9", "1008")]
        // Carry ripples through every single column.
        [InlineData("999", "1", "1000")]
        [InlineData("9999999999", "1", "10000000000")]
        // Zero on one side.
        [InlineData("0", "12345", "12345")]
        [InlineData("12345", "0", "12345")]
        // Both zero.
        [InlineData("0", "0", "0")]
        // Leading zeros in the operands must not leak into the result.
        [InlineData("007", "3", "10")]
        [InlineData("000123", "000456", "579")]
        // A very large number (bigger than long.MaxValue) added to a small one.
        [InlineData("99999999999999999999", "1", "100000000000000000000")]
        // Two very large numbers of different lengths.
        [InlineData("123456789012345678901234567890", "9876543210", "123456789012345678911111111100")]
        public void Sum_ReturnsExpectedResult(string stn1, string stn2, string expected)
        {
            var bigNumber = new MyBigNumber();

            string actual = bigNumber.Sum(stn1, stn2);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("1", "2")]
        [InlineData("184467440737095516150", "9999999999999999999999999999")]
        [InlineData("40000000000000000000000000000000000001", "9999999999999999999999999999999999999")]
        [InlineData("0", "999999999999999999999999999999999999999999")]
        public void Sum_MatchesBigIntegerForLargeValues(string stn1, string stn2)
        {
            var bigNumber = new MyBigNumber();
            string expected = (BigInteger.Parse(stn1) + BigInteger.Parse(stn2)).ToString();

            string actual = bigNumber.Sum(stn1, stn2);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Sum_IsCommutative()
        {
            var bigNumber = new MyBigNumber();

            string result1 = bigNumber.Sum("31415926535897932384626433832795", "271828182845904523536");
            string result2 = bigNumber.Sum("271828182845904523536", "31415926535897932384626433832795");

            Assert.Equal(result1, result2);
        }

        [Fact]
        public void Constructor_WithNullLogger_FallsBackToNullLoggerInsteadOfThrowing()
        {
            // Covers the false branch of `logger ?? NullLogger.Instance`: passing
            // an explicit null must not blow up with a NullReferenceException the
            // first time Sum() tries to log.
            var bigNumber = new MyBigNumber(null!);

            string result = bigNumber.Sum("2", "3");

            Assert.Equal("5", result);
        }

        [Fact]
        public void Sum_WithNullFirstOperand_ThrowsArgumentNullException()
        {
            var bigNumber = new MyBigNumber();

            Assert.Throws<ArgumentNullException>(() => bigNumber.Sum(null!, "1"));
        }

        [Fact]
        public void Sum_WithNullSecondOperand_ThrowsArgumentNullException()
        {
            var bigNumber = new MyBigNumber();

            Assert.Throws<ArgumentNullException>(() => bigNumber.Sum("1", null!));
        }

        [Fact]
        public void Sum_WithBothOperandsEmpty_ReturnsZero()
        {
            var bigNumber = new MyBigNumber();

            string actual = bigNumber.Sum("", "");

            Assert.Equal("0", actual);
        }

        [Fact]
        public void Sum_WithOneOperandEmpty_TreatsItAsZero()
        {
            var bigNumber = new MyBigNumber();

            string actual = bigNumber.Sum("", "42");

            Assert.Equal("42", actual);
        }

        [Fact]
        public void Sum_RecordsEachCallInHistory()
        {
            var bigNumber = new MyBigNumber();

            bigNumber.Sum("1", "2");
            bigNumber.Sum("10", "20");

            Assert.Equal(2, bigNumber.History.Count);

            Assert.Equal("1", bigNumber.History[0].Operand1);
            Assert.Equal("2", bigNumber.History[0].Operand2);
            Assert.Equal("3", bigNumber.History[0].Result);

            Assert.Equal("10", bigNumber.History[1].Operand1);
            Assert.Equal("20", bigNumber.History[1].Operand2);
            Assert.Equal("30", bigNumber.History[1].Result);
        }

        [Fact]
        public void Sum_DoesNotMutateHistoryFromOutside()
        {
            var bigNumber = new MyBigNumber();
            bigNumber.Sum("1", "1");

            Assert.IsAssignableFrom<IReadOnlyList<BigNumberOperation>>(bigNumber.History);
        }

        [Fact]
        public void Sum_WithDebugLoggingEnabled_LogsEachAdditionStep()
        {
            var logger = new RecordingLogger(LogLevel.Debug);
            var bigNumber = new MyBigNumber(logger);

            bigNumber.Sum("1234", "897");

            var stepLogs = logger.Entries.Where(e => e.Level == LogLevel.Debug).ToList();

            // "1234" has 4 digits, so there must be exactly one step logged per column.
            Assert.Equal(4, stepLogs.Count);
            Assert.Contains("Buoc 1", stepLogs[0].Message);
            Assert.Contains("Buoc 4", stepLogs[3].Message);

            Assert.Contains(
                logger.Entries,
                e => e.Level == LogLevel.Information && e.Message.Contains("2131"));
        }

        [Fact]
        public void Sum_WithDebugLoggingEnabled_PrintsEachStepToTestOutput()
        {
            // Same trace as above, but written to the xUnit test output instead of
            // just being asserted on - run this one test to actually read the
            // "Buoc 1: Lay 4 cong voi 7..." lines (Test Explorer's Output pane, or
            // `dotnet test --filter Sum_WithDebugLoggingEnabled_PrintsEachStepToTestOutput -v n`).
            var logger = new RecordingLogger(LogLevel.Debug);
            var bigNumber = new MyBigNumber(logger);

            string result = bigNumber.Sum("1234", "897");

            foreach (var entry in logger.Entries)
            {
                _output.WriteLine($"[{entry.Level}] {entry.Message}");
            }

            Assert.Equal("2131", result);
        }

        [Fact]
        public void Sum_WithoutDebugLoggingEnabled_DoesNotLogSteps()
        {
            var logger = new RecordingLogger(LogLevel.Information);
            var bigNumber = new MyBigNumber(logger);

            bigNumber.Sum("1234", "897");

            Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Debug);
            Assert.Contains(logger.Entries, e => e.Level == LogLevel.Information);
        }

        [Fact]
        public void Sum_HandlesMultiMillionDigitOperandsQuicklyAndCorrectly()
        {
            const int digitCount = 2_000_000;

            // stn1 = 2,000,000 nines, so the addition below carries all the way
            // through every single column, the worst case for this algorithm.
            string stn1 = new string('9', digitCount);
            string stn2 = "1";
            string expected = "1" + new string('0', digitCount);

            var bigNumber = new MyBigNumber();

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string actual = bigNumber.Sum(stn1, stn2);
            stopwatch.Stop();

            Assert.Equal(expected, actual);
            Assert.True(
                stopwatch.ElapsedMilliseconds < 2000,
                $"Sum() took {stopwatch.ElapsedMilliseconds} ms for {digitCount} digits, expected well under 2000 ms.");
        }
    }
}
