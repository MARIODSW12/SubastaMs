
using MediatR;

using Subasta.Application.DTOs;

namespace Subasta.Application.Commands
{

    public class CreatePrizeClaimCommand : IRequest<String>
    {
        public CreatePrizeClaimDto ClaimPrizeDto { get; }

        public CreatePrizeClaimCommand(CreatePrizeClaimDto claimPrizeDto)
        {
            ClaimPrizeDto = claimPrizeDto ?? throw new ArgumentNullException(nameof(claimPrizeDto));
        }
    }
}
