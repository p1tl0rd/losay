---
name: Mẫu Kiến trúc Hệ thống SCADA (SCADA Pattern)
description: Hướng dẫn tích hợp một loại máy móc (PLC/Cảm biến) MỚI vào hệ thống Web SCADA hiện hữu bằng cấu trúc 3 lớp đã được chứng minh hiệu quả.
---

# Kỹ năng Triển khai hệ thống Web SCADA (IoT)

Áp dụng kỹ năng này khi User yêu cầu:
- Tích hợp thêm một dòng máy móc mới (ví dụ: `Siemens S7`, `Cân Nhựa`).
- Đưa thêm dữ liệu máy móc lên hiển thị Web Mới.

## Cấu trúc 3 Lớp Bắt Buộc 

### Lớp 1: Giao tiếp Phần cứng Vật lý (Hardware Comms)
Tạo class Reader (ví dụ: `SiemensReader.cs` hoặc `ModbusReader.cs`).
1. Sử dụng thư viện gốc như `EasyModbus` hoặc `S7.Net`.
2. Tạo hàm `ConnectAsync(ip, port)`.
3. Tạo hàm `ReadRawDataAsync(address, count)` trả về nguyên thủy `int[]` hoặc `byte[]`.
> ⚠️ **Quy tắc Vàng:** KHÔNG CHỨA LOGIC ÉP KIỂU TẠI ĐÂY (không nhân chia cộng trừ). Cái nhiệt kế báo về 855 thì trả về đúng số 855.

### Lớp 2: Xử lý Nghiệp Vụ - Business Logic (Bắt buộc Dùng Interface)
Tạo Interface (ví dụ: `IOvenService`) và class thực thi (ví dụ: `OvenService.cs`).
1. Inject `Reader` của Lớp 1 vào Service này.
2. Tạo các hàm lấy Data có ý nghĩa: `GetCurrentTemperature()`, `GetMachineStatus()`.
3. Phép tóa quy đổi (ví dụ: `rawTemp / 10.0`) diễn ra ở đây.
4. Trả về là Object (DTO) chứa ngữ cảnh: `{ LotNo: "ME_123", Temp: 85.5, Status: "Running"}`.
> Phải dùng Interface để Web API và Background Service lấy ra dùng.

### Lớp 3: Luồng Công nhân Cần mẫn (Background Background Worker)
Tạo class (ví dụ `OvenDataFetcher.cs`) kế thừa `BackgroundService`.
1. Chạy trong vòng lặp vô tận (Vd: Task.Delay(5000) = 5s lấy 1 lần).
2. Gọi Lớp 2 (`OvenService.GetProcessedData()`)
3. Lưu cục Data đó vào Context Entity Framework (SQL Server / PostgreSQL).
4. Gọi `SignalR Hub` bắn Broadcast (Phát loa) cục Data đó vảo Mạng Local.

## Lưu ý Khi Render Màn hình
- Cấm Polling HTTP Get từ Frontend.
- Web Blazor/NextJs/React bắt buộc phải `Subscribe` vào SignalR/WebSocket.
- Khớp đúng tên sự kiện (vd: `.On("ReceiveOvenData")`). Dữ liệu tới đâu là Update vào biểu đồ ngay tới đó.
