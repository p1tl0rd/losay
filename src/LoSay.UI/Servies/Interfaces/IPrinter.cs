namespace LoSay.Servies.Interfaces
{
	public interface IPrinter
	{
		Task PrintPdfWithGhostscript(string pdfFilePath, string printerName);
		Task PrintPdf(string pdfFilePath, string printerName);
		Task PrintPng(string pdfFilePath, string printerName);
		Task<List<string>> GetInstalledPrinters();
		Task LogInstalledPrinters();
	}
}
