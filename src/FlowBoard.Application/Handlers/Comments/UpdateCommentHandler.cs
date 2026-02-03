using AutoMapper;
using FlowBoard.Application.Commands.Comments;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Comments;

/// <summary>
/// Handler for updating an existing comment.
/// </summary>
public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBoardNotificationService _notificationService;

    public UpdateCommentHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBoardNotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
    }

    public async Task<Result<CommentDto>> Handle(
        UpdateCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the comment
        var comments = await _unitOfWork.Comments.FindAsync(c => c.Id == request.CommentId);
        var comment = comments.FirstOrDefault();

        if (comment == null)
        {
            return Result.Failure<CommentDto>("Comment not found");
        }

        // Verify user is the author
        if (comment.AuthorId != request.UserId)
        {
            return Result.Failure<CommentDto>("You can only edit your own comments");
        }

        // Get the task, column, and board for notification
        var task = await _unitOfWork.Tasks.GetByIdAsync(comment.TaskId);
        if (task == null)
        {
            return Result.Failure<CommentDto>("Task not found");
        }

        var column = await _unitOfWork.Columns.GetByIdAsync(task.ColumnId);
        if (column == null)
        {
            return Result.Failure<CommentDto>("Column not found");
        }

        var board = await _unitOfWork.Boards.GetByIdAsync(column.BoardId);
        if (board == null)
        {
            return Result.Failure<CommentDto>("Board not found");
        }

        // Update the comment
        comment.Content = request.Content.Trim();
        comment.UpdatedAt = DateTime.UtcNow;
        comment.IsEdited = true;

        _unitOfWork.Comments.Update(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with author
        var updatedComment = await GetCommentWithAuthorAsync(comment.Id);
        var commentDto = _mapper.Map<CommentDto>(updatedComment);

        // Notify other clients viewing this board
        await _notificationService.NotifyCommentUpdatedAsync(
            board.Id,
            comment.TaskId,
            commentDto,
            request.ConnectionId);

        return commentDto;
    }

    private async Task<Comment?> GetCommentWithAuthorAsync(int commentId)
    {
        var comments = await _unitOfWork.Comments.FindAsync(c => c.Id == commentId);
        var comment = comments.FirstOrDefault();

        if (comment != null)
        {
            comment.Author = (await _unitOfWork.Users.GetByIdAsync(comment.AuthorId))!;
        }

        return comment;
    }
}
