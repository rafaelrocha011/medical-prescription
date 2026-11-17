import {
  ChangeDetectionStrategy,
  Component,
  signal
} from '@angular/core';

import {
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './app.html',
  styleUrl: './app.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App {
  readonly menuAberto = signal(false);
  readonly anoAtual = new Date().getFullYear();

  alternarMenu(): void {
    this.menuAberto.update(aberto => !aberto);
  }

  fecharMenu(): void {
    this.menuAberto.set(false);
  }
}