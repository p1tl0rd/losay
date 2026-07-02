using System.Diagnostics;
using System.Drawing;
using System.Management;
using LoSay.Servies.Interfaces;

namespace LoSay.Servies.Implementations
{
	public class PrintDocument : IPrinter
	{
		private readonly IWebHostEnvironment _env;
		private readonly ILogger<PrintDocument> _logger;

		public PrintDocument(IWebHostEnvironment env, ILogger<PrintDocument> logger)
		{
			if (env == null)
			{
				throw new ArgumentNullException(nameof(env), "IWebHostEnvironment is not injected properly.");
			}

			_env = env;
			_logger = logger;
		}

		public async Task<List<string>> GetInstalledPrinters()
		{
			var printers = new List<string>();
			var query = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

			foreach (ManagementObject printer in query.Get())
			{
				string name = printer["Name"]?.ToString();
				string port = printer["PortName"]?.ToString();

				if (!string.IsNullOrEmpty(name))
				{
					// Th? thêm c?ng logic n?u PortName là IP
					if (!string.IsNullOrEmpty(port))
					{
						printers.Add($"{name} on {port}"); // C?ng g?c
					}

					// Thêm c?ng logic
					for (int i = 1; i <= 10; i++)
					{
						printers.Add($"{name} on Ne0{i}:");
					}
				}
			}

			return printers;
		}

		public async Task LogInstalledPrinters()
		{
			var query = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

			foreach (ManagementObject printer in query.Get())
			{
				string name = printer["Name"]?.ToString();
				string port = printer["PortName"]?.ToString();
				string status = printer["PrinterStatus"]?.ToString();

				_logger.LogError($"Printer: {name}, PortName: {port}, Status: {status}");
			}
		}

		public async Task PrintPdf(string pdfFilePath, string printerName)
		{
			//using var document = PdfDocument.Load(pdfFilePath);
			//using var printDoc = document.CreatePrintDocument();
			//printDoc.PrinterSettings.PrinterName = printerName;
			//printDoc.Print();
		}

		public async Task PrintPdfWithGhostscript(string pdfFilePath, string printerName)
		{
			try
			{
				string _ghostscriptPath = @"C:\Program Files\gs\gs10.05.1\bin\gswin64.exe";
				var args = $"-dPrinted -dBATCH -dNOPAUSE -sDEVICE=mswinpr2 -dDEVICEWIDTHPOINTS=595 -dDEVICEHEIGHTPOINTS=842 -sOutputFile=\"%printer%{printerName}\" \"{pdfFilePath}\"";

				var psi = new ProcessStartInfo
				{
					FileName = _ghostscriptPath,
					Arguments = args,
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WindowStyle = ProcessWindowStyle.Hidden
				};
				using (var process = Process.Start(psi))
				{
					process.WaitForExit();

					string output = process.StandardOutput.ReadToEnd();
					string error = process.StandardError.ReadToEnd();

					if (process.ExitCode != 0)
					{
						throw new Exception($"Ghostscript printing failed. Exit code: {process.ExitCode}, Error: {error}");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"PrintPdfWithGhostscript L?i trong kh?i Try-catch bên trong: {ex.ToString()}");
			}

		}

		public async Task PrintPng(string pdfFilePath, string printerName)
		{
			Image image = Image.FromFile(pdfFilePath);
			System.Drawing.Printing.PrintDocument printDoc = new System.Drawing.Printing.PrintDocument();

			try
			{
				string fullPrinterName = $"KONICA MINOLTA Universal V4 PCL";
				printDoc.PrinterSettings.PrinterName = fullPrinterName;
				printDoc.PrintPage += (sender, e) =>
				{
					// L?y dpi c?a máy in
					float printerDpiX = e.Graphics.DpiX;
					float printerDpiY = e.Graphics.DpiY;

					// Tính kích thu?c in b?ng pixel v?i dpi máy in
					float widthInPrinterUnits = image.Width * printerDpiX / image.HorizontalResolution;
					float heightInPrinterUnits = image.Height * printerDpiY / image.VerticalResolution;

					// V? ?nh v?i kích thu?c chu?n, không scale sai t? l?
					e.Graphics.DrawImage(image, 0, 0, widthInPrinterUnits, heightInPrinterUnits);
				};

				printDoc.Print();
			}
			catch (Exception)
			{

				throw;
			}
			
		}
	}
}
