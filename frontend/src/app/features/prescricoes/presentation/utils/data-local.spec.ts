import { describe, expect, it } from 'vitest';
import { dataLocalHoje } from './data-local';

describe('dataLocalHoje', () => {
  it('usa o dia local e formata com zeros a esquerda', () => {
    expect(dataLocalHoje(new Date(2026, 0, 5, 23, 59))).toBe('2026-01-05');
  });

  it('considera a virada do ano local', () => {
    expect(dataLocalHoje(new Date(2026, 11, 31, 23, 59))).toBe('2026-12-31');
  });
});
