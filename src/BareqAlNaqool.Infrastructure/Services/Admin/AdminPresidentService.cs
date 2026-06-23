using System.Text.Json;
using BareqAlNaqool.Application.DTOs.Admin;
using BareqAlNaqool.Application.Interfaces;
using BareqAlNaqool.Domain.Constants;
using BareqAlNaqool.Domain.Entities;
using BareqAlNaqool.Infrastructure.Identity;
using BareqAlNaqool.Infrastructure.Localization;
using BareqAlNaqool.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BareqAlNaqool.Infrastructure.Services.Admin;

public class AdminPresidentService(AppDbContext db) : IAdminPresidentService
{
    private const int EntityId = 1;

    public async Task<AdminPresidentDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var (en, ar) = await AdminTranslationHelper.GetBilingualAsync(db, EntityTypes.President, EntityId, cancellationToken);
        return new AdminPresidentDto(
            AdminTranslationHelper.Get(en, "name"),
            AdminTranslationHelper.Get(ar, "name"));
    }

    public async Task<AdminPresidentDto> UpdateAsync(AdminPresidentUpdateDto dto, CancellationToken cancellationToken = default)
    {
        await AdminTranslationHelper.SaveBilingualAsync(
            db,
            EntityTypes.President,
            EntityId,
            new { name = dto.NameEn },
            new { name = dto.NameAr },
            cancellationToken);
        return new AdminPresidentDto(dto.NameEn, dto.NameAr);
    }
}
