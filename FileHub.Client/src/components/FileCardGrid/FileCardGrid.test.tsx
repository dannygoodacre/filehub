import { render, screen } from '@testing-library/react';

import { FileCardGrid } from '@/components';
import { FileMetadata } from '@/types';

describe('FileCardGrid', () => {
  it('renders correctly', () => {
    // Arrange
    const fileMetadata: FileMetadata[] = [
      {
        id: '123',
        name: 'test name 1',
        accessLocation: 'test access location 1',
        contentType: 'test/content-type-1',
        createdAt: '2025-09-01',
        uploader: 'test_user1',
        tags: ['tag 1', 'tag 2']
      },
      {
        id: '456',
        name: 'test name 2',
        accessLocation: 'test access location 2',
        contentType: 'test/content-type-2',
        createdAt: '2025-09-01',
        uploader: 'test_user2',
        tags: ['tag 3', 'tag 4']
      }
    ];

    // Act
    render(<FileCardGrid fileMetadata={fileMetadata} />);

    // Assert
    expect(screen.getByText(fileMetadata[0].name)).toBeInTheDocument();
    expect(screen.getByText(fileMetadata[0].contentType)).toBeInTheDocument();
    fileMetadata[0].tags.map((tag) => expect(screen.getByText(tag)).toBeInTheDocument());

    expect(screen.getByText(fileMetadata[1].name)).toBeInTheDocument();
    expect(screen.getByText(fileMetadata[1].contentType)).toBeInTheDocument();
    fileMetadata[1].tags.map((tag) => expect(screen.getByText(tag)).toBeInTheDocument());
  });
});
