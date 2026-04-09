using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs;
using OrderApi.Services;

namespace OrderApi.Controllers;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventory;

    public InventoryController(IInventoryService inventory)
    {
        _inventory = inventory;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<InventoryItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _inventory.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpPost("seed")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Seed([FromBody] SeedInventoryRequestDto request, CancellationToken ct)
    {
        var count = await _inventory.SeedAsync(request, ct);
        return Ok(new { seeded = count });
    }
}
