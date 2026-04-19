using MediatR;

namespace YourAPP_Services.Features.TestFeat
{
    public class CreateTestFeatCommandHandler(YourAppDbContext) : IRequestHandler<CreateTestFeatCommand, int>
    {
        public Task<int> Handle(CreateTestFeatCommand request, CancellationToken cancellationToken)
        {
            
        }
    }
}
