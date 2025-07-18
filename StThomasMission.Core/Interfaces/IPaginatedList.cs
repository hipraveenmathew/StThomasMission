using System.Collections.Generic;

namespace StThomasMission.Core.Interfaces
{
    // Note: This interface only exposes the metadata and the collection itself.
    // The list of items is exposed via IEnumerable<T>.
    public interface IPaginatedList<T> : IEnumerable<T>, IPaginationInfo
    {
       
    }
}