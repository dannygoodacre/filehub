<script lang="ts">
    import type { FileMetaData } from '$lib/models/file';
    import { FileIcon } from 'svelte-feather-icons';

    let { fileMetaData }: { fileMetaData: FileMetaData } = $props();

    let src = $state(fileMetaData.accessLocation);

    const isImage = fileMetaData.contentType.startsWith('image/');

    let retries = 1;
    const maxRetries = 5;

    function retry() {
        if (retries > maxRetries) {
            return;
        }

        src = `${fileMetaData.accessLocation}?retry=${retries++}`;
    }
</script>

<div class="m-2 w-full max-w-sm overflow-hidden rounded shadow-lg">

    <div class="w-full aspect-[4/3] overflow-hidden">
        <a href={fileMetaData.accessLocation} target="_blank" rel="noopener noreferrer">
            {#if isImage}
                <img class="block h-full w-full object-cover" src={src} alt={fileMetaData.name} onerror={retry}/>
            {:else}
                <div class="flex h-full w-full items-center justify-center bg-zinc-100">
                    <div class="flex h-1/3 w-auto items-center justify-center">
                        <FileIcon size="100%" class="h-full w-full text-zinc-600" />
                    </div>
                </div>
            {/if}
        </a>
    </div>

    <div class="px-4 pt-2">
        <div class="text-secondary text-lg truncate max-w-full" title="{fileMetaData.name}">
            {fileMetaData.name}
        </div>
        <div class="text-sm text-zinc-500 truncate max-w-full">
            {fileMetaData.contentType}
        </div>
    </div>

    <div class="px-3 py-2 flex flex-wrap gap-2">
        {#each fileMetaData.tags as tag (tag)}
            <span class="max-w-[250px] truncate rounded-lg bg-zinc-200 px-2 py-1 text-sm font-semibold text-zinc-700" title="{tag}">
                {tag}
            </span>
        {/each}
    </div>

</div>
