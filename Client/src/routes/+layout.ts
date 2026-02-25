import { getCurrentUser, mapToFrontendUser } from "$lib/services/authService";
import { auth } from "$lib/stores/auth";

export const load = async ({ fetch }) => {
  const me = await getCurrentUser(fetch);

  if (!me) {
    auth.clear();
    return {};
  }

  const frontendUser = mapToFrontendUser(me);
  auth.setUser(frontendUser);

  return {};
};