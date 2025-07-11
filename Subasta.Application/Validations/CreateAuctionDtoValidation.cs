using FluentValidation;

using Subasta.Application.DTOs;

namespace Subasta.Application.Validations
{

    public class CreateAuctionDtoValidation : AbstractValidator<CreateAuctionDto>
    {
        public CreateAuctionDtoValidation()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("El ID del usuario es obligatorio.");

            RuleFor(x => x.ProductId)
                .NotEmpty().WithMessage("El ID del producto es obligatorio.");

            RuleFor(x => x.ProductQuantity)
                .NotEmpty().WithMessage("La cantidad del producto no puede ser nula")
                .GreaterThanOrEqualTo(1);

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("La Descripcion es obligatoria.");

            RuleFor(x => x.BasePrice)
                .NotEmpty().WithMessage("El precio base es obligatorio.");

            RuleFor(x => x.Duration)
                .NotEmpty().WithMessage("La duracion es obligatoria.");

            RuleFor(x => x.MinimumIncrease)
                .NotEmpty().WithMessage("El incremento minimo es obligatorio.");

            RuleFor(x => x.ReservePrice)
                .NotEmpty().WithMessage("El precio de reserva es obligatorio.");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("La fecha de inicio es obligatoria.")
                .GreaterThan(DateTime.UtcNow);

        }
    }
}
