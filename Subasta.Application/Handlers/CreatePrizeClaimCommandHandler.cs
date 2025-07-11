using MediatR;
using log4net;

using Subasta.Application.Commands;

using Subasta.Domain.Repositories;
using Subasta.Domain.ValueObjects;
using Subasta.Domain.Aggregates;
using Subasta.Domain.Events;
using Subasta.Domain.Factories;
using MassTransit;
using MassTransit.Transports;
using Subasta.Application.StateMachine;
using Subasta.Domain.Enums;

namespace Subasta.Application.Handlers
{
    public class CreatePrizeClaimCommandHandler : IRequestHandler<CreatePrizeClaimCommand, string>
    {
        private readonly IPrizeClaimRepository _prizeClaimRepository;
        private readonly IPublishEndpoint _publishEndpoint;

        public CreatePrizeClaimCommandHandler(IPrizeClaimRepository prizeClaimRepository, IPublishEndpoint publishEndpoint)
        {
            _prizeClaimRepository = prizeClaimRepository ?? throw new ArgumentNullException(nameof(prizeClaimRepository));
            _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(_publishEndpoint));
        }

        public async Task<string> Handle(CreatePrizeClaimCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var prizeClaimId = Guid.NewGuid().ToString();
                var prizeClaim = PrizeClaimInfoFactory.Create(
                    new VOPrizeId(prizeClaimId),
                    new VOPrizeUserId(request.ClaimPrizeDto.UserId),
                    new VOId(request.ClaimPrizeDto.AuctionId),
                    new VOClaimDate(DateTime.UtcNow),
                    new VODeliverDirection(request.ClaimPrizeDto.DeliverDirection),
                    new VODeliverMethod(request.ClaimPrizeDto.DeliverMethod.ToString())
                    );

                await _prizeClaimRepository.CreatePrizeClaim(prizeClaim);

                var prizeClaimCreatedEvent = new PrizeClaimCreatedEvent(
                    prizeClaim.PrizeUserId.Value,
                    prizeClaim.AuctionId.Value,
                    prizeClaim.DeliverDirection.Value,
                    prizeClaim.DeliverMethod.Value,
                    prizeClaim.ClaimedDate.Value,
                    prizeClaim.PrizeId.Value
                );
                await _publishEndpoint.Publish(prizeClaimCreatedEvent);

                return prizeClaim.PrizeId.Value;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}