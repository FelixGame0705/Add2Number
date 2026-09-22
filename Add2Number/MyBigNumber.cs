using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Add2Number
{
    /// <summary>
    /// Represents one completed addition, kept so the operation history can be inspected
    /// (in addition to whatever the logger writes out).
    /// </summary>
    public sealed record BigNumberOperation(string Operand1, string Operand2, string Result, DateTime TimestampUtc);

    /// <summary>
    /// Core algorithm class. Adds two non-negative integers represented as decimal digit
    /// strings, the same way it is taught in elementary school: walk both strings from the
    /// last character to the first, add digit by digit while carrying the overflow to the
    /// next column.
    ///
    /// Per the requirement, inputs are assumed to already be valid (only digit characters,
    /// non-null), so no input-format validation is performed here beyond guarding against
    /// null references, which is a normal boundary check for a public API.
    /// </summary>
    public class MyBigNumber
    {
        private readonly ILogger _logger;
        private readonly List<BigNumberOperation> _history = new();

        /// <summary>History of every Sum(...) call made through this instance.</summary>
        public IReadOnlyList<BigNumberOperation> History => _history.AsReadOnly();

        public MyBigNumber() : this(NullLogger.Instance)
        {
        }

        public MyBigNumber(ILogger logger)
        {
            _logger = logger ?? NullLogger.Instance;
        }

        /// <summary>
        /// Adds two big numbers given as strings of decimal digits and returns the result,
        /// also as a string of decimal digits (no leading zeros, except the value "0" itself).
        /// </summary>
        public string Sum(string stn1, string stn2)
        {
            ArgumentNullException.ThrowIfNull(stn1);
            ArgumentNullException.ThrowIfNull(stn2);

            var result = ComputeSum(stn1, stn2, _logger);

            _history.Add(new BigNumberOperation(stn1, stn2, result, DateTime.UtcNow));
            _logger.LogInformation("Sum(\"{Stn1}\", \"{Stn2}\") = \"{Result}\"", stn1, stn2, result);

            return result;
        }

        private static string ComputeSum(string stn1, string stn2, ILogger logger)
        {
            int maxLen = Math.Max(stn1.Length, stn2.Length) + 1;
            var buffer = new char[maxLen];

            int pos = maxLen;
            int i = stn1.Length - 1;
            int j = stn2.Length - 1;
            int carry = 0;
            int step = 1;

            bool traceSteps = logger.IsEnabled(LogLevel.Debug);

            while (i >= 0 || j >= 0 || carry > 0)
            {
                int digit1 = i >= 0 ? stn1[i] - '0' : 0;
                int digit2 = j >= 0 ? stn2[j] - '0' : 0;
                int carryIn = carry;

                int columnSum = digit1 + digit2 + carry;
                carry = columnSum / 10;
                int savedDigit = columnSum % 10;
                buffer[--pos] = (char)('0' + savedDigit);

                if (traceSteps)
                {
                    string carryInPhrase = carryIn > 0
                        ? $" Cong tiep voi nho {carryIn} duoc {columnSum}."
                        : string.Empty;
                    string carryOutPhrase = carry > 0
                        ? $" Ghi nho {carry}."
                        : string.Empty;

                    logger.LogDebug(
                        "Buoc {Step}: Lay {Digit1} cong voi {Digit2} duoc {PartialSum}.{CarryInPhrase} Luu {SavedDigit} vao ket qua.{CarryOutPhrase}",
                        step, digit1, digit2, digit1 + digit2, carryInPhrase, savedDigit, carryOutPhrase);
                }

                i--;
                j--;
                step++;
            }

            if (pos == maxLen)
            {
                // Both operands were empty: treat the sum as "0".
                buffer[--pos] = '0';
            }

            int start = pos;
            while (start < maxLen - 1 && buffer[start] == '0')
            {
                start++;
            }

            return new string(buffer, start, maxLen - start);
        }
    }
}
