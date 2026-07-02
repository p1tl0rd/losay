# Quy tắc phát triển hệ thống IoT/SCADA (Trích xuất từ LoSay Framework)

Khi được yêu cầu xây dựng một hệ thống IoT/SCADA trên nền tảng Web (ví dụ: Đóng gói, Bơm nước, Quan trắc môi trường), Agent BẮT BUỘC tuân thủ các quy tắc kiến trúc sau:

## 1. Kiến trúc 3 Lớp Bắt Buộc (3-Tier SCADA Architecture)
Tuyệt đối không gộp chung logic đọc thiết bị vào Web API hoặc UI Component. Phải tách bạch thành:
- **Tầng 1: Hardware Reader (Ví dụ: `ModbusReader.cs`, `MqttClient.cs`)**
  - Nhiệm vụ duy nhất: Giao tiếp Protocol (Modbus, OPC UA, MQTT).
  - Đầu ra: Mảng byte, mảng int thô (Raw Data). Không chứa logic chuyển đổi.
- **Tầng 2: Data Service (Ví dụ: `PumpService.cs`, `OvenService.cs`)**
  - Nhiệm vụ: Ép kiểu dữ liệu (chia 10, nhân hệ số), gắn tên thiết bị, quản lý trạng thái máy (Đang chạy/Dừng).
  - Đầu ra: Object DTO (Data Transfer Object) đã sạch sẽ và mang ý nghĩa Business (VD: `{ Temp: 85.5, Status: "Running" }`).
- **Tầng 3: Background Worker (Ví dụ: `DataAcquisitionWorker.cs`)**
  - Implement `IHostedService` hoặc `BackgroundService`.
  - Nhiệm vụ: Chạy vòng lặp vô tận (Timer) -> Gọi Tầng 2 -> Lưu Database -> Bắn SignalR.

## 2. Lưu trữ dữ liệu (Database Design)
- **Không Hardcode Cột (No Hardcoded Columns):** KHÔNG tạo bảng có các cột `Value1`, `Value2`... `ValueN` cố định.
- **Khuyến nghị:** 
  - Sử dụng cấu trúc linh hoạt theo Entity-Attribute-Value (EAV): `(MachineId, SensorName, Value, Timestamp)`.
  - Hoặc lưu Payload dưới dạng JSON: `(MachineId, PayloadJson, Timestamp)` để dễ mở rộng khi thêm bớt cảm biến.

## 3. Thời gian thực (Real-time Communication)
- **Cấm Fetch Polling:** Web UI tuyệt đối KHÔNG đực dùng JavaScript `setInterval` để fetch HTTP API liên tục.
- **Bắt Buộc:** Mọi cập nhật dữ liệu nóng phải được đẩy từ Server xuống thông qua WebSockets (SignalR, Socket.io).
- **Nhóm (Groups):** Tận dụng tính năng Group thiết bị của SignalR (e.g. `Clients.Group("Machine_1")`) để tránh gửi rác data cho những User đang xem màn hình máy khác.

## 4. An toàn luồng (Thread & Task Safety)
- Vì Background Worker chạy 24/7 độc lập với Web Request, việc truy cập Database bằng Entity Framework (DbContext) bắt buộc phải tuân thủ nguyên tắc tạo Scope mới cơ hội (`IServiceProvider.CreateScope()`), vì DbContext không Thread-safe.
