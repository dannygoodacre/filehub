<script lang="ts">
    import NavBar from '$lib/components/NavBar.svelte';
    import UploadBox from '$lib/components/UploadBox.svelte';
</script>

<NavBar />

<svelte:head>
    <title>Upload | FileHub</title>
</svelte:head>

<div class="container mx-auto px-4 py-8">
    <UploadBox />
</div>
