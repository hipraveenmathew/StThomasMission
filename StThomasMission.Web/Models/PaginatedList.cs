using System;
using System.Collections.Generic;
using System.Linq;

namespace StThomasMission.Web.Models
{
    public class PaginatedList<T> : IEnumerable<T>
    {
        private readonly List<T> _items;
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItems { get; private set; }

        public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            _items = items.ToList();
            PageIndex = pageIndex;
            TotalItems = count;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => _items.Count; // Added for checking if items exist
    }
}