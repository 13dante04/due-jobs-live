import { DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { Job } from '../../models/job.model';
import { JobStatusComponent } from '../job-status/job-status.component';

@Component({
  selector: 'app-jobs-table',
  standalone: true,
  imports: [DatePipe, JobStatusComponent],
  templateUrl: './jobs-table.component.html',
  styleUrl: './jobs-table.component.css',
})
export class JobsTableComponent {
  readonly jobs = input.required<readonly Job[]>();
}
