using AutoMapper;
using FlowBoard.Application.Commands.Canvas;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Canvas;

/// <summary>
/// Handler for updating canvas data.
/// </summary>
public class UpdateCanvasDataHandler : IRequestHandler<UpdateCanvasDataCommand, Result<SaveCanvasDataResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICanvasNotificationService? _notificationService;

    public UpdateCanvasDataHandler(
        IUnitOfWork unitOfWork,
        ICanvasNotificationService? notificationService = null)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<Result<SaveCanvasDataResponse>> Handle(
        UpdateCanvasDataCommand request,
        CancellationToken cancellationToken)
    {
        // Get the canvas
        var canvas = await _unitOfWork.Canvases.GetByIdAsync(request.CanvasId);
        if (canvas == null)
        {
            return Result.Failure<SaveCanvasDataResponse>("Canvas not found");
        }

        // Verify user has access via team
        if (!await UserHasAccessToCanvas(canvas, request.UserId))
        {
            return Result.Failure<SaveCanvasDataResponse>("You don't have permission to edit this canvas");
        }

        // Get or create canvas data
        var canvasDataList = await _unitOfWork.CanvasData
            .FindAsync(cd => cd.CanvasId == canvas.Id);
        var canvasData = canvasDataList.FirstOrDefault();

        if (canvasData == null)
        {
            // Create new canvas data
            canvasData = new Core.Entities.CanvasData
            {
                CanvasId = canvas.Id,
                Elements = request.Elements,
                AppState = request.AppState,
                Files = request.Files,
                Version = 1,
                UpdatedById = request.UserId,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.CanvasData.AddAsync(canvasData);
        }
        else
        {
            // Update existing canvas data
            canvasData.Elements = request.Elements;
            canvasData.AppState = request.AppState;
            canvasData.Files = request.Files;
            canvasData.Version++;
            canvasData.UpdatedById = request.UserId;
            canvasData.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.CanvasData.Update(canvasData);
        }

        // Update canvas timestamp
        canvas.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Canvases.Update(canvas);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Notify other clients viewing this canvas
        if (_notificationService != null)
        {
            await _notificationService.NotifyCanvasUpdatedAsync(
                canvas.Id,
                request.Elements,
                request.AppState,
                canvasData.Version,
                request.ConnectionId);
        }

        return new SaveCanvasDataResponse
        {
            CanvasId = canvas.Id,
            Version = canvasData.Version,
            UpdatedAt = canvasData.UpdatedAt
        };
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
