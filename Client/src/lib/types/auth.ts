export type Role =
  | "user"
  | "superuser"
  | "admin"
  | "superadmin";

export type FrontendUser = {
  id: string;
  userName: string;
  displayName: string;
  email: string;
  emailConfirmed: boolean;
  roles: Role[];
  notifyOnNewPost: boolean;
  requiresUsernameSetup: boolean;
};