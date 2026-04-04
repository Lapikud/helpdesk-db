namespace Base.Contracts;

public interface IDomainMeta
{
    public string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }

    // hidden information about the record in db
    public string? SysNotes { get; set; }
}