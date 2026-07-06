using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Application.PaymentMethods.GetAll;

internal sealed class GetAllPaymentMethodsQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetAllPaymentMethodsQuery, List<PaymentMethodResponse>>
{
    public async Task<Result<List<PaymentMethodResponse>>> Handle(GetAllPaymentMethodsQuery query, CancellationToken cancellationToken)
    {
        List<PaymentMethodResponse> paymentMethods = await context.PaymentMethods
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PaymentMethodResponse
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToListAsync(cancellationToken);

        return paymentMethods;
    }
}
