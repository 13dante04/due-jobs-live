import { Component, input } from '@angular/core';
import { JobStatus } from '../../models/job.model';

@Component({
  selector: 'app-job-status',
  standalone: true,
  templateUrl: './job-status.component.html',
  styleUrl: './job-status.component.css',
})
export class JobStatusComponent {
  readonly status = input.required<JobStatus>();
}
