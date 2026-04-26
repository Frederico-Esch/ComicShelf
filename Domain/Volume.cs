using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class Volume
    {
        public Guid Id { get; set; }
        public Guid CollectionId { get; set; }
        public int? Number { get; set; }
        public string? SpecialEdition { get; set; }
        public string? Details { get; set; }
        public bool IsOwned { get; set; } = false;
        // Data de publicacao

        public string Name {
            get => Number is { } number ? $"Volume #{number}"
                    : SpecialEdition is { } specialEdition ? $"{SpecialEdition}"
                    : "Unknown";
        }
        public bool HasDetail
        {
            get => !string.IsNullOrWhiteSpace(Details);
        }

        public virtual Collection Collection { get; set; } = null!;

        public Volume() {
            Id = Guid.NewGuid();
            CollectionId = Guid.Empty;
        }

        public Volume(Guid collectionId)
        {
            Id = Guid.NewGuid();
            CollectionId = collectionId;
        }
    }
}
