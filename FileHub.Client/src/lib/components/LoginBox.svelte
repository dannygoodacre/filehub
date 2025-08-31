<script lang="ts">
    import { authStore } from '$lib/stores/authStore';

    let username = $state('');
    let password = $state('');

    let isLoading = $state(false);
    let hasSubmitted = $state(false);
    let isSuccessfulLogin = $state(false);

    let isUsernameEmpty = $derived(!username);
    let isPasswordEmpty = $derived(!password);

    let showErrors = $derived(!isLoading && hasSubmitted && !isSuccessfulLogin);

    let showMissingUsernameError = $derived(showErrors && isUsernameEmpty);
    let showMissingPasswordError = $derived(showErrors && isPasswordEmpty);

    let showLoginFailedError = $derived(showErrors && !isUsernameEmpty && !isPasswordEmpty);

    async function handleLogin() {
        if (isUsernameEmpty && isPasswordEmpty) {
            return;
        }

        hasSubmitted = true;

        isLoading = true;

        isSuccessfulLogin = await authStore.login({ username, password});

        isLoading = false;

        if (isSuccessfulLogin) {
            username = '';
            password = '';
        }
    }
</script>

<div class="w-full max-w-xs">
    <form class="mb-4 rounded bg-white px-8 pt-6 pb-8 shadow-sm" onsubmit={handleLogin}>
        <h1 class="mb-6 text-2xl font-semibold text-gray-800">Login</h1>

        <div class="mb-4">
            <label class="mb-2 block text-sm font-bold text-gray-700" for="username">Username</label>

            <input
                class="appearance-none border {showMissingUsernameError ? 'border-red-500' : 'border-zinc-300'}
                          focus:shadow-outline w-full rounded-lg px-3 py-2 leading-tight focus:outline-none"
                id="username"
                type="text"
                placeholder="Username"
                bind:value={username}
                oninput={() => (hasSubmitted = false)}
            />

            {#if showMissingUsernameError}
                <p class="text-xs text-red-500 italic">Please enter a username.</p>
            {/if}
        </div>

        <div class="mb-4">
            <label class="mb-2 block text-sm font-bold text-gray-700" for="password">Password</label>
            <input
                class="appearance-none border {showMissingPasswordError ? 'border-red-500' : 'border-zinc-300'}
                          focus:shadow-outline w-full rounded-lg px-3 py-2 leading-tight focus:outline-none"
                id="password"
                type="password"
                placeholder="**********"
                bind:value={password}
                oninput={() => (hasSubmitted = false)}
            />

            {#if showMissingPasswordError}
                <p class="text-xs text-red-500 italic">Please enter a password.</p>
            {/if}
        </div>

        <button class="mt-2 w-full rounded-lg bg-blue-600 py-2 text-white transition-colors hover:cursor-pointer hover:bg-blue-700" type="submit">
            Log in
        </button>
    </form>

    {#if isSuccessfulLogin}
        <div class="alert border-green-400 bg-green-100 text-green-700" role="alert">
            <span class="block sm:inline">Login successful</span>
        </div>
    {/if}

    {#if showLoginFailedError}
        <div class="alert border-red-400 bg-red-100 text-red-700" role="alert">
            <span class="block sm:inline">Login failed</span>
        </div>
    {/if}
</div>
