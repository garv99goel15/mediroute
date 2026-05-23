using FluentValidation;
using MediRoute.API.DTOs;

namespace MediRoute.API.Validators;

public class HospitalSearchDtoValidator : AbstractValidator<HospitalSearchDto>
{
    public HospitalSearchDtoValidator()
    {
        RuleFor(x => x.Lat).InclusiveBetween(-90, 90);
        RuleFor(x => x.Lng).InclusiveBetween(-180, 180);
        RuleFor(x => x.Radius).GreaterThan(0).LessThanOrEqualTo(200);
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(4).MaximumLength(200);
    }
}

public class RefreshRequestDtoValidator : AbstractValidator<RefreshRequestDto>
{
    public RefreshRequestDtoValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public class BedStatusUpdateDtoValidator : AbstractValidator<BedStatusUpdateDto>
{
    public BedStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class BulkBedUpdateDtoValidator : AbstractValidator<BulkBedUpdateDto>
{
    public BulkBedUpdateDtoValidator()
    {
        RuleFor(x => x.Updates).NotEmpty().Must(u => u.Count <= 200)
            .WithMessage("Cannot update more than 200 beds at once.");
        RuleForEach(x => x.Updates).ChildRules(item =>
        {
            item.RuleFor(i => i.BedId).GreaterThan(0);
            item.RuleFor(i => i.Status).IsInEnum();
        });
    }
}

public class DoctorAvailabilityUpdateDtoValidator : AbstractValidator<DoctorAvailabilityUpdateDto>
{
    public DoctorAvailabilityUpdateDtoValidator()
    {
        RuleFor(x => x.NextAvailableAt)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-1))
            .When(x => x.NextAvailableAt.HasValue);
    }
}

public class AmbulanceStatusUpdateDtoValidator : AbstractValidator<AmbulanceStatusUpdateDto>
{
    public AmbulanceStatusUpdateDtoValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}

public class OtStatusUpdateDtoValidator : AbstractValidator<OtStatusUpdateDto>
{
    public OtStatusUpdateDtoValidator()
    {
        RuleFor(x => x.AvailableRooms).GreaterThanOrEqualTo(0).When(x => x.AvailableRooms.HasValue);
    }
}
