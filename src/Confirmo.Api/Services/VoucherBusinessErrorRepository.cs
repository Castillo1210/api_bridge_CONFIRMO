using Confirmo.Api.Data;
using Microsoft.EntityFrameworkCore;
using Confirmo.Api.Models.Entities;

namespace Confirmo.Api.Services;

public class VoucherBusinessErrorRepository : IVoucherBusinessErrorRepository
{
    private readonly AppDbContext _context;

    public VoucherBusinessErrorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<VoucherBusinessError>> GetByIdsAsync(List<string> ids)
    {
        if (ids == null || ids.Count == 0) return new List<VoucherBusinessError>();

        var guids = ids.Select(Guid.Parse).ToList();

        return await _context.VoucherBusinessErrors.Where(e => guids.Contains(e.Id) && e.IsActive).ToListAsync();
    }

    public async Task<VoucherBusinessError?> GetByCodeAsync(string errorCode)
    {
        return await _context.VoucherBusinessErrors.FirstOrDefaultAsync(e => e.ErrorCode == errorCode && e.IsActive);
    }
}