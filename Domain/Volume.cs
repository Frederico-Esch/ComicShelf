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
        public bool IsOwned { get; set; } = false;

        public virtual Collection Collection { get; set; } = null!;

        public Volume() { }
        public Volume(Guid collectionId)
        {
            Id = Guid.NewGuid();
            CollectionId = collectionId;
        }
    }
}
