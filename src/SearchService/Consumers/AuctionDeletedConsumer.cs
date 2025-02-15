﻿using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;
using static MassTransit.ValidationResultExtensions;

namespace SearchService.Consumers
{
    public class AuctionDeletedConsumer:IConsumer<AuctionDeleted>
    {
        public async Task Consume(ConsumeContext<AuctionDeleted> context)
        {
            Console.WriteLine("---->Consuming auction deleted" + context.Message.Id);
            var result = await DB.DeleteAsync<Item>(context.Message.Id);
                if (!result.IsAcknowledged)
            {
                throw new MessageException(typeof(AuctionUpdated), "Problem updating mongo db");
            }
        }
    }
}
