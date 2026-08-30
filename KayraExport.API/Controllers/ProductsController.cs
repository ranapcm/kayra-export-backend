using KayraExport.Application.Products.Commands.CreateProduct;
using KayraExport.Application.Products.Commands.DeleteProduct;
using KayraExport.Application.Products.Commands.UpdateProduct;
using KayraExport.Application.Products.Dtos;
using KayraExport.Application.Products.Queries.GetAllProducts;
using KayraExport.Application.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KayraExport.API.Controllers;

[ApiController]
[Route("api/v1/products")]
public class ProductsController : ControllerBase
{
    private readonly ISender _sender;

    public ProductsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [Authorize(Policy = "ProductRead")]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var products = await _sender.Send(
            new GetAllProductsQuery(),
            cancellationToken);

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ProductRead")]
    public async Task<ActionResult<ProductDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _sender.Send(
            new GetProductByIdQuery(id),
            cancellationToken);

        return Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = "ProductWrite")]
    public async Task<ActionResult<ProductDto>> Create(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var product = await _sender.Send(
            command,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ProductWrite")]
    public async Task<ActionResult<ProductDto>> Update(
        Guid id,
        UpdateProductCommand command,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        var product = await _sender.Send(
            command,
            cancellationToken);

        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ProductDelete")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new DeleteProductCommand(id),
            cancellationToken);

        return NoContent();
    }
}