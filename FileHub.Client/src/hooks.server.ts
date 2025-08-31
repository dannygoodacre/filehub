import type { Handle } from '@sveltejs/kit';
import { redirect } from '@sveltejs/kit';

export const handle: Handle = async ({ event, resolve }) => {
    const path = event.url.pathname;
    const publicPaths = ['/login', '/register'];

    const isPublicPath = publicPaths.some((publicPath) => path === publicPath || path.startsWith(publicPath + '/'));

    if (!isPublicPath) {
        const authCookie = event.cookies.get('FileHub');

        if (!authCookie) {
            redirect(303, '/login');
        }
    }

    return resolve(event);
};
