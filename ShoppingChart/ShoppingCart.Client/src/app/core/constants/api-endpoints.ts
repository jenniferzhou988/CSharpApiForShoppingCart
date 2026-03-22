import { environment } from '../../../environments/environment';

const BASE = environment.apiUrl;

export const API_ENDPOINTS = {
  auth: {
    login:      `${BASE}/Auth/login`,
    register:   `${BASE}/Auth/register`,
    refresh:    `${BASE}/Auth/refresh`,
    logout:     `${BASE}/Auth/logout`,
    logoutAll:  `${BASE}/Auth/logout-all`,
  },
  productCategory: {
    getAll:               `${BASE}/ProductCategory`,
    getById:              (id: number) => `${BASE}/ProductCategory/${id}`,
    getProductsByCategory:(id: number) => `${BASE}/ProductCategory/${id}/products`,
  },
  product: {
    getAll:             `${BASE}/Product`,
    getById:            (id: number) => `${BASE}/Product/${id}`,
    create:             `${BASE}/Product`,
    update:             (id: number) => `${BASE}/Product/${id}`,
    delete:             (id: number) => `${BASE}/Product/${id}`,
    addImage:           (productId: number) => `${BASE}/Product/${productId}/images`,
    removeImage:        (productId: number, imageId: number) => `${BASE}/Product/${productId}/images/${imageId}`,
    addCategory:        (productId: number, categoryId: number) => `${BASE}/Product/${productId}/categories/${categoryId}`,
    removeCategory:     (productId: number, categoryId: number) => `${BASE}/Product/${productId}/categories/${categoryId}`,
    importProduct:      (productId: number) => `${BASE}/Product/${productId}/import`,
  },
  shoppingCart: {
    getAll:             `${BASE}/ShoppingCart`,
    getById:            (id: number) => `${BASE}/ShoppingCart/${id}`,
    create:             `${BASE}/ShoppingCart`,
    delete:             (id: number) => `${BASE}/ShoppingCart/${id}`,
    addItem:            (cartId: number) => `${BASE}/ShoppingCart/${cartId}/items`,
    updateItem:         (cartId: number, detailId: number) => `${BASE}/ShoppingCart/${cartId}/items/${detailId}`,
    removeItem:         (cartId: number, detailId: number) => `${BASE}/ShoppingCart/${cartId}/items/${detailId}`,
  },
  order: {
    getAll:             `${BASE}/Order`,
    getById:            (id: number) => `${BASE}/Order/${id}`,
    create:             `${BASE}/Order`,
  },
  shipping: {
    getAll:             `${BASE}/ShippingTracking`,
    getById:            (id: number) => `${BASE}/ShippingTracking/${id}`,
    create:             `${BASE}/ShippingTracking`,
  },
} as const;
