namespace StThomasMission.Core.Interfaces
{
    /// <summary>
    /// A non-generic interface containing only the properties needed for pagination controls.
    /// </summary>
    public interface IPaginationInfo
    {
        int PageIndex { get; }
        int TotalPages { get; }
        int TotalCount { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}