import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-whiteboard',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './whiteboard.component.html',
  styleUrl: './whiteboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WhiteboardComponent {}
