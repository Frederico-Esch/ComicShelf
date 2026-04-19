using System.Collections.Immutable;

namespace Domain;

public record Collection
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public HashSet<string> Tags { get; set; } = [];
    public byte[]? Cover { get; set; } = [];
    public virtual ICollection<Volume> Volumes { get; set; } = [];

    public Collection() { }
    public Collection(string name, HashSet<string> tags, byte[]? cover)
    {
        Id = Guid.NewGuid();
        Name = name;
        Tags = tags;
        Cover = cover;
    }
}
