using System.Collections.Generic;
using System.Threading.Tasks;
using Confirmo.Api.Models.Entities;

namespace Confirmo.Api.Services;

public interface IVoucherBusinessErrorRepository
{
    Task<List<VoucherBusinessError>> GetByIdsAsync(List<string> ids);
    Task<VoucherBusinessError?> GetByCodeAsync(string errorCode);
}