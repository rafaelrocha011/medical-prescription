import { FormControl } from '@angular/forms';
import { describe, expect, it } from 'vitest';
import { dosagemValidator } from './dosagem.validator';

describe('dosagemValidator', () => {
  it('aceita a menor dose permitida', () => {
    expect(dosagemValidator(new FormControl(0.001))).toBeNull();
  });

  it('aceita tres casas decimais', () => {
    expect(dosagemValidator(new FormControl(12.345))).toBeNull();
  });

  it('rejeita mais de tres casas decimais', () => {
    expect(dosagemValidator(new FormControl(1.2345))).toEqual({ dosagemInvalida: true });
  });

  it('rejeita valores fora do limite do banco', () => {
    expect(dosagemValidator(new FormControl(1_000_000_000))).not.toBeNull();
    expect(dosagemValidator(new FormControl(0))).not.toBeNull();
  });
});
