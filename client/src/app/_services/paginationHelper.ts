import { HttpParams, HttpClient } from '@angular/common/http';
import { map } from 'rxjs';
import { PaginatedResults } from '../_models/Pagination';

export function getPaginatedResults<T>(
  url: string,
  params: HttpParams,
  http: HttpClient
) {
  const paginatedResult: PaginatedResults<T> = new PaginatedResults<T>();
  return http.get<T>(url, { observe: 'response', params }).pipe(
    map((response) => {
      if (response.body) {
        paginatedResult.result = response.body;
      }
      // Create tha pagination object
      const pagination = response.headers.get('pagination');
      if (pagination) {
        paginatedResult.pagination = JSON.parse(pagination);
      }
      return paginatedResult;
    })
  );
}

export function getPaginationHeaders(pageNumber: number, pageSize: number) {
  let params = new HttpParams();

  params = params.append('pageNumber', pageNumber);
  params = params.append('pageSize', pageSize);

  return params;
}
