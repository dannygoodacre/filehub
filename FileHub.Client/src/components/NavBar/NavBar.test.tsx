import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import { MemoryRouter, useLocation } from 'react-router-dom';
import { Mock } from 'vitest';

vi.mock('@/hooks');

import { NavBar } from '@/components';
import { useAuth } from '@/hooks';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false }
  }
});

const renderComponent = () => {
  render(
    <QueryClientProvider client={queryClient}>
      <MemoryRouter>
        <NavBar />
      </MemoryRouter>
    </QueryClientProvider>
  );
};

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useLocation: vi.fn()
  };
});

describe('NavBar', () => {
  it('renders correctly when logged out and on home page', () => {
    // Arrange
    (useLocation as Mock).mockReturnValue({
      pathname: '/'
    });

    (useAuth as Mock).mockReturnValue({
      data: null
    });

    // Act
    renderComponent();

    // Assert
    const title = screen.getByText('FileHub');
    expect(title).toBeInTheDocument();

    const homeLink = screen.queryByRole('link', { name: 'Home' });
    expect(homeLink).not.toBeInTheDocument();

    const uploadLink = screen.queryByRole('link', { name: 'Upload' });
    expect(uploadLink).not.toBeInTheDocument();

    const loginButton = screen.getByRole('button', { name: 'Login' });
    expect(loginButton).toBeInTheDocument();
  });

  it('renders correctly when logged out and on login page', () => {
    // Arrange
    (useLocation as Mock).mockReturnValue({
      pathname: '/login'
    });

    (useAuth as Mock).mockReturnValue({
      data: null
    });

    // Act
    renderComponent();

    // Assert
    const title = screen.getByText('FileHub');
    expect(title).toBeInTheDocument();

    const homeLink = screen.queryByRole('link', { name: /Home/i });
    expect(homeLink).not.toBeInTheDocument();

    const uploadLink = screen.queryByRole('link', { name: /Upload/i });
    expect(uploadLink).not.toBeInTheDocument();

    const loginButton = screen.queryByRole('button', { name: /Login/i });
    expect(loginButton).not.toBeInTheDocument();
  });

  it('renders correctly when logged in', () => {
    // Arrange
    (useLocation as Mock).mockReturnValue({
      pathname: '/'
    });

    const username = 'test_username';

    (useAuth as Mock).mockReturnValue({
      data: { username: username }
    });

    // Act
    renderComponent();

    // Assert
    const title = screen.getByText('FileHub');
    expect(title).toBeInTheDocument();

    const homeLink = screen.queryByRole('link', { name: /Home/i });
    expect(homeLink).toBeInTheDocument();

    const uploadLink = screen.queryByRole('link', { name: /Upload/i });
    expect(uploadLink).toBeInTheDocument();

    const usernameLabel = screen.getByText(username);
    expect(usernameLabel).toBeInTheDocument();

    const loginButton = screen.getByRole('button', { name: /Logout/i });
    expect(loginButton).toBeInTheDocument();
  });
});
