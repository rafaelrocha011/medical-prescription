import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('deve criar a aplicacao', () => {
    const fixture = TestBed.createComponent(App);

    expect(fixture.componentInstance).toBeTruthy();
  });

  it('deve exibir cabecalho, menu e rodape', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();

    const elemento = fixture.nativeElement as HTMLElement;

    expect(elemento.querySelector('.marca')?.textContent)
      .toContain('MedicalPrescription');

    expect(elemento.querySelector('.sidebar nav'))
      .toBeTruthy();

    expect(elemento.querySelector('.app-footer'))
      .toBeTruthy();
  });

  it('deve abrir e fechar o menu', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;

    app.alternarMenu();
    expect(app.menuAberto()).toBe(true);

    app.fecharMenu();
    expect(app.menuAberto()).toBe(false);
  });
});