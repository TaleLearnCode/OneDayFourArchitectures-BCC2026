using Microsoft.AspNetCore.Mvc;
using TorettoMotors.BLL.Models;
using TorettoMotors.BLL.Services.Interfaces;

namespace TorettoMotors.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;

    public InvoicesController(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
    {
        var invoices = await _invoiceService.GetAllInvoicesAsync();
        return Ok(invoices);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
            return NotFound();
        return Ok(invoice);
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByCustomerId(int customerId)
    {
        var invoices = await _invoiceService.GetInvoicesByCustomerIdAsync(customerId);
        return Ok(invoices);
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(InvoiceDto invoice)
    {
        try
        {
            var created = await _invoiceService.CreateInvoiceAsync(invoice);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, InvoiceDto invoice)
    {
        if (id != invoice.Id)
            return BadRequest();

        try
        {
            var updated = await _invoiceService.UpdateInvoiceAsync(invoice);
            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _invoiceService.DeleteInvoiceAsync(id);
        if (!result)
            return NotFound();
        return NoContent();
    }
}
