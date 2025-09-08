<script lang="ts">
    import type { FileMetaData } from '$lib/models/file';
    import { FileIcon } from 'svelte-feather-icons';
    import FileThumbnail from './FileThumbnail.svelte';

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
        <FileThumbnail {fileMetaData} />
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
