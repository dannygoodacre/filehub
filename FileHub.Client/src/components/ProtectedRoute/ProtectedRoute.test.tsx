import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { Mock } from 'vitest';

vi.mock('@/hooks');

import { ProtectedRoute } from '@/components';
import { useAuth } from '@/hooks';

const renderComponent = () => {
  render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route path="/login" element={<p>Test login page content</p>} />
        <Route element={<ProtectedRoute />}>
          <Route path="/protected" element={<p>Test protected page content</p>} />
        </Route>
      </Routes>
    </MemoryRouter>
  );
};

describe('ProtectedRoute', () => {
  it('renders correctly when logged out', () => {
    // Arrange
    (useAuth as Mock).mockReturnValue({
      data: null
    });

    // Act
    renderComponent();

    // Assert
    const loginPageContent = screen.getByText('Test login page content');
    expect(loginPageContent).toBeInTheDocument();

    const protectedPageContent = screen.queryByText('Test protected page content');
    expect(protectedPageContent).not.toBeInTheDocument();
  });

  it('renders correctly when logged in', () => {
    // Arrange
    (useAuth as Mock).mockReturnValue({
      data: { username: 'test_username' }
    });

    // Act
    renderComponent();

    // Assert
    const loginPageContent = screen.queryByText('Test login page content');
    expect(loginPageContent).not.toBeInTheDocument();

    const protectedPageContent = screen.getByText('Test protected page content');
    expect(protectedPageContent).toBeInTheDocument();
  });
});
