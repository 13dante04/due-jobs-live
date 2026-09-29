import { Job } from '../models/job.model';

export function createMockJobs(): readonly Job[] {
  const now = Date.now();
  const hour = 60 * 60 * 1000;

  return [
    {
      id: '11111111-1111-1111-1111-111111111111',
      title: 'Send report',
      scheduledAt: new Date(now - 2 * hour).toISOString(),
      status: 'Pending',
      attempts: 0,
      lastError: null,
      kind: 'Email',
      target: 'operator@example.test',
    },
    {
      id: '33333333-3333-3333-3333-333333333333',
      title: 'Deliver export',
      scheduledAt: new Date(now - hour).toISOString(),
      status: 'Failed',
      attempts: 1,
      lastError: 'Upstream timeout',
      kind: 'Webhook',
      target: 'https://example.test/export',
    },
    {
      id: '22222222-2222-2222-2222-222222222222',
      title: 'Review metrics',
      scheduledAt: new Date(now + 3 * hour).toISOString(),
      status: 'Pending',
      attempts: 0,
      lastError: null,
      kind: 'Email',
      target: 'operator@example.test',
    },
  ];
}
