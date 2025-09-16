using DomainService.Application.Dto;
using DomainService.Application.Dto.Response;
using DomainService.Application.Queries;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace DomainService.Application.Handlers;

public class DomainQueryHandlers : IWolverineHandler
{
    public static async Task<DomainDto?> Handle(GetDomainByIdQuery query, AppDbContext context)
    {
        return await context.Domains
            .Where(d => d.Id == query.DomainId && !d.IsDeleted)
            .Select(d => new DomainDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                UserId = d.UserId,
                IsVerified = d.IsVerified,
                IsDeleted = d.IsDeleted,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public static async Task<DomainDto?> Handle(GetDomainByIdAndUserQuery query, AppDbContext context)
    {
        return await context.Domains
            .Where(d => d.Id == query.DomainId && d.UserId == query.UserId && !d.IsDeleted)
            .Select(d => new DomainDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                UserId = d.UserId,
                IsVerified = d.IsVerified,
                IsDeleted = d.IsDeleted,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public static async Task<PagedResult<DomainSummaryDto>> Handle(GetDomainsByUserQuery query, AppDbContext context)
    {
        var totalQuery = context.Domains
            .Where(d => d.UserId == query.UserId && !d.IsDeleted);

        var total = await totalQuery.CountAsync();

        var domains = await totalQuery
            .OrderBy(d => d.DomainName)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(d => new DomainSummaryDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                IsVerified = d.IsVerified,
                CreatedAt = d.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<DomainSummaryDto>
        {
            Items = domains,
            Total = total,
            Skip = query.Skip,
            Take = query.Take
        };
    }

    public static async Task<PagedResult<DomainSummaryDto>> Handle(GetAllDomainsQuery query, AppDbContext context)
    {
        var totalQuery = context.Domains.Where(d => !d.IsDeleted);
        var total = await totalQuery.CountAsync();

        var domains = await totalQuery
            .OrderBy(d => d.UserId)
            .ThenBy(d => d.DomainName)
            .Skip(query.Skip)
            .Take(query.Take)
            .Select(d => new DomainSummaryDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                IsVerified = d.IsVerified,
                CreatedAt = d.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<DomainSummaryDto>
        {
            Items = domains,
            Total = total,
            Skip = query.Skip,
            Take = query.Take
        };
    }

    public static async Task<IEnumerable<DomainSummaryDto>> Handle(GetVerifiedDomainsQuery query, AppDbContext context)
    {
        return await context.Domains
            .Where(d => d.UserId == query.UserId && d.IsVerified && !d.IsDeleted)
            .OrderBy(d => d.DomainName)
            .Select(d => new DomainSummaryDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                IsVerified = d.IsVerified,
                CreatedAt = d.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public static async Task<IEnumerable<DomainSummaryDto>> Handle(GetDeletedDomainsQuery query, AppDbContext context)
    {
        return await context.Domains
            .Where(d => d.UserId == query.UserId && d.IsDeleted)
            .OrderByDescending(d => d.UpdatedAt)
            .Select(d => new DomainSummaryDto
            {
                Id = d.Id,
                DomainName = d.DomainName,
                IsVerified = d.IsVerified,
                CreatedAt = d.CreatedAt
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public static async Task<bool> Handle(CheckDomainExistsQuery query, AppDbContext context)
    {
        return await context.Domains
            .AnyAsync(d => d.DomainName == query.DomainName
                           && d.UserId == query.UserId
                           && !d.IsDeleted);
    }
}