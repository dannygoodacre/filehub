<script lang="ts">
    import { onMount } from 'svelte';
    import NavBar from '$lib/components/NavBar.svelte';
    import FileViewGrid from '$lib/components/FileViewGrid.svelte';
    import { fileService } from '$lib/services/fileService';
    import type { FileMetaData } from '$lib/models/file';
    import { ChevronLeftIcon, ChevronRightIcon } from 'svelte-feather-icons';

    onMount(() => {
        load(1, pageSize);
    });

    const pageSize = 12;
    const maxVisible = 5;

    let fileMetaData = $state<FileMetaData[]>([]);

    let totalPageCount = $state<number>()!;
    let pageNumber = $state(1);
    
    let buttonLabels = $state<number[]>();
    let visibleButtonLabels = $state<number[]>();

    async function load(pageNumber: number, count: number) {
        fileMetaData = (await fileService.getPaginatedFiles(pageNumber, count))!;

        totalPageCount = (await fileService.getPageCount(count))!;
        
        buttonLabels = Array.from({ length: totalPageCount }, (_, i) => i);

        update();
    }

    async function update() {
        fileMetaData = (await fileService.getPaginatedFiles(pageNumber, pageSize))!;

        const start = Math.max(
          0,
          Math.min(totalPageCount - maxVisible, pageNumber - Math.floor(maxVisible / 2) - 1)
        );

        const end = Math.min(totalPageCount!, start + maxVisible);

        visibleButtonLabels = buttonLabels!.slice(start, end);
    }

    async function prevPage()
    {
        pageNumber--;

        update();
    }

    async function gotoPage(n: number)
    {
        pageNumber = n;
        
        update();
    }

    async function nextPage()
    {
        pageNumber++;
        
        update();
    }
</script>

<svelte:head>
    <title>Home | FileHub</title>
</svelte:head>

<NavBar />

<FileViewGrid {fileMetaData} />

<div class="my-8 fixed bottom-0 left-0 right-0 flex items-center justify-center space-x-4 text-gray-700">
    <button
        class="rounded-full px-3 py-1 transition-colors hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed"
        onclick={prevPage}
        disabled={pageNumber === 1}
    >
        <ChevronLeftIcon class="h-full w-full text-zinc-600" />
    </button>

    <div class="flex space-x-2">
        {#each visibleButtonLabels! as i}
            <button
            class="rounded {i + 1 == pageNumber ? "bg-zinc-900" : "bg-zinc-700"} px-2 py-1 text-zinc-100 {i + 1 == pageNumber ? "" : "hover:bg-zinc-400 hover:cursor-pointer"} w-10"
                onclick={() => gotoPage(i + 1)}
            >
                {i + 1}
            </button>
        {/each}
    </div>

    <button
        class="rounded-full px-3 py-1 transition-colors hover:bg-gray-200 disabled:opacity-50 disabled:cursor-not-allowed hover:cursor-pointer"
        onclick={nextPage}
        disabled={pageNumber === totalPageCount}
    >
        <ChevronRightIcon class="h-full w-full text-zinc-600" />
    </button>
</div>
