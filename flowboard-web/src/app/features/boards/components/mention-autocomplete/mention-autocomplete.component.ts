import {
  Component,
  Input,
  Output,
  EventEmitter,
  signal,
  computed,
  ChangeDetectionStrategy,
  HostListener,
  ElementRef,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { UserSummaryDto } from '../../../../core/models/board.model';

@Component({
  selector: 'app-mention-autocomplete',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mention-autocomplete.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MentionAutocompleteComponent {
  private readonly elementRef = inject(ElementRef);

  @Input({ required: true }) set members(value: UserSummaryDto[]) {
    this.membersList.set(value);
  }

  @Input() set searchQuery(value: string) {
    this.query.set(value.toLowerCase());
  }

  @Input() excludeUserId?: number;

  @Output() selectMember = new EventEmitter<UserSummaryDto>();
  @Output() closeAutocomplete = new EventEmitter<void>();

  private readonly membersList = signal<UserSummaryDto[]>([]);
  private readonly query = signal<string>('');
  readonly selectedIndex = signal<number>(0);

  readonly filteredMembers = computed(() => {
    const q = this.query();
    let members = this.membersList();

    // Exclude current user if specified
    if (this.excludeUserId) {
      members = members.filter((m) => m.id !== this.excludeUserId);
    }

    if (!q) {
      return members.slice(0, 5);
    }

    return members
      .filter(
        (m) =>
          m.fullName.toLowerCase().includes(q) ||
          m.email.toLowerCase().includes(q)
      )
      .slice(0, 5);
  });

  @HostListener('document:keydown', ['$event'])
  onKeydown(event: KeyboardEvent): void {
    const members = this.filteredMembers();

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.selectedIndex.update((i) =>
          i < members.length - 1 ? i + 1 : 0
        );
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.selectedIndex.update((i) =>
          i > 0 ? i - 1 : members.length - 1
        );
        break;
      case 'Enter':
      case 'Tab':
        event.preventDefault();
        const selected = members[this.selectedIndex()];
        if (selected) {
          this.onSelect(selected);
        }
        break;
      case 'Escape':
        event.preventDefault();
        this.closeAutocomplete.emit();
        break;
    }
  }

  onSelect(member: UserSummaryDto): void {
    this.selectMember.emit(member);
  }

  onHover(index: number): void {
    this.selectedIndex.set(index);
  }

  getInitials(name: string): string {
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
