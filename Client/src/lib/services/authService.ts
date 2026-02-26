export type MeResponse = {
  id: string;
  userName: string;
  email?: string | null;
  name?: string | null;
  birthYear?: number | null;
  emailConfirmed: boolean;
  phoneNumber?: string | null;
  roles: string[];
  notifyOnNewPost: boolean;
  requiresUsernameSetup: boolean;
};

import type { FrontendUser, Role } from '$lib/types/auth';

export function mapToFrontendUser(me: MeResponse): FrontendUser {
  return {
    id: me.id,
    userName: me.userName,
    displayName: me.name ?? me.userName,
    email: me.email ?? '',
    emailConfirmed: me.emailConfirmed,
    roles: me.roles as Role[],
    notifyOnNewPost: me.notifyOnNewPost,
    requiresUsernameSetup: me.requiresUsernameSetup
  };
}

export async function getCurrentUser(
  fetchFn: typeof fetch
): Promise<MeResponse | null> {

  const response = await fetchFn(
    '/api/users/me',
    {
      method: "GET",
      credentials: "include",
    }
  );

  if (response.status === 401) return null;

  if (!response.ok) {
    throw new Error("Failed to fetch current user");
  }

  return await response.json();
}

export async function login(
  fetchFn: typeof fetch,
  userNameOrEmail: string,
  password: string,
  rememberMe: boolean
): Promise<boolean> {

  const response = await fetchFn('/api/auth/login', {
    method: 'POST',
    credentials: 'include',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      userNameOrEmail,
      password,
      rememberMe
    })
  });

  return response.ok;
}

export async function logout(fetchFn: typeof fetch): Promise<void> {
  await fetchFn('/api/auth/logout', {
    method: 'POST',
    credentials: 'include'
  });
}