using ChipoBackend.Application.Features.Products.Commands.AddProductImage;
using ChipoBackend.Application.Features.Products.Commands.AddProductVariant;
using ChipoBackend.Application.Features.Products.Commands.ChangeProductStatus;
using ChipoBackend.Application.Features.Products.Commands.ConfigureDecant;
using ChipoBackend.Application.Features.Products.Commands.RemoveProductImage;
using ChipoBackend.Application.Features.Products.Commands.CreateProduct;
using ChipoBackend.Application.Features.Products.Commands.UpdateProduct;
using ChipoBackend.Application.Features.Products.Commands.UpdateProductVariant;
using ChipoBackend.Application.Features.Products.DTOs;
using ChipoBackend.Application.Features.Products.Queries.GetProductById;
using ChipoBackend.Application.Features.Products.Queries.GetProducts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChipoBackend.API.Controllers;

[Route("api/[controller]")]
public class ProductsController : BaseApiController
{
    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>Listado paginado de productos (público para tienda, admin para back-office)</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var result = await Mediator.Send(new GetProductsQuery(page, pageSize, categoryId, search, status), ct);
        return Ok(result);
    }

    /// <summary>Detalle completo de un producto por ID (con variantes e imágenes)</summary>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await Mediator.Send(new GetProductByIdQuery(id), ct);
        return Ok(result);
    }

    // ── Commands ─────────────────────────────────────────────────────────────

    /// <summary>Crear un producto nuevo con sus variantes iniciales</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command, CancellationToken ct)
    {
        var id = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    /// <summary>Actualizar datos generales de un producto</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command, CancellationToken ct)
    {
        if (id != command.Id) return BadRequest("El ID de la ruta no coincide con el del body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Cambiar el estado de un producto: Draft | Published | Discontinued</summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ProductChangeStatusRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ChangeProductStatusCommand(id, request.Status), ct);
        return NoContent();
    }

    // ── Variantes ────────────────────────────────────────────────────────────

    /// <summary>Agregar una variante a un producto existente</summary>
    [HttpPost("{productId:guid}/variants")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> AddVariant(Guid productId, [FromBody] AddProductVariantCommand command, CancellationToken ct)
    {
        if (productId != command.ProductId) return BadRequest("ProductId de la ruta no coincide con el del body.");
        var variantId = await Mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = productId }, new { variantId });
    }

    /// <summary>Actualizar precio o estado de una variante</summary>
    [HttpPut("{productId:guid}/variants/{variantId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> UpdateVariant(Guid productId, Guid variantId,
        [FromBody] UpdateProductVariantCommand command, CancellationToken ct)
    {
        if (productId != command.ProductId || variantId != command.VariantId)
            return BadRequest("IDs de la ruta no coinciden con el body.");
        await Mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>Configurar un producto como decant (por ml): frasco, stock en ml y reposición</summary>
    [HttpPut("{productId:guid}/decant")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> ConfigureDecant(Guid productId, [FromBody] ProductDecantRequest request, CancellationToken ct)
    {
        await Mediator.Send(new ConfigureDecantCommand(productId, request.BottleCost, request.BottleMl, request.StockMl, request.ReorderMl), ct);
        return NoContent();
    }

    // ── Imágenes ─────────────────────────────────────────────────────────────

    /// <summary>Agregar una imagen (por URL; admite links de Google Drive) a un producto</summary>
    [HttpPost("{productId:guid}/images")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> AddImage(Guid productId, [FromBody] ProductAddImageRequest request, CancellationToken ct)
    {
        var imageId = await Mediator.Send(new AddProductImageCommand(productId, request.Url, request.AltText), ct);
        return CreatedAtAction(nameof(GetById), new { id = productId }, new { imageId });
    }

    /// <summary>Eliminar una imagen de un producto</summary>
    [HttpDelete("{productId:guid}/images/{imageId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Supervisor")]
    public async Task<IActionResult> RemoveImage(Guid productId, Guid imageId, CancellationToken ct)
    {
        await Mediator.Send(new RemoveProductImageCommand(productId, imageId), ct);
        return NoContent();
    }
}

public record ProductChangeStatusRequest(string Status);
public record ProductAddImageRequest(string Url, string? AltText = null);
public record ProductDecantRequest(decimal? BottleCost, int? BottleMl, int StockMl, int ReorderMl);
