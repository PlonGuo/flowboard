using AutoMapper;
using FlowBoard.Application.Commands.Comments;
using FlowBoard.Application.Common;
using FlowBoard.Application.DTOs;
using FlowBoard.Application.Services;
using FlowBoard.Core.Entities;
using FlowBoard.Core.Interfaces;
using MediatR;

namespace FlowBoard.Application.Handlers.Comments;

/// <summary>
/// Handler for creating a new comment on a task.
/// </summary>
public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IBoardNotificationService _notificationService;
    private readonly IMentionService _mentionService;

    public CreateCommentHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IBoardNotificationService notificationService,
        IMentionService mentionService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
        _mentionService = mentionService;
    }

    public async Task<Result<CommentDto>> Handle(
        CreateCommentCommand request,
        CancellationToken cancellationToken)
    {
        // Get the task
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.TaskId);
        if (task == null)
        {
            return Result.Failure<CommentDto>("Task not found");
        }

        // Get the column and board
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

        // Verify user has access to the board
        if (!await UserHasAccessToBoard(board, request.UserId))
        {
            return Result.Failure<CommentDto>("You don't have access to this board");
        }

        // Create the comment
        var comment = new Comment
        {
            Content = request.Content.Trim(),
            TaskId = request.TaskId,
            AuthorId = request.UserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsEdited = false
        };

        await _unitOfWork.Comments.AddAsync(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Reload with navigation properties
        var createdComment = await GetCommentWithAuthorAsync(comment.Id);
        var commentDto = _mapper.Map<CommentDto>(createdComment);

        // Notify other clients viewing this board
        await _notificationService.NotifyCommentAddedAsync(
            board.Id,
            request.TaskId,
            commentDto,
            request.ConnectionId);

        // Process @mentions and create notifications
        await ProcessMentionsAsync(
            request.Content,
            task,
            board,
            createdComment!.Author,
            cancellationToken);

        return commentDto;
    }

    private async Task ProcessMentionsAsync(
        string content,
        TaskItem task,
        Board board,
        User commentAuthor,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get board members for mention matching
            var boardMembers = await GetBoardMembersAsync(board);

            // Parse mentions from content
            var mentionedUserIds = _mentionService.ParseMentions(content, boardMembers);

            if (mentionedUserIds.Any())
            {
                // Create notifications for mentioned users
                await _mentionService.CreateMentionNotificationsAsync(
                    task.Id,
                    task.Title,
                    board.Id,
                    commentAuthor.Id,
                    commentAuthor.FullName,
                    mentionedUserIds,
                    cancellationToken);
            }
        }
        catch (Exception)
        {
            // Don't fail the comment creation if mention processing fails
            // Just log the error (logging would be injected in a real app)
        }
    }

    private async Task<IEnumerable<UserSummaryDto>> GetBoardMembersAsync(Board board)
    {
        var members = new List<UserSummaryDto>();

        // Add board creator
        var creator = await _unitOfWork.Users.GetByIdAsync(board.CreatedById);
        if (creator != null)
        {
            members.Add(_mapper.Map<UserSummaryDto>(creator));
        }

        // Add team owner
        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team != null && team.OwnerId != board.CreatedById)
        {
            var owner = await _unitOfWork.Users.GetByIdAsync(team.OwnerId);
            if (owner != null)
            {
                members.Add(_mapper.Map<UserSummaryDto>(owner));
            }
        }

        // Add team members
        var teamMembers = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == board.TeamId);

        foreach (var tm in teamMembers)
        {
            if (members.All(m => m.Id != tm.UserId))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(tm.UserId);
                if (user != null)
                {
                    members.Add(_mapper.Map<UserSummaryDto>(user));
                }
            }
        }

        return members;
    }

    private async Task<Comment?> GetCommentWithAuthorAsync(int commentId)
    {
        var comments = await _unitOfWork.Comments.FindAsync(c => c.Id == commentId);
        var comment = comments.FirstOrDefault();

        if (comment != null)
        {
            // Load the author
            comment.Author = (await _unitOfWork.Users.GetByIdAsync(comment.AuthorId))!;
        }

        return comment;
    }

    private async Task<bool> UserHasAccessToBoard(Board board, int userId)
    {
        if (board.CreatedById == userId)
        {
            return true;
        }

        var team = await _unitOfWork.Teams.GetByIdAsync(board.TeamId);
        if (team?.OwnerId == userId)
        {
            return true;
        }

        var membership = await _unitOfWork.TeamMembers
            .FindAsync(tm => tm.TeamId == board.TeamId && tm.UserId == userId);

        return membership.Any();
    }
}
