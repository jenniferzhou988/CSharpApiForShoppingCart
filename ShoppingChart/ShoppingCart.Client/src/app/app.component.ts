import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from './core/services/auth.service';
import { CategoryService, CategoryWithCount } from './core/services/category.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit {
  title = 'ShoppingCart';
  categories: CategoryWithCount[] = [];

  constructor(
    public authService: AuthService,
    private categoryService: CategoryService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.categoryService.loadCategories().subscribe({
        next: (data) => this.categories = data
      });
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
