<script lang="ts">
    import { SearchIcon, XIcon } from 'svelte-feather-icons';

    let { query = $bindable(''), onSearch } = $props<{
        query: string;
        onSearch: () => void;
    }>();

    function handleKeydown(event: KeyboardEvent) {
        if (event.key === 'Enter') {
            onSearch();
        }
    }
</script>

<div class="mx-auto w-full max-w-xl">
    <div class="relative flex items-center">
        <div class="absolute inset-y-0 left-0 flex items-center pl-3">
            <button
                class="text-zinc-400 transition-colors hover:cursor-pointer hover:text-zinc-600 focus:outline-none"
                title="Search"
                onclick={onSearch}
            >
                <SearchIcon size="20" />
            </button>
        </div>

        <input
            type="text"
            class="w-full rounded-lg border border-zinc-300 bg-white py-2 pr-10 pl-10 focus:outline-none"
            placeholder="Tag"
            bind:value={query}
            onkeydown={handleKeydown}
        />

        {#if query}
            <div class="absolute inset-y-0 right-0 flex items-center pr-3">
                <button
                    class="text-zinc-400 transition-colors hover:cursor-pointer hover:text-zinc-600 focus:outline-none"
                    title="Clear search"
                    onclick={() => (query = '')}
                >
                    <XIcon size="20" />
                </button>
            </div>
        {/if}
    </div>
</div>
