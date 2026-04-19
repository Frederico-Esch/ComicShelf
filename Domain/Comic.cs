namespace Domain;

public record Comic
{
    public Guid Id { get; set; }
    public int? Volume { get; set; }
    public string? Special { get; set; } //For special editions (like annuals) Volume doesn't make sense
    public bool OwnIt { get; set; }
}