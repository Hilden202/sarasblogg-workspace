import { writable } from "svelte/store";
import type { FrontendUser, Role } from "$lib/types/auth";

const roleHierarchy: Record<Role, number> = {
  user: 1,
  superuser: 2,
  admin: 3,
  superadmin: 4
};

function createAuthStore() {
  const { subscribe, set } = writable<{
    user: FrontendUser | null;
    isLoading: boolean;
  }>({
    user: null,
    isLoading: true
  });

  function setUser(user: FrontendUser | null) {
    set({
      user,
      isLoading: false
    });
  }

  function clear() {
    set({
      user: null,
      isLoading: false
    });
  }

  function getCurrentUser(): FrontendUser | null {
    let current: FrontendUser | null = null;
    const unsubscribe = subscribe(state => {
      current = state.user;
    });
    unsubscribe();
    return current;
  }

  function hasMinRole(required: Role): boolean {
    const user = getCurrentUser();
    if (!user) return false;

    const highestLevel = Math.max(
      ...user.roles.map(r => roleHierarchy[r])
    );

    return highestLevel >= roleHierarchy[required];
  }

  function hasRole(role: Role): boolean {
    const user = getCurrentUser();
    if (!user) return false;
    return user.roles.includes(role);
  }

  function hasAnyRole(roles: Role[]): boolean {
    const user = getCurrentUser();
    if (!user) return false;
    return roles.some(r => user.roles.includes(r));
  }

  return {
    subscribe,
    setUser,
    clear,
    hasRole,
    hasAnyRole,
    hasMinRole
  };
}

export const auth = createAuthStore();