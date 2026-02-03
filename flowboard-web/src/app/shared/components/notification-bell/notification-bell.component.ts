import {
  Component,
  inject,
  signal,
  HostListener,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { NotificationService } from '../../../core/services/notification.service';
import { NotificationPanelComponent } from '../notification-panel/notification-panel.component';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, NotificationPanelComponent],
  templateUrl: './notification-bell.component.html',
})
export class NotificationBellComponent {
  private readonly elementRef = inject(ElementRef);
  readonly notificationService = inject(NotificationService);

  readonly isPanelOpen = signal<boolean>(false);

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!this.elementRef.nativeElement.contains(target)) {
      this.isPanelOpen.set(false);
    }
  }

  togglePanel(): void {
    const wasOpen = this.isPanelOpen();
    this.isPanelOpen.set(!wasOpen);

    // Load notifications when opening the panel
    if (!wasOpen) {
      this.notificationService.loadNotifications().subscribe();
    }
  }

  closePanel(): void {
    this.isPanelOpen.set(false);
  }

  formatUnreadCount(count: number): string {
    return count > 99 ? '99+' : String(count);
  }
}
