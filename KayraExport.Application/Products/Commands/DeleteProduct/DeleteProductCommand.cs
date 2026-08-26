using KayraExport.Application.Interfaces;
using MediatR;

namespace KayraExport.Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(Guid Id)
    : IRequest<bool>;

public sealed class DeleteProductCommandHandler
    : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (product is null)
        {
            throw new KeyNotFoundException(
                $"Product with ID '{request.Id}' was not found.");
        }

        _productRepository.Delete(product);
        await _productRepository.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync(
            ProductCacheKeys.All,
            cancellationToken);

        return true;
    }
}