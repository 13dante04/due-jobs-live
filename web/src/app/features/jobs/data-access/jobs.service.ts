import { Injectable } from '@angular/core';
import { defer, delay, Observable, of } from 'rxjs';
import { Job } from '../models/job.model';
import { createMockJobs } from './jobs.mock';

@Injectable({ providedIn: 'root' })
export class JobsService {
  getJobs(): Observable<readonly Job[]> {
    return defer(() => of(createMockJobs())).pipe(delay(300));
  }
}
