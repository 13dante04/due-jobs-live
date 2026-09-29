export type JobStatus = 'Pending' | 'Completed' | 'Cancelled' | 'Failed';
export type JobKind = 'Email' | 'Webhook';

export interface Job {
  readonly id: string;
  readonly title: string;
  readonly scheduledAt: string;
  readonly status: JobStatus;
  readonly attempts: number;
  readonly lastError: string | null;
  readonly kind: JobKind;
  readonly target: string;
}
