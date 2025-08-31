<script lang="ts">
    import type { FileMetaData } from '$lib/models/file';
    import FileView from '$lib/components/FileView.svelte';

    let { fileMetaData }: { fileMetaData: FileMetaData[] } = $props();
</script>

<div class="my-4 grid w-full" style="grid-template-columns: repeat(auto-fit, minmax(250px, 1fr))">
    {#each fileMetaData as file (file.id)}
        <div class="flex justify-center">
            <FileView fileMetaData={file} />
        </div>
    {/each}
</div>
