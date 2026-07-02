namespace LoSay.Data.Models
{
    public class ProcessedPlcData
    {
        public bool IsSuccess { get; set; }
        public string? LotNo { get; set; }
        public double[] Values { get; set; } = Array.Empty<double>();
    }
}
