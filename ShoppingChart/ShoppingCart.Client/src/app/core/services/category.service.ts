import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { API_ENDPOINTS } from '../constants/api-endpoints';
import { Product, ProductCategory } from '../models/product.model';

export interface CategoryWithCount extends ProductCategory {
  productCount: number;
}

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private categories$ = new BehaviorSubject<CategoryWithCount[]>([]);

  /** Observable stream of categories — subscribe from any component. */
  readonly allCategories$ = this.categories$.asObservable();

  constructor(private http: HttpClient) {}

  /** Loads categories once and caches in the BehaviorSubject. */
  loadCategories(): Observable<CategoryWithCount[]> {
    return this.http.get<CategoryWithCount[]>(API_ENDPOINTS.productCategory.getAll).pipe(
      tap(data => this.categories$.next(data))
    );
  }

  getCategories(): CategoryWithCount[] {
    return this.categories$.value;
  }

  getProductsByCategory(categoryId: number): Observable<Product[]> {
    return this.http.get<Product[]>(API_ENDPOINTS.productCategory.getProductsByCategory(categoryId));
  }
}
