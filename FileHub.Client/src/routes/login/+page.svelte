<script lang="ts">
    import NavBar from '$lib/components/NavBar.svelte';
    import LoginBox from '$lib/components/LoginBox.svelte';
</script>

<svelte:head>
    <title>Login | FileHub</title>
</svelte:head>

<div class="flex h-screen flex-col">
    <NavBar />

    <div class="flex flex-grow items-center justify-center">
        <LoginBox />
    </div>
</div>
