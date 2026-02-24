import { getCurrentUser } from '../services/authService';

export const load = async ({ fetch }) => {
  const user = await getCurrentUser(fetch);
  return { user };
};