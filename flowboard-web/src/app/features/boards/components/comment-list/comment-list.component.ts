import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  computed,
  inject,
  ChangeDetectionStrategy,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommentDto } from '../../../../core/models/task.model';
import { UserSummaryDto } from '../../../../core/models/board.model';
import { AuthService } from '../../../../core/services/auth.service';
import { MentionAutocompleteComponent } from '../mention-autocomplete/mention-autocomplete.component';

@Component({
  selector: 'app-comment-list',
  standalone: true,
  imports: [FormsModule, MentionAutocompleteComponent],
  templateUrl: './comment-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommentListComponent {
  private readonly authService = inject(AuthService);

  @Input() set comments(value: CommentDto[]) {
    this.commentsList.set(value);
  }

  @Input() set boardMembers(value: UserSummaryDto[]) {
    this.membersList.set(value);
  }

  @Input() isLoading = false;
  @Input() isSaving = false;

  @ViewChild('commentInput') commentInputRef?: ElementRef<HTMLInputElement>;

  @Output() addComment = new EventEmitter<string>();
  @Output() updateComment = new EventEmitter<{ commentId: number; content: string }>();
  @Output() deleteComment = new EventEmitter<number>();

  readonly commentsList = signal<CommentDto[]>([]);
  readonly membersList = signal<UserSummaryDto[]>([]);
  readonly newCommentContent = signal('');
  readonly editingCommentId = signal<number | null>(null);
  readonly editContent = signal('');

  // Mention autocomplete state
  readonly showMentionAutocomplete = signal(false);
  readonly mentionQuery = signal('');
  readonly mentionStartIndex = signal<number | null>(null);

  readonly currentUserId = computed(() => this.authService.user()?.id);

  readonly sortedComments = computed(() => {
    return [...this.commentsList()].sort(
      (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
    );
  });

  submitComment(): void {
    const content = this.newCommentContent().trim();
    if (!content) return;

    this.addComment.emit(content);
    this.newCommentContent.set('');
  }

  startEdit(comment: CommentDto): void {
    this.editingCommentId.set(comment.id);
    this.editContent.set(comment.content);
  }

  cancelEdit(): void {
    this.editingCommentId.set(null);
    this.editContent.set('');
  }

  saveEdit(): void {
    const commentId = this.editingCommentId();
    const content = this.editContent().trim();

    if (!commentId || !content) return;

    this.updateComment.emit({ commentId, content });
    this.cancelEdit();
  }

  confirmDelete(commentId: number): void {
    this.deleteComment.emit(commentId);
  }

  canEditComment(comment: CommentDto): boolean {
    return comment.author.id === this.currentUserId();
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    const now = new Date();
    const diff = now.getTime() - d.getTime();

    // Less than a minute
    if (diff < 60000) {
      return 'Just now';
    }

    // Less than an hour
    if (diff < 3600000) {
      const minutes = Math.floor(diff / 60000);
      return `${minutes}m ago`;
    }

    // Less than a day
    if (diff < 86400000) {
      const hours = Math.floor(diff / 3600000);
      return `${hours}h ago`;
    }

    // Less than a week
    if (diff < 604800000) {
      const days = Math.floor(diff / 86400000);
      return `${days}d ago`;
    }

    // Older than a week
    return d.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: d.getFullYear() !== now.getFullYear() ? 'numeric' : undefined,
    });
  }

  getAuthorInitials(author: CommentDto['author']): string {
    if (!author?.fullName) return '?';
    return author.fullName
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }

  // Mention handling methods
  onCommentInput(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;
    const cursorPos = input.selectionStart ?? value.length;

    this.newCommentContent.set(value);

    // Find the @ symbol before cursor
    const textBeforeCursor = value.substring(0, cursorPos);
    const lastAtIndex = textBeforeCursor.lastIndexOf('@');

    if (lastAtIndex >= 0) {
      // Check if there's a space after the @ (which would end the mention)
      const textAfterAt = textBeforeCursor.substring(lastAtIndex + 1);

      // Only show autocomplete if no space in the mention query
      if (!textAfterAt.includes(' ') || textAfterAt.trim() === '') {
        this.showMentionAutocomplete.set(true);
        this.mentionStartIndex.set(lastAtIndex);
        this.mentionQuery.set(textAfterAt);
        return;
      }
    }

    this.closeMentionAutocomplete();
  }

  onMemberSelect(member: UserSummaryDto): void {
    const content = this.newCommentContent();
    const startIndex = this.mentionStartIndex();

    if (startIndex === null) return;

    // Replace @query with @FullName
    const beforeMention = content.substring(0, startIndex);
    const afterMention = content.substring(
      startIndex + 1 + this.mentionQuery().length
    );
    const newContent = `${beforeMention}@${member.fullName} ${afterMention}`;

    this.newCommentContent.set(newContent);
    this.closeMentionAutocomplete();

    // Focus back on input
    setTimeout(() => {
      this.commentInputRef?.nativeElement?.focus();
    }, 0);
  }

  closeMentionAutocomplete(): void {
    this.showMentionAutocomplete.set(false);
    this.mentionQuery.set('');
    this.mentionStartIndex.set(null);
  }

  onInputKeydown(event: KeyboardEvent): void {
    if (this.showMentionAutocomplete()) {
      // Let the autocomplete handle these keys
      if (['ArrowUp', 'ArrowDown', 'Tab', 'Escape'].includes(event.key)) {
        return; // Don't prevent default - autocomplete will handle it
      }
      if (event.key === 'Enter') {
        // Autocomplete will handle Enter if visible
        return;
      }
    }

    // Normal Enter behavior - submit comment
    if (event.key === 'Enter' && !this.showMentionAutocomplete()) {
      this.submitComment();
    }
  }

  /**
   * Parse comment content and return HTML with highlighted mentions.
   */
  parseCommentWithMentions(content: string): string {
    // Match @FirstName LastName pattern (names with letters, spaces, hyphens, apostrophes)
    const mentionPattern = /@([A-Za-z][A-Za-z\s\-']*[A-Za-z]|[A-Za-z])/g;

    return content.replace(mentionPattern, (match) => {
      return `<span class="text-primary font-medium">${match}</span>`;
    });
  }
}
