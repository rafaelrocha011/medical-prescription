import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

// O Domain persiste a dose como decimal(12,3).
export const dosagemValidator: ValidatorFn = (
  control: AbstractControl
): ValidationErrors | null => {
  if (control.value === null || control.value === '') {
    return null; // Validators.required cuida do preenchimento.
  }

  const texto = String(control.value);
  const numero = Number(control.value);

  const valido = Number.isFinite(numero) &&
    numero >= 0.001 &&
    numero <= 999_999_999.999 &&
    /^\d+(?:\.\d{1,3})?$/.test(texto);

  return valido ? null : { dosagemInvalida: true };
};
