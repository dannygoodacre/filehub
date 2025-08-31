<script lang="ts">
    import { UploadIcon, FileIcon, XIcon } from 'svelte-feather-icons';
    import { fileService } from '../services/fileService';

    let uploading = $state(false);
    let uploadComplete = $state(false);

    let selectedFile = $state<File | null>(null);

    let name = $state<string>();

    let tags = $state<string[]>([]);
    let tagInput = $state('');

    function handleFileSelect(event: Event) {
        const input = event.target as HTMLInputElement;
        const files = input.files as FileList;

        if (files && files.length > 0) {
            selectedFile = files[0];
        }
    }

    function removeFile() {
        selectedFile = null;
        tags = [];
    }

    function addTag() {
        const tag = tagInput.trim();

        if (tag && !tags.includes(tag)) {
            tags = [...tags, tag];
            tagInput = '';
        }
    }
    function removeTag(index: number) {
        tags = tags.filter((_, i) => i !== index);
    }

    function handleKeydown(event: KeyboardEvent) {
        if (event.key === 'Enter') {
            event.preventDefault();
            addTag();
        }
    }

    async function handleUpload() {
        if (!selectedFile) return;

        name = (name || selectedFile.name).trim();

        uploading = true;
        uploadComplete = await fileService.uploadFile(selectedFile, name, tags);
        uploading = false;

        selectedFile = null;
        tags = [];
    }

    function fileSize(size: number): string {
        const units = ['B', 'KB', 'MB', 'GB'];

        let i = 0;
        while (size >= 1000 && i < units.length - 1) {
            size /= 1000;
            i++;
        }

        return `${i == 0 ? size : size.toFixed(2)} ${units[i]}`;
    }
</script>

<div class="mx-auto my-8 max-w-xl rounded bg-white p-6 shadow-sm">
    <h1 class="mb-6 text-2xl font-semibold text-zinc-800">Upload</h1>

    <div class="w-full">
        <div class="relative rounded-lg border-2 border-dashed border-zinc-300 p-8 text-center transition-colors hover:bg-zinc-50">
            <input
                class="absolute inset-0 h-full w-full cursor-pointer opacity-0"
                type="file"
                id="file-upload"
                onchange={handleFileSelect}
                onclick={() => (uploadComplete = false)}
            />
            <div class="flex flex-col items-center justify-center">
                <UploadIcon size="48" class="mb-4 text-zinc-400" />
                <p class="text-lg text-zinc-400">Drop a file here or click to browse</p>
            </div>
        </div>
    </div>

    <div class="mt-6">
        <div class="space-y-3">
            {#if selectedFile !== null}
                <div class="flex items-center justify-between rounded-lg bg-zinc-100 p-3">
                    <div class="flex items-center">
                        <FileIcon size="20" class="mr-3 text-zinc-500" />
                        <div>
                            <p class="text-sm font-medium text-gray-700">{selectedFile.name}</p>
                            <p class="text-xs text-gray-500">{fileSize(selectedFile.size)}</p>
                        </div>
                    </div>
                    <button class="text-zinc-500 hover:cursor-pointer hover:text-zinc-700" onclick={removeFile}>
                        <XIcon size="20" />
                    </button>
                </div>
            {/if}
        </div>
    </div>

    <div class="mt-6 mb-6">
        {#if selectedFile !== null}
            <input
                class="flex w-full rounded-lg border border-zinc-300 p-2 focus:outline-none"
                type="text"
                placeholder="Name"
                bind:value={name}
            />
        {/if}
    </div>

    <div class="">
        {#if tags.length > 0}
            <div class="mt-6 flex flex-wrap gap-2">
                {#each tags as tag, i (i)}
                    <button
                            class="flex items-center rounded-lg bg-zinc-200 px-2 py-1 text-sm font-semibold
                             text-zinc-700 transition-colors hover:cursor-pointer hover:bg-red-300"
                            title="Remove tag"
                            onclick={() => removeTag(i)}>{tag}</button
                    >
                {/each}
            </div>
        {/if}

        {#if selectedFile !== null}
            <div class="mt-6 flex items-center">
                <input
                    class="flex-grow rounded-lg border border-zinc-300 p-2 focus:outline-none"
                    type="text"
                    bind:value={tagInput}
                    onkeydown={handleKeydown}
                    placeholder="Tag"
                />
                <button class="ml-2 rounded-lg bg-zinc-200 px-4 py-2 text-zinc-700 transition-colors
                    hover:cursor-pointer hover:bg-zinc-300" onclick={addTag}>Add</button>
            </div>
        {/if}

    </div>

    <div class="mt-6">
        <button
            class="flex w-full items-center justify-center rounded-lg bg-blue-600 py-2 text-white transition-colors
                {!selectedFile || uploading ? 'cursor-not-allowed opacity-50' : 'cursor-pointer hover:bg-blue-700'}"
            onclick={handleUpload}
            disabled={uploading || selectedFile === null}
        >
            {#if uploading}
                <div class="mr-2 h-5 w-5 animate-spin rounded-full border-2 border-white border-t-transparent"></div>
                Uploading...
            {:else}
                Upload File
            {/if}
        </button>
    </div>

    {#if uploadComplete}
        <div class="mt-4 rounded-lg bg-green-100 p-3 text-center text-green-700">File uploaded successfully!</div>
    {/if}
</div>
