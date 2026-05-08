namespace Server.Handler.BR;

public class BackupRestore
{
    public string Name { get; set; }
    public DateTime UpdateAt { get; set; }
    public long Size { get; set; } = 0;
}
