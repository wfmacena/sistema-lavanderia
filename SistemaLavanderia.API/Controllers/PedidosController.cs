using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaLavanderia.Core.Entities;
using SistemaLavanderia.Infrastructure.Data;

namespace SistemaLavanderia.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
        // Desabilitar validação automática do ModelState
        ModelState.MaxAllowedErrors = int.MaxValue;
    }

    // GET: api/Pedidos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
    {
        return await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Usuario)
            .Include(p => p.ItensPedido)
                .ThenInclude(i => i.Servico)
            .ToListAsync();
    }

    // GET: api/Pedidos/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Pedido>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Usuario)
            .Include(p => p.ItensPedido)
                .ThenInclude(i => i.Servico)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
        {
            return NotFound();
        }

        return pedido;
    }

    // POST: api/Pedidos
    [HttpPost]
    public async Task<ActionResult<Pedido>> PostPedido(Pedido pedido)
    {
        try
        {
            // Log para debug
            Console.WriteLine("=== RECEBENDO PEDIDO ===");
            Console.WriteLine($"ClienteId: {pedido.ClienteId}");
            Console.WriteLine($"UsuarioId: {pedido.UsuarioId}");
            Console.WriteLine($"Itens recebidos: {pedido.ItensPedido?.Count ?? 0}");

            // Criar um novo pedido do zero
            var novoPedido = new Pedido
            {
                ClienteId = pedido.ClienteId,
                UsuarioId = pedido.UsuarioId,
                DataPrevistaEntrega = pedido.DataPrevistaEntrega,
                Status = "Aguardando",
                Observacoes = pedido.Observacoes,
                DataPedido = DateTime.Now,
                ItensPedido = new List<ItemPedido>()
            };

            decimal total = 0;

            // Processar cada item
            if (pedido.ItensPedido != null)
            {
                foreach (var item in pedido.ItensPedido)
                {
                    var servico = await _context.Servicos.FindAsync(item.ServicoId);
                    if (servico == null)
                        return BadRequest($"Serviço ID {item.ServicoId} não encontrado");

                    novoPedido.ItensPedido.Add(new ItemPedido
                    {
                        ServicoId = item.ServicoId,
                        Quantidade = item.Quantidade,
                        PrecoUnitario = servico.PrecoBase
                    });

                    total += item.Quantidade * servico.PrecoBase;
                }
            }

            novoPedido.ValorTotal = total;

            _context.Pedidos.Add(novoPedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = novoPedido.Id }, novoPedido);
        }
        catch (Exception ex)
        {
            return BadRequest($"Erro: {ex.Message}");
        }
    }

    // PUT: api/Pedidos/5/status
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, string status)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }

        // Lista de status válidos
        var statusValidos = new[] { "Aguardando", "Lavando", "Pronto", "Entregue", "Cancelado" };

        if (!statusValidos.Contains(status))
        {
            return BadRequest("Status inválido");
        }

        pedido.Status = status;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Pedidos/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound();
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}