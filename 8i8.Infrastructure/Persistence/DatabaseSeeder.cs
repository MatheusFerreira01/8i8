using ConnectAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConnectAi.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Tags.AnyAsync()) return;

        var tags = new[]
        {
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Financeiro",
                Slug = "financeiro",
                Description = "Boletos, pagamentos e cobranças",
                CreatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Suporte Técnico",
                Slug = "suporte-tecnico",
                Description = "Problemas técnicos e suporte ao produto",
                CreatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Comercial",
                Slug = "comercial",
                Description = "Vendas, propostas e negociações",
                CreatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "RH",
                Slug = "rh",
                Description = "Recursos humanos, contratos e benefícios",
                CreatedAt = DateTime.UtcNow
            },
            new Tag
            {
                Id = Guid.NewGuid(),
                Name = "Atendimento Geral",
                Slug = "atendimento-geral",
                Description = "Dúvidas gerais e atendimento ao cliente",
                CreatedAt = DateTime.UtcNow
            },
        };

        db.Tags.AddRange(tags);
        await db.SaveChangesAsync();
    }
}
