import { get, post } from '@/api/client/client';
import { API_URL } from '@/config';

describe('client', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn());
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('GET', async () => {
    // Arrange
    const endpoint = '/test';

    const mockResponse = { ok: true, data: 'test data' };

    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse
    });

    // Act
    const result = await get<{ data: string }>(endpoint);

    // Assert
    expect(fetch).toHaveBeenCalledWith(`${API_URL}${endpoint}`, {
      credentials: 'include'
    });

    expect(result).toEqual(mockResponse);
  });

  it('GET with JSON error', async () => {
    // Arrange
    const mockResponse = { message: 'Test message' };

    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      ok: false,
      status: 401,
      json: async () => mockResponse
    });

    // Act & Assert
    await expect(get('/test')).rejects.toThrowError('API Error 401: Test message');
  });

  it('GET with text error', async () => {
    // Arrange
    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      ok: false,
      status: 402,
      json: async () => {
        throw new Error('Test error');
      },
      text: async () => 'Test text'
    });

    // Act & Assert
    await expect(get('/test')).rejects.toThrowError('API Error 402: Test text');
  });

  it('POST', async () => {
    // Arrange
    const body = { data: 'test body data' };

    const mockResponse = { data: 'test response data' };

    const endpoint = '/test';

    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse
    });

    // Act
    const result = await post<typeof mockResponse, typeof body>(endpoint, body);

    // Assert
    expect(fetch).toHaveBeenCalledWith(`${API_URL}${endpoint}`, {
      method: 'POST',
      credentials: 'include',
      body: JSON.stringify(body),
      headers: { 'Content-Type': 'application/json' }
    });

    expect(result).toEqual(mockResponse);
  });

  it('POST with JSON error', async () => {
    // Arrange
    const mockResponse = { message: 'Test message' };

    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      ok: false,
      status: 403,
      json: async () => mockResponse
    });

    // Act & Assert
    await expect(post('/test')).rejects.toThrowError('API Error 403: Test message');
  });

  it('POST with text error', async () => {
    // Arrange
    (fetch as unknown as ReturnType<typeof vi.fn>).mockResolvedValueOnce({
      ok: false,
      status: 404,
      json: async () => {
        throw new Error('Test error');
      },
      text: async () => 'Test text'
    });

    // Act & Assert
    await expect(post('/test')).rejects.toThrowError('API Error 404: Test text');
  });
});
