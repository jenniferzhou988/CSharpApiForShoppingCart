import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CategoryService } from '../../../core/services/category.service';
import { Product } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-browse',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './product-browse.component.html',
  styleUrl: './product-browse.component.scss'
})
export class ProductBrowseComponent implements OnInit {
  products: Product[] = [];
  categoryName = '';
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private categoryService: CategoryService
  ) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('categoryId'));
      if (id) {
        this.loadProducts(id);
        const cat = this.categoryService.getCategories().find(c => c.id === id);
        this.categoryName = cat?.categoryName ?? 'Category';
      }
    });
  }

  loadProducts(categoryId: number): void {
    this.loading = true;
    this.categoryService.getProductsByCategory(categoryId).subscribe({
      next: (data) => {
        this.products = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  getFirstImage(product: Product): string {
    return product.images?.[0]?.imageUrl ?? 'assets/placeholder.png';
  }

  getTotalInventory(product: Product): number {
    return product.productInventories?.reduce((sum, inv) => sum + inv.quantity, 0) ?? 0;
  }
}
