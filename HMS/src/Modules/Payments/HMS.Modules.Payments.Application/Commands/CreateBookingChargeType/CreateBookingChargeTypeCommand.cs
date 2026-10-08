using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Application.Common;
using HMS.Modules.Payments.Application.DTOs;
using HMS.Modules.Payments.Application.Queries.GetBookingChargeTypes;
using HMS.Modules.Payments.Domain.Enums;
using MediatR;

namespace HMS.Modules.Payments.Application.Commands.CreateBookingChargeType;

public sealed record CreateBookingChargeTypeCommand(
    Guid PropertyUid,
    string Code,
    string Name,
    BookingChargeCategory Category,
    bool IsTaxable,
    decimal? DefaultPrice,
    bool IsActive,
    string ActorSubject,
    bool IsPlatformAdmin) : IRequest<PaymentResult<BookingChargeTypeDto>>;

public sealed class CreateBookingChargeTypeCommandHandler(
    IPaymentPropertyAccess propertyAccess,
    IBookingChargeTypeRepository chargeTypeRepository)
    : IRequestHandler<CreateBookingChargeTypeCommand, PaymentResult<BookingChargeTypeDto>>
{
    public async Task<PaymentResult<BookingChargeTypeDto>> Handle(
        CreateBookingChargeTypeCommand request,
        CancellationToken cancellationToken)
    {
        var property = await ChargeTypeGuard.LoadProperty(
            propertyAccess, request.PropertyUid, request.ActorSubject, request.IsPlatformAdmin, true, cancellationToken);
        if (property.Error is not null)
            return PaymentResult<BookingChargeTypeDto>.From(property.Error);

        var validation = BookingChargeTypeRules.Validate(request.Code, request.Name, request.Category, request.DefaultPrice);
        if (validation is not null)
            return PaymentResult<BookingChargeTypeDto>.Validation(validation);

        var code = request.Code.Trim().ToUpperInvariant();
        if (await chargeTypeRepository.CodeExistsAsync(property.Value!.Id, code, null, cancellationToken))
            return PaymentResult<BookingChargeTypeDto>.Conflict("This charge type code already exists for the property.");

        var uid = await chargeTypeRepository.InsertAsync(
            property.Value.OrganizationId,
            property.Value.Id,
            code,
            request.Name.Trim(),
            request.Category.ToDatabaseValue(),
            request.IsTaxable,
            request.DefaultPrice,
            request.IsActive,
            request.ActorSubject,
            cancellationToken);

        var detail = await chargeTypeRepository.GetDetailAsync(uid, property.Value.Id, property.Value.Uid, cancellationToken);
        return detail is null
            ? PaymentResult<BookingChargeTypeDto>.NotFound("Charge type was not found.")
            : PaymentResult<BookingChargeTypeDto>.Success(detail);
    }
}

internal static class BookingChargeTypeRules
{
    public static string? Validate(string code, string name, BookingChargeCategory category, decimal? defaultPrice)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length > 30)
            return "Code is required and cannot exceed 30 characters.";

        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
            return "Name is required and cannot exceed 100 characters.";

        if (!Enum.IsDefined(category))
            return "Category must be Food, Cooking, Laundry, Beverage, Transport, Activity, Damage, or Other.";

        if (defaultPrice is < 0)
            return "Default price cannot be negative.";

        return null;
    }
}
