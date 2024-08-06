using AuctionService.Entities;
using Contracts;
using MassTransit;

namespace AuctionService.Consumers
{
    public class AuctionCreatedFaultConsumer:IConsumer<Fault<AuctionCreated>>
    {

        public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
        {
            Console.WriteLine("--->consuming faulty operation");

            var exception= context.Message.Exceptions.First();
            //IN case exception thrown from coumer we are cathcing exception here and re publishing value 
            if (exception.ExceptionType == "System.ArgumentException")
            {
                context.Message.Message.Model = "FooBar";
                await context.Publish(context.Message.Message);
            }
            else
            {
                Console.WriteLine("Not and argument exception- update dashboard somewhere else");
            }

        }
    }
}
