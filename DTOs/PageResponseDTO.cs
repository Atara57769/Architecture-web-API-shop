using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTOs
{
    public record PageResponseDTO<T>
    (
        List<T> Data ,
        int TotalItems ,
        int CurrentPage ,
        int PageSize ,
        bool HasPreviousPage ,
        bool HasNextPage 
    );
}
