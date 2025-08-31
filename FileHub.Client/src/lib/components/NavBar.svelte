<script>
    import { authStore } from '$lib/stores/authStore';
    import { page } from '$app/state';

    let isLoginPage = $derived(page.url.pathname.includes('/login'));
</script>

<nav class="w-full bg-zinc-900 px-6 py-2 shadow-md">
    <div class="mx-auto flex items-center justify-between">
        <div class="flex items-center">
            <a href={$authStore.isLoggedIn ? '/' : '/login'} class="text-primary text-3xl font-bold text-zinc-100">FileHub</a>
        </div>

        {#if $authStore.isLoggedIn}
            <div class="flex space-x-6">
                <a href="/" class="text-zinc-700 transition-colors hover:text-zinc-400">Home</a>
                <a href="/upload" class="text-zinc-700 transition-colors hover:text-zinc-400">Upload</a>
            </div>
        {/if}

        <div class="right">
            <div class="flex items-center gap-4">
                {#if $authStore.isLoggedIn}
                    <p class="font-semibold text-zinc-100">{$authStore.username || 'USER'}</p>
                    <button class="rounded bg-zinc-800 px-2 py-1 text-zinc-100 transition-colors hover:bg-zinc-700"
                        onclick={async () => await authStore.logout()}>Log out</button>
                {:else if !isLoginPage}
                    <a class="rounded bg-zinc-800 px-2 py-1 text-zinc-100 hover:bg-zinc-700" href="/login">Log In</a>
                {/if}
            </div>
        </div>
    </div>
</nav>
