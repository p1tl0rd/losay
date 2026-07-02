---
description: Hướng dẫn từng bước cách khởi tạo và debug một luồng kết nối PLC/IoT Mới.
---

# Workflow: Thêm luồng kết nối dữ liệu máy móc Mới (IoT/PLC)

Khi có yêu cầu kết nối với một loại thiết bị mới (ví dụ: `Sensor Nhiệt Độ`, `Máy Bơm`, `PLC Siemens`), hãy thực hiện các bước sau theo đúng trình tự.

## Bước 1: Khởi tạo Lớp Giao tiếp Phần cứng (Hardware Layer)
- Thêm file Reader trong thư mục `Hardware` hoặc `Services/Readers`. Vd: `SiemensReader.cs`.
- Triển khai hàm kết nối thiết bị lấy mảng `Raw Data`.
- **Debug:** Viết Console App nhỏ hoặc Unit test gọi thẳng hàm Reader này. Nếu nó in ra được cái mảng số (ví dụ: `[855, 12, 0]`) là thành công Bước 1. Cấm đi tiếp nếu chưa ra số.

## Bước 2: Xây dựng Lớp Nghiệp Vụ (Service Layer)
- Mở SQL Server, tạo Bảng lưu trữ. **Lưu ý rule số 2**: Khuyến khích xài cấu trúc dạng JSON hoặc EAV, không fix cứng column.
- Thêm file Service kết thừa Interface tương ứng (Vd: `ISiemensService`, `SiemensService.cs`).
- Nhúng (Inject) cái Reader ở Bước 1 vào.
- Chuyển `Raw Data` thành `Business Object`. Ví dụ lấy mảng `[855]` chia cho `10.0` thành Object `{ Temp: 85.5 }`.
- **Debug:** Dùng Swagger UI (hoặc Postman) viết tạm 1 cái API `[HttpGet]` gọi thẳng Service này. Nếu API trả về JSON `{ "Temp": 85.5 }` là thành công Bước 2. Cấm đi tiếp nếu số chưa đẹp.

## Bước 3: Đóng Hộp Background Worker 
- Thêm file Worker (Vd: `SiemensDataFetcher.cs`) kế thừa `BackgroundService`.
- Viết vòng lặp `while (!stoppingToken.IsCancellationRequested)` và `Task.Delay`.
- Tại mỗi vòng lặp:
  1. `using var scope = _serviceProvider.CreateScope();` -> **BẮT BUỘC RẤT QUAN TRỌNG** để tránh lỗi Thread Context của Database.
  2. Gọi `SiemensService` lấy Data đẹp.
  3. Lưu xuống Database (`dbContext.Add(...)`).
- Vào `Program.cs` thêm dòng `builder.Services.AddHostedService<SiemensDataFetcher>();`.
- **Debug:** Chạy App (F5). Mở SQL Server Management Studio lên xem Database có tự động Insert ra dòng mới cứ mỗi 5 giây không. Nếu Data nổ ra đều đặn là thành công Bước 3.

## Bước 4: Lên Sóng Realtime (SignalR)
- Nếu App chưa có SignalR thì cài vào xài. Nếu có rồi thì mở file Hub ra (Vd: `NotificationHub.cs` hoặc `PlcHub.cs`).
- Tại dòng lưu Database ở Bước 3, chèn thêm 1 dòng code phát loa: `await _hubContext.Clients.All.SendAsync("ReceiveSiemensData", dataObject);`.
- Nếu có phân quyền xem xưởng nào máy nấy, hãy dùng `Clients.Group(...)` thay vì `.All`.
- **Debug:** Có thể cài chorme extension `SignalR Client` để bắt thử cục Gói tin gửi ra chuẩn chưa.

## Bước 5: Móc lên Giao Diện (Blazor/React/Angular)
- Phía Fontend, tạo Component màn hình Monitor.
- Connect vào con đường SignalR (`/signalr/plcHub`).
- Đăng ký sự kiện lắng nghe (VD Blazor: `hubConnection.On<Đúng_Object_Vừa_Tạo>("ReceiveSiemensData", (data) => { ... })`).
- Bơm thẳng cái biến `data` đó vào Model của thư viện vẽ Biểu đồ (như: ChartJS, OxyPlot, MudBlazor Chart).
- **Hoàn Tất!** Chạy ra xem thành phẩm biểu đồ nhấp nháy 🚀.
