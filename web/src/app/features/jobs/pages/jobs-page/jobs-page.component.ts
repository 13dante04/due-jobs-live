import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { finalize } from 'rxjs';
import { JobsService } from '../../data-access/jobs.service';
import { Job } from '../../models/job.model';
import { JobsTableComponent } from '../../components/jobs-table/jobs-table.component';
import { EmptyStateComponent } from '../../../../shared/ui/empty-state/empty-state.component';

@Component({
  selector: 'app-jobs-page',
  standalone: true,
  imports: [JobsTableComponent, EmptyStateComponent],
  templateUrl: './jobs-page.component.html',
  styleUrl: './jobs-page.component.css',
})
export class JobsPageComponent implements OnInit {
  private readonly jobsService = inject(JobsService);
  private readonly destroyRef = inject(DestroyRef);
  readonly jobs = signal<readonly Job[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');

  ngOnInit(): void {
    this.refresh();
  }

  refresh(): void {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    this.jobsService.getJobs().pipe(
      takeUntilDestroyed(this.destroyRef),
      finalize(() => this.loading.set(false)),
    ).subscribe({
      next: jobs => this.jobs.set(jobs),
      error: () => this.error.set('Could not load jobs. Please try again.'),
    });
  }
}
