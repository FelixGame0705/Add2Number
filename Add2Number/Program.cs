using Add2Number;
using Microsoft.Extensions.Logging;

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .SetMinimumLevel(LogLevel.Information);
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
