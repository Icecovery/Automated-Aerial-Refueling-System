using System.Collections.Generic;
using System.Linq;

    public static class PartLoaderExtensions
    {
        public static IEnumerable<AvailablePart> AARSParts(this List<AvailablePart> parts)
        {
            return (from avPart in parts.Where(p => p.partPrefab)
                    let AARSModule = avPart.partPrefab.GetComponent<IsAARSpart>()
                    where AARSModule != null
                    select avPart
                     ).ToList();
        }
    }

