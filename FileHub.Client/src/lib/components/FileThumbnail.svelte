<script lang="ts">
    import type { FileMetaData } from "$lib/models/file";
    import { FileIcon, FileTextIcon, HeadphonesIcon, VideoIcon } from 'svelte-feather-icons';

    let { fileMetaData }: { fileMetaData: FileMetaData } = $props();
    
    const generalType = fileMetaData.contentType.split('/')[0];
    
    let src = $state(fileMetaData.accessLocation);
    
    let retries = 1;
    const maxRetries = 5;

    function retry() {
        if (retries > maxRetries) {
            return;
        }

        src = `${fileMetaData.accessLocation}?retry=${retries++}`;
    }
</script>

<a href={fileMetaData.accessLocation} target="_blank" rel="noopener noreferrer">
    {#if generalType === 'image'}
        <img class="block h-full w-full object-cover" src={src} alt={fileMetaData.name} onerror={retry}/>
    {:else}
        <div class="flex h-full w-full items-center justify-center bg-zinc-100">
            <div class="flex h-1/3 w-auto items-center justify-center">
                {#if generalType === 'audio'}
                    <HeadphonesIcon size="100%" class="h-full w-full text-zinc-600" />
                {:else if generalType === 'video'}
                    <VideoIcon size="100%" class="h-full w-full text-zinc-600" />
                {:else if generalType === 'text'}
                    <FileTextIcon size="100%" class="h-full w-full text-zinc-600" />
                {:else}
                    <FileIcon size="100%" class="h-full w-full text-zinc-600" />
                {/if}
            </div>
        </div>
    {/if}
</a>
