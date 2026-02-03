using FlowBoard.Application.Commands.Canvas;
using FlowBoard.Application.Common;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Canvas;

/// <summary>
/// Handler for deleting a canvas.
/// </summary>
public class DeleteCanvasHandler : IRequestHandler<DeleteCanvasCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCanvasHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteCanvasCommand request,
        CancellationToken cancellationToken)
    {
        // Get the canvas
        var canvas = await _unitOfWork.Canvases.GetByIdAsync(request.CanvasId);
        if (canvas == null)
        {
            return Result.Failure("Canvas not found");
        }

        // Verify user has permission (canvas creator or team owner)
        if (!await UserCanDeleteCanvas(canvas, request.UserId))
        {
            return Result.Failure("You don't have permission to delete this canvas");
        }

        // Delete canvas data first
        var canvasDataList = await _unitOfWork.CanvasData
            .FindAsync(cd => cd.CanvasId == canvas.Id);

        foreach (var canvasData in canvasDataList)
        {
            _unitOfWork.CanvasData.Remove(canvasData);
        }

        // Delete the canvas
        _unitOfWork.Canvases.Remove(canvas);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<bool> UserCanDeleteCanvas(Core.Entities.Canvas canvas, int userId)
    {
        // Canvas creator can delete
        if (canvas.CreatedById == userId)
        {
            return true;
        }

        // Team owner can delete
        var team = await _unitOfWork.Teams.GetByIdAsync(canvas.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        return false;
    }
}
