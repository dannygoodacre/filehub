import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { Mock } from 'vitest';

import { UploadBox } from '@/components';
import { useUpload } from '@/hooks';

const queryClient = new QueryClient({
  defaultOptions: {
    queries: { retry: false },
    mutations: { retry: false }
  }
});

const renderComponent = () => {
  render(
    <QueryClientProvider client={queryClient}>
      <UploadBox />
    </QueryClientProvider>
  );
};

vi.mock('@/hooks/useUpload');

describe('UploadBox', () => {
  it('initial render', () => {
    // Arrange
    (useUpload as Mock).mockReturnValue({
      isSuccess: false
    });

    // Act
    renderComponent();

    // Assert
    const title = screen.getByText('Upload');
    expect(title).toBeInTheDocument();

    const uploadInstructionText = screen.getByText('Drop a file here or click to browse');
    expect(uploadInstructionText).toBeInTheDocument();
  });

  it('file selected', async () => {
    // Arrange
    (useUpload as Mock).mockReturnValue({
      reset: vi.fn(),
      isSuccess: false
    });

    const fileName = 'test-file.txt';

    const file = new File(['test file contents'], fileName, { type: 'text/plain' });

    // Act 1
    renderComponent();

    const fileInput = screen.getByLabelText('Drop a file here or click to browse');
    await userEvent.upload(fileInput, file);

    // Assert 2
    const fileNameLabel = screen.getByText(fileName);
    expect(fileNameLabel).toBeInTheDocument();

    const nameInput = screen.getByPlaceholderText('Name');
    expect(nameInput).toBeInTheDocument();

    const tagsInput = screen.getByPlaceholderText('Tags');
    expect(tagsInput).toBeInTheDocument();

    const addTagButton = screen.getByRole('button', { name: 'Add' });
    expect(addTagButton).toBeInTheDocument();

    const uploadButton = screen.getByRole('button', { name: 'Upload' });
    expect(uploadButton).toBeInTheDocument();
    expect(uploadButton).toBeDisabled();
  });

  it('tags entered and removed', async () => {
    // Arrange
    (useUpload as Mock).mockReturnValue({
      reset: vi.fn(),
      isSuccess: false
    });

    const fileName = 'test-file.txt';

    const file = new File(['test file contents'], fileName, { type: 'text/plain' });

    const tag1 = 'test tag 1';
    const tag2 = 'test tag 2';

    // Act 1
    renderComponent();

    const fileInput = screen.getByLabelText('Drop a file here or click to browse');
    await userEvent.upload(fileInput, file);

    const tagsInput = screen.getByPlaceholderText('Tags');
    await userEvent.type(tagsInput, tag1);

    const addTagButton = screen.getByRole('button', { name: 'Add' });
    await userEvent.click(addTagButton);

    await userEvent.type(tagsInput, tag2);

    await userEvent.click(addTagButton);

    // Assert 1
    let tag1Button = screen.getByRole('button', { name: tag1 });
    expect(screen.getByRole('button', { name: tag1 })).toBeInTheDocument();

    let tag2Button = screen.getByRole('button', { name: tag2 });
    expect(tag2Button).toBeInTheDocument();

    // Act 2
    await userEvent.click(tag1Button);

    // Assert 2
    tag1Button = screen.queryByRole('button', { name: tag1 }) as HTMLElement;
    expect(tag1Button).not.toBeInTheDocument();

    tag2Button = screen.queryByRole('button', { name: tag2 }) as HTMLElement;
    expect(tag2Button).toBeInTheDocument();
  });

  it('upload success', async () => {
    // Arrange
    const fileName = 'test-file.txt';

    const file = new File(['test file contents'], fileName, { type: 'text/plain' });

    const name = 'test name';

    const tag = 'test tag';

    (useUpload as Mock).mockReturnValue({
      mutate: vi.fn(),
      reset: vi.fn(),
      isSuccess: true
    });

    // Act 1
    renderComponent();

    const fileInput = screen.getByLabelText('Drop a file here or click to browse');
    await userEvent.upload(fileInput, file);

    const nameInput = screen.getByPlaceholderText('Name');
    await userEvent.type(nameInput, name);

    const tagsInput = screen.getByPlaceholderText('Tags');
    await userEvent.type(tagsInput, tag);

    const addTagButton = screen.getByRole('button', { name: 'Add' });
    await userEvent.click(addTagButton);

    // Assert 1
    const uploadButton = screen.getByRole('button', { name: 'Upload' });
    expect(uploadButton).toBeEnabled();

    // Act 2
    await userEvent.click(uploadButton);

    // Assert 2
    const uploadAlert = screen.getByRole('alert');
    expect(uploadAlert).toHaveTextContent('File uploaded successfully!');
    expect(uploadAlert).toBeInTheDocument();
  });
});
