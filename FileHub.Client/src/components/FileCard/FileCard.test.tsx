import { render, screen } from '@testing-library/react';

import FileCard from './FileCard';

import { FileMetadata } from '@/types';

describe('FileCard component', () => {
  it('renders correctly', () => {
    // Arrange
    const fileMetaData: FileMetadata = {
      id: '123',
      name: 'test name',
      accessLocation: 'test access location',
      contentType: 'test/content-type',
      createdAt: '2025-09-01',
      uploader: 'test_user',
      tags: ['tag 1', 'tag 2']
    };

    // Act
    render(<FileCard fileMetadata={fileMetaData} />);

    // Assert
    expect(screen.getByText(fileMetaData.name)).toBeInTheDocument();
    expect(screen.getByText(fileMetaData.contentType)).toBeInTheDocument();
    fileMetaData.tags.map((tag) => expect(screen.getByText(tag)).toBeInTheDocument());
  });
});
