using MediatR;
using log4net;

using Subasta.Application.DTOs;

using Subasta.Infrastructure.Interfaces;
using MongoDB.Bson;

namespace Subasta.Infrastructure.Queries.QueryHandlers
{
    public class GetAuctionByIdQueryHandler : IRequestHandler<GetAuctionByIdQuery, GetAuctionDto>
    {
        private readonly IReadAuctionRepository _auctionReadRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(GetAuctionByIdQueryHandler));

        public GetAuctionByIdQueryHandler(IReadAuctionRepository auctionReadRepository)
        {
            _auctionReadRepository = auctionReadRepository ?? throw new ArgumentNullException(nameof(auctionReadRepository));
        }

        public async Task<GetAuctionDto> Handle(GetAuctionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var auction = await _auctionReadRepository.GetAuctionById(request.Id);

                if (auction == null)
                {
                    throw new KeyNotFoundException("Subasta no encontrada.");
                }

                var auctionDto = new GetAuctionDto
                {
                    Id = auction["_id"].AsString,
                    UserId = auction["userId"].AsString,
                    ProductId = auction["productId"].AsString,
                    ProductQuantity = auction["productQuantity"].AsInt32,
                    Name = auction["name"].AsString,
                    Description = auction["description"].AsString,
                    Status = auction["status"].AsString,
                    BasePrice = auction["basePrice"].AsDecimal,
                    Duration = auction["duration"].AsInt32,
                    MinimumIncrease = auction["minimumIncrease"].AsDecimal,
                    ReservePrice = auction["reservePrice"].AsDecimal,
                    StartDate = auction["startDate"].AsBsonDateTime.ToUniversalTime(),
                };

                return auctionDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
