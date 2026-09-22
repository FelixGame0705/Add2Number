# Add2Number

Cài đặt thuật toán cộng 2 số lớn được biểu diễn dưới dạng chuỗi (string), theo đúng cách học sinh Tiểu học thực hiện phép cộng: duyệt hai chuỗi từ phải sang trái, cộng từng cặp kí số kèm số nhớ (carry).

## 1. Cấu trúc project

```
Add2Number/
├── Add2Number.sln              # Solution
├── Add2Number/                 # Project chính (core + console app)
│   ├── MyBigNumber.cs           # Lớp lõi: class MyBigNumber, method Sum(string, string)
│   ├── Program.cs               # Console app gọi MyBigNumber để demo
│   └── Add2Number.csproj
└── Add2Number.Tests/            # Project Unit Test (tách riêng khỏi mã nguồn chính)
    ├── MyBigNumberTests.cs
    └── Add2Number.Tests.csproj
```

## 2. Yêu cầu công cụ

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) trở lên (project dùng `net8.0`).
- Có thể build/chạy bằng CLI (`dotnet`) hoặc mở `Add2Number.sln` bằng Visual Studio 2022+.

## 3. Thiết kế

### `MyBigNumber` (Add2Number/MyBigNumber.cs)

```csharp
public class MyBigNumber
{
    public string Sum(string stn1, string stn2) { ... }
    public IReadOnlyList<BigNumberOperation> History { get; }
}
```

- `Sum(stn1, stn2)`: cộng hai số nguyên không âm biểu diễn dưới dạng chuỗi kí số, trả về kết quả cũng là chuỗi kí số (không có số 0 thừa ở đầu, trừ khi kết quả là `"0"`).
  - Thuật toán: dùng 2 con trỏ chạy từ cuối chuỗi về đầu, cộng từng cặp kí số + carry, giống hệt ví dụ `sum("1234", "897")` trong yêu cầu đề bài.
  - Không giới hạn độ dài chuỗi (số "lớn" tùy ý), không phụ thuộc kiểu số nguyên có sẵn của ngôn ngữ (`int`, `long`).
  - Theo giả định của đề bài, tham số truyền vào được coi là hợp lệ (chỉ chứa kí số), nên không xử lý lỗi định dạng dữ liệu. Chỉ kiểm tra `null` cho tham số vì đây là ranh giới (boundary) của một API dùng chung cho nhóm khác.
- **Logging / lịch sử phép toán**: mỗi lần gọi `Sum`, phép toán được:
  1. Ghi log qua `Microsoft.Extensions.Logging` (interface `ILogger` được inject vào constructor; console app cấu hình sẵn `AddConsole()`).
     - Ở mức `Information`: log 1 dòng tổng kết `Sum("a", "b") = "c"`.
     - Ở mức `Debug`: log chi tiết **từng bước cộng từng cột kí số** (giống hệt cách trình bày `Bước 1`, `Bước 2`, ... trong ví dụ của đề bài) — chỉ bật khi cần, vì format/log từng bước sẽ chậm hơn khi cộng số có hàng triệu chữ số nếu bật mặc định.
  2. Lưu vào `History` (danh sách `BigNumberOperation { Operand1, Operand2, Result, TimestampUtc }`) để có thể kiểm tra lại lịch sử các phép cộng đã thực hiện trong bộ nhớ, kể cả khi không cần đọc log console.

### `Program.cs`

Console app đơn giản: đọc 2 số từ bàn phím (mỗi số 1 dòng), gọi `MyBigNumber.Sum`, in kết quả và log. Thêm flag `-v` (hoặc `--verbose`) để bật log `Debug`, xem chi tiết từng bước cộng.

## 4. Build

```bash
dotnet build Add2Number.sln
```

## 5. Chạy console app

```bash
dotnet run --project Add2Number/Add2Number.csproj
```

Ví dụ:

```
Nhap vao 2 so nguyen khong am (moi so tren mot dong):
999
1
999 + 1 = 1000
info: Add2Number[0]
      Sum("999", "1") = "1000"
```

Xem chi tiết từng bước cộng (thêm `-v`):

```bash
dotnet run --project Add2Number/Add2Number.csproj -- -v
```

```
Nhap vao 2 so nguyen khong am (moi so tren mot dong):
1234
897
dbug: Add2Number[0]
      Buoc 1: Lay 4 cong voi 7 duoc 11. Luu 1 vao ket qua. Ghi nho 1.
dbug: Add2Number[0]
      Buoc 2: Lay 3 cong voi 9 duoc 12. Cong tiep voi nho 1 duoc 13. Luu 3 vao ket qua. Ghi nho 1.
dbug: Add2Number[0]
      Buoc 3: Lay 2 cong voi 8 duoc 10. Cong tiep voi nho 1 duoc 11. Luu 1 vao ket qua. Ghi nho 1.
dbug: Add2Number[0]
      Buoc 4: Lay 1 cong voi 0 duoc 1. Cong tiep voi nho 1 duoc 2. Luu 2 vao ket qua.
1234 + 897 = 2131
info: Add2Number[0]
      Sum("1234", "897") = "2131"
```

## 6. Chạy Unit Test

Unit test được viết bằng **xUnit**, đặt trong project riêng `Add2Number.Tests` (tham chiếu tới project `Add2Number` qua `ProjectReference`), theo đúng khuyến khích của đề bài là tách project test khỏi mã nguồn chính.

```bash
dotnet test Add2Number.sln
```

Các nhóm test case trong `MyBigNumberTests.cs`:

- Ví dụ mẫu trong đề bài (`sum("1234", "897") == "2131"`).
- Cộng không nhớ / có nhớ ở một cột / nhớ lan truyền qua toàn bộ số (ví dụ `"999" + "1" == "1000"`).
- Hai chuỗi khác độ dài (số thứ nhất ngắn hơn, và ngược lại).
- Có số `0` (một bên bằng 0, cả hai bên bằng 0).
- Chuỗi nhập có số 0 ở đầu (leading zeros), kết quả phải được rút gọn đúng.
- Số rất lớn (vượt quá `long.MaxValue`, vượt quá cả `decimal`), đối chiếu kết quả với `System.Numerics.BigInteger` để đảm bảo đúng với số cực lớn.
- Tính giao hoán (`Sum(a, b) == Sum(b, a)`).
- Chuỗi rỗng ở một hoặc cả hai tham số.
- Tham số `null` phải ném `ArgumentNullException`.
- Lịch sử (`History`) ghi nhận đúng thứ tự và nội dung các phép cộng đã thực hiện.

## 7. Phiên bản

Bản hoàn thành để đánh giá được gắn tag `0.0.1`:

```bash
git tag -a 0.0.1 -m "Add2Number v0.0.1"
git push origin 0.0.1
```
