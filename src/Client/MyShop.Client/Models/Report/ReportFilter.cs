namespace Server.Handler.Report;

public class ReportFilter
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    // GroupType: 1-Ngày, 2-Tuần, 3-Tháng, 4-Năm
    public int GroupType { get; set; } = 1;
}
