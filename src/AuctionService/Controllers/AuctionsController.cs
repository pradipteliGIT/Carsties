using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using MassTransit.Transports;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IPublishEndpoint _publishEndpoint;

        public AuctionsController(AuctionDbContext context,IMapper mapper, IPublishEndpoint publishEndpoint)
        {
            _context=context;
            _mapper=mapper;
            _publishEndpoint=publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuction(string date)
        {
            var query = _context.Auctions.OrderBy(x => x.Item.Make).AsQueryable();

            //This date is passed from Search service to get latest auctions
            if (!string.IsNullOrEmpty(date)) {
                query = query.Where(x => x.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
            }

            return await query.ProjectTo<AuctionDto>(_mapper.ConfigurationProvider).ToListAsync();          

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuction(Guid id)
        {
            var auction= await _context.Auctions.Include(i=>i.Item).FirstOrDefaultAsync(a => a.Id==id);
            if (auction == null) return NotFound();
            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
        {
            var auction = _mapper.Map<Auction>(auctionDto);
            //TODO: add current user as seller
            auction.Seller = "test"; 

            _context.Auctions.Add(auction);
            var newAuctionDto = _mapper.Map<AuctionDto>(auction);
            await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuctionDto));
            var result =await _context.SaveChangesAsync()>0;
            
            if (!result) BadRequest("Could not save changes to database");
            return CreatedAtAction(nameof(GetAuction), new { auction.Id }, newAuctionDto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AuctionDto>>UpdateAuction(Guid id, UpdateAuctionDto auctionDto)
        {
            var auction= await _context.Auctions.Include(i=>i.Item).FirstOrDefaultAsync(i=>i.Id==id);

            if (auction == null) return NotFound();

            //TODO: Check seller ==username

            auction.Item.Make = auctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = auctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = auctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = auctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = auctionDto.Year ?? auction.Item.Year;

            await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));
            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Problem saving details");
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult>DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);
            if (auction == null) return NotFound();

            //TODO: Check seller==username
            _context.Auctions.Remove(auction);
            await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString()});
            var result = await _context.SaveChangesAsync() > 0;
            if (result) return Ok();
            return BadRequest("Problem deleting auction");
        }
    }
}
