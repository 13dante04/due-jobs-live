import { Component, input } from '@angular/core';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  templateUrl: './empty-state.component.html',
  styles: '.empty-state { padding: 3rem 1rem; text-align: center; background: var(--surface); border: 1px dashed var(--border); border-radius: .75rem; } p { color: var(--muted); }',
})
export class EmptyStateComponent {
  readonly title = input.required<string>();
  readonly description = input('');
}
