import { API_URL } from '@/config';

async function handleResponse<T>(res: Response): Promise<T> {
  if (res.ok) {
    try {
      return await res.json();
    } catch {
      return null as unknown as T;
    }
  }

  let errorBody: any;

  try {
    errorBody = await res.json();
  } catch {
    errorBody = await res.text();
  }

  const message = typeof errorBody === 'string' ? errorBody : errorBody?.message || JSON.stringify(errorBody);

  throw new Error(`API Error ${res.status}: ${message}`);
}

export async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${API_URL}${path}`, {
    credentials: 'include' as RequestCredentials
  });

  return handleResponse<T>(res);
}

export async function post<TResponse, TBody = unknown>(path: string, body?: TBody): Promise<TResponse> {
  const isFormData = body instanceof FormData;

  const res = await fetch(`${API_URL}${path}`, {
    method: 'POST',
    credentials: 'include' as RequestCredentials,
    body: isFormData ? (body as FormData) : JSON.stringify(body),
    headers: isFormData ? undefined : { 'Content-Type': 'application/json' }
  });

  return handleResponse<TResponse>(res);
}
