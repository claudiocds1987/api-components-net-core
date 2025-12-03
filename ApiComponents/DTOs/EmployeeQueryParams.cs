using System;

namespace ApiComponents.DTOs
{
    public class EmployeeQueryParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1;

        private int _pageSize = 25;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }

        public int? Id { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }

        public DateTime? BirthDate { get; set; }

        public int? CountryId { get; set; }

        public int? GenderId { get; set; }
        public int? PositionId { get; set; }
        public bool? Active { get; set; }

        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; } // "asc" o "desc"
    }
}
