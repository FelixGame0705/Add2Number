using System.Numerics;
using Add2Number;
using Xunit;

namespace Add2Number.Tests
{
    public class MyBigNumberTests
    {
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
    }
}
