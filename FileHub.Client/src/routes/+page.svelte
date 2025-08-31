<script lang="ts">
    import { onMount } from 'svelte';
    import NavBar from '$lib/components/NavBar.svelte';
    import FileViewGrid from '$lib/components/FileViewGrid.svelte';
    import { fileService } from '$lib/services/fileService';
    import type { FileMetaData } from '$lib/models/file';

    let fileMetaData = $state<FileMetaData[]>([]);

    async function load(pageNumber: number, count: number) {
        let result = await fileService.getPaginatedFiles(pageNumber, count);

        fileMetaData = result === null ? [] : result;
    }

    onMount(() => {
        load(1, 50);
    });
</script>

<svelte:head>
    <title>Home | FileHub</title>
</svelte:head>

<NavBar />

<FileViewGrid {fileMetaData} />
