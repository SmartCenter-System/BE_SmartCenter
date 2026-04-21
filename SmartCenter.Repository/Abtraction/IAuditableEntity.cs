namespace SmartCenter.Repository.Abtraction;

public interface IAuditableEntity
{
    DateTimeOffset CreateAt { get; set; }
    DateTimeOffset? UpdateAt { get; set; }
}   