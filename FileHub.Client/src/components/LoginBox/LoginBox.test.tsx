import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Mock } from 'vitest';

import { LoginBox } from '@/components';
import { useAuth, useLogin } from '@/hooks';
import { UserInfo } from '@/types';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false }
  }
});

const renderComponent = () => {
  render(
    <QueryClientProvider client={queryClient}>
      <LoginBox />
    </QueryClientProvider>
  );
};

vi.mock('@/hooks/useAuth');

vi.mock('@/hooks/useLogin');

afterEach(() => {
  vi.clearAllMocks();
});

describe('LoginBox component', () => {
  it('renders correctly', () => {
    // Arrange
    (useLogin as Mock).mockReturnValue({
      isPending: false
    });

    (useAuth as Mock).mockReturnValue({
      data: null
    });

    // Act
    renderComponent();

    // Assert
    const title = screen.getByRole('heading', { name: /Login/i });
    expect(title).toBeInTheDocument();

    const usernameLabel = screen.getByLabelText('Username');
    expect(usernameLabel).toBeInTheDocument();

    const usernameInput = screen.getByRole('textbox', { name: /Username/i });
    expect(usernameInput).toBeInTheDocument();

    const usernameError = screen.queryByPlaceholderText('Please enter a username');
    expect(usernameError).not.toBeInTheDocument();

    const passwordLabel = screen.getByLabelText('Password');
    expect(passwordLabel).toBeInTheDocument();

    const passwordInput = screen.getByPlaceholderText('**********');
    expect(passwordInput).toBeInTheDocument();

    const passwordError = screen.queryByPlaceholderText('Please enter a password');
    expect(passwordError).not.toBeInTheDocument();

    const loginButton = screen.getByRole('button');
    expect(loginButton).toBeInTheDocument();

    const loginSuccessMessage = screen.queryByRole('alert', { name: /Login successful/i });
    expect(loginSuccessMessage).not.toBeInTheDocument();

    const loginFailedMessage = screen.queryByRole('alert', { name: /Login failed/i });
    expect(loginFailedMessage).not.toBeInTheDocument();
  });

  it('shows username and password missing errors', async () => {
    // Arrange
    (useLogin as Mock).mockReturnValue({
      isPending: false
    });

    (useAuth as Mock).mockReturnValue({
      data: null
    });

    // Act
    renderComponent();

    const loginButton = screen.getByRole('button', { name: /Login/i });
    await userEvent.click(loginButton);

    // Assert
    const usernameError = screen.getByText('Please enter a username');
    expect(usernameError).toBeInTheDocument();

    const passwordError = screen.getByText('Please enter a password');
    expect(passwordError).toBeInTheDocument();
  });

  it('shows login failure message', async () => {
    // Arrange
    const username = 'test_username';
    const password = 'test_incorrect_password';

    (useLogin as Mock).mockReturnValue({
      mutate: vi.fn(),
      reset: vi.fn(),
      isError: true
    });

    (useAuth as Mock).mockReturnValue({
      data: null
    });

    // Act
    renderComponent();

    const usernameInput = screen.getByRole('textbox', { name: /Username/i });
    await userEvent.type(usernameInput, username);

    const passwordInput = screen.getByPlaceholderText('**********');
    await userEvent.type(passwordInput, password);

    const loginButton = screen.getByRole('button', { name: /Login/i });
    await userEvent.click(loginButton);

    // Assert
    const loginMessage = screen.getByRole('alert');
    expect(loginMessage).toHaveTextContent('Login failed');
    expect(loginMessage).toBeInTheDocument();
  });

  it('shows login success message', async () => {
    // Arrange
    const username = 'test_username';
    const password = 'test_correct_password';

    (useLogin as Mock).mockReturnValue({
      mutate: vi.fn(),
      reset: vi.fn(),
      isSuccess: true
    });

    const userInfo: UserInfo = {
      username: username,
      isAccountConfirmed: true
    };

    (useAuth as Mock).mockReturnValue({
      isSuccess: true,
      data: userInfo
    });

    // Act
    renderComponent();

    const usernameInput = screen.getByRole('textbox', { name: /Username/i });
    await userEvent.type(usernameInput, username);

    const passwordInput = screen.getByPlaceholderText('**********');
    await userEvent.type(passwordInput, password);

    const loginButton = screen.getByRole('button', { name: /Login/i });
    await userEvent.click(loginButton);

    // Assert
    const loginMessage = screen.getByRole('alert');
    expect(loginMessage).toHaveTextContent('Login successful');
    expect(loginMessage).toBeInTheDocument();
  });
});
