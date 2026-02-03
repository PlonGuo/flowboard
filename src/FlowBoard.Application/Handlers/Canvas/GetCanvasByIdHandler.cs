using AutoMapper;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Queries.Canvas;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Canvas;

/// <summary>
/// Handler for getting canvas by ID.
/// </summary>
public class GetCanvasByIdHandler : IRequestHandler<GetCanvasByIdQuery, Result<CanvasDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCanvasByIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CanvasDetailDto>> Handle(
        GetCanvasByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Get the canvas
        var canvas = await _unitOfWork.Canvases.GetByIdAsync(request.CanvasId);
        if (canvas == null)
        {
            return Result.Failure<CanvasDetailDto>("Canvas not found");
        }

        // Verify user has access via team
        if (!await UserHasAccessToCanvas(canvas, request.UserId))
        {
            return Result.Failure<CanvasDetailDto>("You don't have permission to view this canvas");
        }

        // Load CreatedBy for mapping
        var createdBy = await _unitOfWork.Users.GetByIdAsync(canvas.CreatedById);

        // Get canvas data
        var canvasDataList = await _unitOfWork.CanvasData
            .FindAsync(cd => cd.CanvasId == canvas.Id);
        var canvasData = canvasDataList.FirstOrDefault();

        // Build DTO manually to avoid navigation property issues
        var dto = new CanvasDetailDto
        {
            Id = canvas.Id,
            Name = canvas.Name,
            Description = canvas.Description,
            BoardId = canvas.BoardId,
            TaskId = canvas.TaskId,
            TeamId = canvas.TeamId,
            CreatedBy = createdBy != null ? _mapper.Map<UserSummaryDto>(createdBy) : null!,
            Data = canvasData != null ? _mapper.Map<CanvasDataDto>(canvasData) : null,
            CreatedAt = canvas.CreatedAt,
            UpdatedAt = canvas.UpdatedAt
        };

        return dto;
    }

    private async Task<bool> UserHasAccessToCanvas(Core.Entities.Canvas canvas, int userId)
    {
        if (canvas.CreatedById == userId)
        {
            return true;
        }

        var team = await _unitOfWork.Teams.GetByIdAsync(canvas.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == canvas.TeamId && tm.UserId == userId);

        return membership.Any();
    }
}
