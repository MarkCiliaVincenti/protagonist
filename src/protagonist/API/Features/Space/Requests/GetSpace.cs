using DLCS.Model.Spaces;
using MediatR;

namespace API.Features.Space.Requests;

/// <summary>
/// Get details of specified space for customer
/// </summary>
public class GetSpace : IRequest<DLCS.Model.Spaces.Space?>
{
    public GetSpace(int customerId, int spaceId)
    {
        CustomerId = customerId;
        SpaceId = spaceId;
    }
    
    public int CustomerId { get; }
    public int SpaceId { get; }
}

public class GetSpaceHandler : IRequestHandler<GetSpace, DLCS.Model.Spaces.Space?>
{
    private readonly ISpaceRepository spaceRepository;

    public GetSpaceHandler(ISpaceRepository spaceRepository)
    {
        this.spaceRepository = spaceRepository;
    }
    
    public async Task<DLCS.Model.Spaces.Space?> Handle(GetSpace request, CancellationToken cancellationToken)
    {
        var space = await spaceRepository.GetSpace(
            request.CustomerId, request.SpaceId, noCache: true, cancellationToken: cancellationToken);
        return space;
    }
}