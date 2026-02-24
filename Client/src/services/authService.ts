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