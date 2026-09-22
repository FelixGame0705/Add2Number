using Add2Number;
using Microsoft.Extensions.Logging;

// Pass -v or --verbose to also print the elementary-school-style, step-by-step
// trace of the addition (each column: digits, carry in, carry out).
bool verbose = args.Contains("-v") || args.Contains("--verbose");
LogLevel minLevel = verbose ? LogLevel.Debug : LogLevel.Information;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(minLevel);
});

ILogger logger = loggerFactory.CreateLogger("Add2Number");
var bigNumber = new MyBigNumber(logger);

Console.WriteLine("Nhap vao 2 so nguyen khong am (moi so tren mot dong): ");
string? num1 = Console.ReadLine();
string? num2 = Console.ReadLine();

if (string.IsNullOrEmpty(num1) || string.IsNullOrEmpty(num2))
{
    Console.WriteLine("Ban chua nhap du 2 so.");
    return;
}

string result = bigNumber.Sum(num1, num2);
Console.WriteLine($"{num1} + {num2} = {result}");
