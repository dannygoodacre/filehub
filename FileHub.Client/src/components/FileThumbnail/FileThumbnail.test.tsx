import { render, screen } from '@testing-library/react';

import FileThumbnail from './FileThumbnail';

const renderComponent = (name: string, contentType: string, url: string) => {
  render(<FileThumbnail name={name} contentType={contentType} url={url} />);
};

vi.mock('react-feather', () => ({
  File: () => <svg data-testid="file-icon" />,
  Film: () => <svg data-testid="film-icon" />,
  Headphones: () => <svg data-testid="headphone-icon" />
}));

describe('FileThumbnail', () => {
  it('renders an image for image files', () => {
    // Arrange
    const name = 'test_name';
    const contentType = 'image/png';
    const url = 'https://test_url.com/test_image.png';

    // Act
    renderComponent(name, contentType, url);

    // Assert
    const image = screen.getByRole('img');
    expect(image).toBeInTheDocument();
    expect(image).toHaveAttribute('src', url);
    expect(image).toHaveAttribute('alt', 'test_name');
  });

  it('renders an audio icon for audio files', () => {
    // Arrange
    const name = 'test_name';
    const contentType = 'audio/mp3';
    const url = 'https://test_url.com/test_file.mp3';

    // Act
    renderComponent(name, contentType, url);

    // Assert
    expect(screen.getByTestId('headphone-icon')).toBeInTheDocument();

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', url);
  });

  it('renders a film icon for video files', () => {
    // Arrange
    const name = 'test_name';
    const contentType = 'video/mp4';
    const url = 'https://test_url.com/test_file.mp4';

    // Act
    renderComponent(name, contentType, url);

    // Assert
    expect(screen.getByTestId('film-icon')).toBeInTheDocument();

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', url);
  });

  it('renders a generic icon for other files', () => {
    // Arrange
    const name = 'test_name';
    const contentType = 'test/plain';
    const url = 'https://test_url.com/test_file.txt';

    // Act
    renderComponent(name, contentType, url);

    // Assert
    const icon = screen.getByTestId('file-icon');
    expect(icon).toBeInTheDocument();

    const link = screen.getByRole('link');
    expect(link).toHaveAttribute('href', url);
  });
});
