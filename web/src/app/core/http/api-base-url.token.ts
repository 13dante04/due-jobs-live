import { InjectionToken } from '@angular/core';

/** Relative requests use the dev proxy or the production reverse proxy. */
export const API_BASE_URL = new InjectionToken<string>('API_BASE_URL', {
  providedIn: 'root',
  factory: () => '',
});
