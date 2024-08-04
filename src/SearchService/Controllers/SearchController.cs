using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>>SearchItems([FromQuery]SearchParams searchParams)
        {
            var query = DB.PagedSearch<Item,Item>();

            if (!string.IsNullOrEmpty(searchParams.SearchTerm))
            {
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            //Orer by
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(i => i.Make)),
                "new" => query.Sort(x => x.Descending(i => i.CreatedAt)),
                _ => query.Sort(x => x.Ascending(i => i.AuctionEnd))
            };

            //Filtering
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                // Below line will give auction which will end in 6 hours from now
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow),
            };

            //Seller
            if (!string.IsNullOrEmpty(searchParams.Seller))
            {
                query = query.Match(x=>x.Seller == searchParams.Seller);
            }

            //Winner
            if (!string.IsNullOrEmpty(searchParams.Winner))
            {
                query = query.Match(x => x.Winner == searchParams.Winner);
            }

            //Pagination
            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);
            var result = await query.ExecuteAsync();

            return Ok(new
            {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
                
        }
    }
}
