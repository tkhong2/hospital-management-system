using HospitalMS.BuildingBlocks.Common;
using HospitalMS.Identity.Application.Common.Interfaces;
using HospitalMS.Identity.Domain.Entities;
using HospitalMS.Identity.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HospitalMS.Identity.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(
    int PageNumber = 1,
    int PageSize = 10,
    string? Search = null,
    Guid? DepartmentId = null,
    UserStatus? Status = null
) : IRequest<Result<PagedResult<UserListItemDto>>>;

public record UserListItemDto(
    Guid Id,
    string Username,
    string FullName,
    string Email,
    string? PhoneNumber,
    Guid? DepartmentId,
    string? DepartmentName,
    UserStatus Status,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt
);

public class GetUsersQueryHandler(IIdentityDbContext dbContext)
    : IRequestHandler<GetUsersQuery, Result<PagedResult<UserListItemDto>>>
{
    public async Task<Result<PagedResult<UserListItemDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.Users
            .Include(u => u.Department)
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Where(u => !u.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.Username.ToLower().Contains(search) ||
                u.FullName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        if (request.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == request.DepartmentId.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(u => u.Status == request.Status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(u => new UserListItemDto(
                u.Id,
                u.Username,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.DepartmentId,
                u.Department != null ? u.Department.Name : null,
                u.Status,
                u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                u.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<UserListItemDto>(
            items: items,
            totalCount: totalCount,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize
        );

        return Result.Success(pagedResult);
    }
}

