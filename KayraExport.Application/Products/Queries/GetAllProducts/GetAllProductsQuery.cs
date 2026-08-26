using KayraExport.Application.Interfaces;
using KayraExport.Application.Products.Dtos;
using MediatR;

namespace KayraExport.Application.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery
    : IRequest<IReadOnlyList<ProductDto>>;

public sealed class GetAllProductsQueryHandler
    : IRequestHandler<GetAllProductsQuery, IReadOnlyList<ProductDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    public GetAllProductsQueryHandler(
        IProductRepository productRepository,
        ICacheService cacheService)
    {
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<IReadOnlyList<ProductDto>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var cachedProducts = await _cacheService.GetAsync<List<ProductDto>>(
            ProductCacheKeys.All,
            cancellationToken);

        if (cachedProducts is not null)
        {
            return cachedProducts;
        }

        var products = await _productRepository.GetAllAsync(
            cancellationToken);

        var productDtos = products
            .Select(ProductDto.FromEntity)
            .ToList();

        await _cacheService.SetAsync(
            ProductCacheKeys.All,
            productDtos,
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return productDtos;
    }
}